import { app } from '@azure/functions';
import { context } from '@opentelemetry/api';
import { loggerProvider, traceContext } from '@utils';

// The worker exports its own logs, so the Functions host must not export them again.
app.setup({ capabilities: { WorkerOpenTelemetryEnabled: true } });

app.hook.preInvocation((hook) => {
  const { traceParent, traceState } = hook.invocationContext.traceContext ?? {};
  hook.functionHandler = context.bind(traceContext({ traceparent: traceParent, tracestate: traceState }), hook.functionHandler);
});

app.hook.appTerminate(() => loggerProvider.shutdown());
