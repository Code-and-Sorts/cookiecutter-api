{%- set azure = cloud_service == 'Azure Function App' -%}
{%- if azure %}
import { AzureMonitorLogExporter } from '@azure/monitor-opentelemetry-exporter';
{%- endif %}
import { context, propagation, ROOT_CONTEXT, type Context } from '@opentelemetry/api';
import { logs, SeverityNumber, type LogAttributes } from '@opentelemetry/api-logs';
import { AsyncLocalStorageContextManager } from '@opentelemetry/context-async-hooks';
import { ExportResultCode, hrTimeToTimeStamp, W3CTraceContextPropagator, type ExportResult } from '@opentelemetry/core';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-proto';
import { defaultResource, detectResources, envDetector, resourceFromAttributes } from '@opentelemetry/resources';
import {
  BatchLogRecordProcessor,
  LoggerProvider,
  SimpleLogRecordProcessor,
  type LogRecordExporter,
  type LogRecordProcessor,
  type ReadableLogRecord,
} from '@opentelemetry/sdk-logs';

type Environment = Record<string, string | undefined>;
type Headers = Record<string, string | string[] | undefined> | null | undefined;

export const RESOURCE = {
{%- for key, value in telemetry_resource | dictsort %}
  '{{ key }}': '{{ value }}',
{%- endfor %}
};

// One JSON line per record keeps console logs structured where the platform reads stdout and stderr line by line.
export class ConsoleJsonExporter implements LogRecordExporter {
  export(records: ReadableLogRecord[], done: (result: ExportResult) => void): void {
    for (const record of records) {
      const line = JSON.stringify({
        timestamp: hrTimeToTimeStamp(record.hrTime),
        severityText: record.severityText,
        body: record.body,
        attributes: record.attributes,
        traceId: record.spanContext?.traceId,
        spanId: record.spanContext?.spanId,
        resource: record.resource.attributes,
      });
      if ((record.severityNumber ?? SeverityNumber.UNSPECIFIED) >= SeverityNumber.ERROR) {
        console.error(line);
      } else {
        console.log(line);
      }
    }
    done({ code: ExportResultCode.SUCCESS });
  }

  async shutdown(): Promise<void> {}

  async forceFlush(): Promise<void> {}
}

export const logExporters = (env: Environment): LogRecordExporter[] => [
{%- if azure %}
  ...(env.APPLICATIONINSIGHTS_CONNECTION_STRING
    ? [new AzureMonitorLogExporter({ connectionString: env.APPLICATIONINSIGHTS_CONNECTION_STRING })]
    : []),
{%- endif %}
  ...(env.OTEL_EXPORTER_OTLP_ENDPOINT || env.OTEL_EXPORTER_OTLP_LOGS_ENDPOINT ? [new OTLPLogExporter()] : []),
];

export const createLoggerProvider = (env: Environment): LoggerProvider => {
  const exporters = logExporters(env);
  const processors: LogRecordProcessor[] = exporters.length
    ? exporters.map((exporter) => new BatchLogRecordProcessor({ exporter }))
    : [new SimpleLogRecordProcessor({ exporter: new ConsoleJsonExporter() })];
  const resource = defaultResource()
    .merge(resourceFromAttributes(RESOURCE))
    .merge(detectResources({ detectors: [envDetector] }));
  return new LoggerProvider({ resource, processors });
};

context.setGlobalContextManager(new AsyncLocalStorageContextManager().enable());
propagation.setGlobalPropagator(new W3CTraceContextPropagator());
export const loggerProvider = createLoggerProvider(process.env);
logs.setGlobalLoggerProvider(loggerProvider);

// Logs carry the caller's trace and span ids when the request has a traceparent header, in any casing.
export const traceContext = (headers: Headers): Context =>
  propagation.extract(
    ROOT_CONTEXT,
    Object.fromEntries(Object.entries(headers ?? {}).map(([name, value]) => [name.toLowerCase(), value])),
  );

// Named after the OpenTelemetry exception attributes, so log backends show the record as an exception.
const exceptionAttributes = (error: unknown): LogAttributes =>
  error instanceof Error
    ? { 'exception.type': error.name, 'exception.message': error.message, 'exception.stacktrace': error.stack }
    : { 'exception.message': String(error) };

const emit = (severityNumber: SeverityNumber, severityText: string, body: string, attributes?: LogAttributes): void =>
  logs.getLogger('{{ project_endpoint }}').emit({ severityNumber, severityText, body, attributes });

export const logger = {
  info: (message: string, attributes?: LogAttributes): void => emit(SeverityNumber.INFO, 'INFO', message, attributes),
  warn: (message: string, attributes?: LogAttributes): void => emit(SeverityNumber.WARN, 'WARN', message, attributes),
  error: (message: string, error?: unknown, attributes?: LogAttributes): void =>
    emit(SeverityNumber.ERROR, 'ERROR', message, { ...attributes, ...(error === undefined ? {} : exceptionAttributes(error)) }),
};

export type Logger = typeof logger;
