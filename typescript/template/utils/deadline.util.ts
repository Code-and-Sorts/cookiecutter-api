import { ProxyError } from '../types/errors/proxy.error';
import { cancellationError, currentSignal } from './requestContext.util';

// Upper bound for one repository operation (every database call it makes, SDK retries
// included). A failing database then ends in the generic 500 well within 10 seconds and
// every platform timeout. The SDK clients in config/container.ts also cap their own
// per-attempt timeouts and retries (see DATABASE_ATTEMPT_TIMEOUT_MS) so work stops early too.
export const DATABASE_DEADLINE_MS = 8000;
export const DATABASE_ATTEMPT_TIMEOUT_MS = 3000;

// Resolves or rejects like operation; rejects with a ProxyError once ms have passed, or with
// an AbortError as soon as the current request is cancelled (see requestContext.util.ts).
export const withDeadline = <T>(operation: Promise<T>, ms: number = DATABASE_DEADLINE_MS): Promise<T> => {
  const signal = currentSignal();
  if (signal?.aborted) {
    operation.catch(() => undefined);
    return Promise.reject(cancellationError());
  }
  let timer: NodeJS.Timeout | undefined;
  let onAbort: (() => void) | undefined;
  const deadline = new Promise<never>((_, reject) => {
    timer = setTimeout(() => reject(new ProxyError(`Database operation timed out after ${ms} ms.`)), ms);
    if (signal) {
      onAbort = () => reject(cancellationError());
      signal.addEventListener('abort', onAbort, { once: true });
    }
  });
  return Promise.race([operation, deadline]).finally(() => {
    clearTimeout(timer);
    if (signal && onAbort) {
      signal.removeEventListener('abort', onAbort);
    }
  });
};
