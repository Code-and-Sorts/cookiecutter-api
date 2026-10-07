import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import { context, trace } from '@opentelemetry/api';
import { loggerProvider } from '@utils';
import { mockFunctionsApp } from '../../test/mocks';

jest.unstable_mockModule('@azure/functions', () => ({ app: mockFunctionsApp() }));

let app: ReturnType<typeof mockFunctionsApp>;

beforeAll(async () => {
    ({ app } = (await import('@azure/functions')) as unknown as { app: ReturnType<typeof mockFunctionsApp> });
    await import('../telemetry');
});

describe('Functions telemetry', () => {
    it('should tell the host the worker exports its own logs', () => {
        expect(app.setup).toHaveBeenCalledWith({ capabilities: { WorkerOpenTelemetryEnabled: true } });
    });

    it.each([
        { traceContext: { traceParent: '00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01' }, traceId: '4bf92f3577b34da6a3ce929d0e0e4736' },
        { traceContext: undefined, traceId: undefined },
    ])('should run each invocation in the trace the host started ($traceId)', ({ traceContext, traceId }) => {
        const [[preInvocation]] = app.hook.preInvocation.mock.calls as [[(hook: Record<string, any>) => void]];
        const hook: Record<string, any> = {
            invocationContext: { traceContext },
            functionHandler: () => trace.getSpanContext(context.active())?.traceId,
        };

        preInvocation(hook);

        expect(hook.functionHandler()).toEqual(traceId);
    });

    it('should shut the logger provider down when the worker stops', async () => {
        const shutdown = jest.spyOn(loggerProvider, 'shutdown').mockResolvedValue(undefined);
        const [[appTerminate]] = app.hook.appTerminate.mock.calls as [[() => Promise<void>]];

        await appTerminate();

        expect(shutdown).toHaveBeenCalledTimes(1);
    });
});
