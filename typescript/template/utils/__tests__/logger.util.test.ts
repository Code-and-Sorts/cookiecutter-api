import { describe, it, expect, afterEach, jest } from '@jest/globals';
import { createServer } from 'node:http';
import { AddressInfo } from 'node:net';
{%- if cloud_service == 'Azure Function App' %}
import { AzureMonitorLogExporter } from '@azure/monitor-opentelemetry-exporter';
{%- endif %}
import { context } from '@opentelemetry/api';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-proto';
import { ConsoleJsonExporter, createLoggerProvider, detectError, logExporters, logger, RESOURCE, traceContext } from '@utils';

const TRACE_ID = '4bf92f3577b34da6a3ce929d0e0e4736';
const SPAN_ID = '00f067aa0ba902b7';
const TRACEPARENT = `00-${TRACE_ID}-${SPAN_ID}-01`;
const OTLP = { OTEL_EXPORTER_OTLP_ENDPOINT: 'http://localhost:4318' };
{%- if cloud_service == 'Azure Function App' %}
const APP_INSIGHTS = { APPLICATIONINSIGHTS_CONNECTION_STRING: 'InstrumentationKey=00000000-0000-0000-0000-000000000000' };
{%- endif %}

const lines = (spy: jest.SpiedFunction<typeof console.log>) =>
    spy.mock.calls.map(([line]) => JSON.parse(line as string) as Record<string, any>);

afterEach(() => {
    jest.restoreAllMocks();
    delete process.env.OTEL_SERVICE_NAME;
    delete process.env.OTEL_RESOURCE_ATTRIBUTES;
    delete process.env.OTEL_EXPORTER_OTLP_ENDPOINT;
});

describe('logExporters', () => {
    it.each([
        { env: {}, kinds: [] },
        { env: { OTEL_EXPORTER_OTLP_ENDPOINT: '' }, kinds: [] },
        { env: OTLP, kinds: [OTLPLogExporter] },
        { env: { OTEL_EXPORTER_OTLP_LOGS_ENDPOINT: 'http://localhost:4318/v1/logs' }, kinds: [OTLPLogExporter] },
{%- if cloud_service == 'Azure Function App' %}
        { env: APP_INSIGHTS, kinds: [AzureMonitorLogExporter] },
        { env: { ...APP_INSIGHTS, ...OTLP }, kinds: [AzureMonitorLogExporter, OTLPLogExporter] },
{%- endif %}
    ])('should pick the exporters $env asks for', ({ env, kinds }) => {
        expect(logExporters(env).map((exporter) => exporter.constructor)).toEqual(kinds);
    });
});

describe('createLoggerProvider', () => {
    it('should write one JSON line per record to the console when nothing exports', () => {
        const log = jest.spyOn(console, 'log').mockImplementation(() => undefined);
        const error = jest.spyOn(console, 'error').mockImplementation(() => undefined);
        const provider = createLoggerProvider({});

        context.with(traceContext({ traceparent: TRACEPARENT }), () => {
            provider.getLogger('test').emit({ severityNumber: 9, severityText: 'INFO', body: 'served' });
            provider.getLogger('test').emit({ severityNumber: 17, severityText: 'ERROR', body: 'failed' });
        });

        expect(lines(log)).toEqual([expect.objectContaining({ body: 'served', severityText: 'INFO', traceId: TRACE_ID, spanId: SPAN_ID })]);
        expect(lines(error)).toEqual([expect.objectContaining({ body: 'failed', severityText: 'ERROR' })]);
        expect(lines(log)[0].resource).toMatchObject(RESOURCE);
    });

    it('should let the environment override the resource', () => {
        process.env.OTEL_SERVICE_NAME = 'renamed';
        process.env.OTEL_RESOURCE_ATTRIBUTES = 'deployment.environment.name=dev';
        const log = jest.spyOn(console, 'log').mockImplementation(() => undefined);

        createLoggerProvider({}).getLogger('test').emit({ severityNumber: 9, body: 'served' });

        expect(lines(log)[0].resource).toMatchObject({
            'service.name': 'renamed',
            'deployment.environment.name': 'dev',
            'cloud.provider': RESOURCE['cloud.provider'],
        });
    });

    it('should send records over OTLP instead of the console when an endpoint is set', async () => {
        const paths: string[] = [];
        const server = createServer((req, res) => {
            paths.push(req.url ?? '');
            req.resume().on('end', () => res.end());
        });
        await new Promise<void>((resolve) => server.listen(0, '127.0.0.1', resolve));
        process.env.OTEL_EXPORTER_OTLP_ENDPOINT = `http://127.0.0.1:${(server.address() as AddressInfo).port}`;
        const log = jest.spyOn(console, 'log').mockImplementation(() => undefined);
        const provider = createLoggerProvider(process.env);

        provider.getLogger('test').emit({ severityNumber: 9, body: 'served' });
        await provider.forceFlush();

        expect(paths).toEqual(['/v1/logs']);
        expect(log).not.toHaveBeenCalled();
        await provider.shutdown();
        server.close();
    });
});

describe('ConsoleJsonExporter', () => {
    it('should have nothing to flush or shut down', async () => {
        const exporter = new ConsoleJsonExporter();
        await expect(exporter.forceFlush()).resolves.toBeUndefined();
        await expect(exporter.shutdown()).resolves.toBeUndefined();
    });
});

describe('traceContext', () => {
    it.each(['traceparent', 'Traceparent', 'TRACEPARENT'])('should read the %s header', (name) => {
        const log = jest.spyOn(console, 'log').mockImplementation(() => undefined);

        context.with(traceContext({ [name]: TRACEPARENT }), () => logger.info('served'));

        expect(lines(log)[0]).toMatchObject({ traceId: TRACE_ID, spanId: SPAN_ID });
    });

    it('should leave a request without the header outside any trace', () => {
        const log = jest.spyOn(console, 'log').mockImplementation(() => undefined);

        context.with(traceContext(undefined), () => logger.warn('slow', { attempt: 2 }));

        expect(lines(log)[0]).toMatchObject({ severityText: 'WARN', body: 'slow', attributes: { attempt: 2 } });
        expect(lines(log)[0].traceId).toBeUndefined();
    });
});

describe('unexpected errors', () => {
    it('should emit an error record with the exception and the trace', () => {
        const error = jest.spyOn(console, 'error').mockImplementation(() => undefined);

        context.with(traceContext({ traceparent: TRACEPARENT }), () => detectError(new TypeError('secret details')));

        const [record] = lines(error);
        expect(record).toMatchObject({
            severityText: 'ERROR',
            body: 'Unexpected error while handling the request.',
            traceId: TRACE_ID,
            spanId: SPAN_ID,
            attributes: { 'exception.type': 'TypeError', 'exception.message': 'secret details' },
        });
        expect(record.attributes['exception.stacktrace']).toContain('TypeError: secret details');
    });

    it('should describe a thrown value that is not an Error', () => {
        const error = jest.spyOn(console, 'error').mockImplementation(() => undefined);

        detectError('boom');

        expect(lines(error)[0].attributes).toEqual({ 'exception.message': 'boom' });
    });
});
