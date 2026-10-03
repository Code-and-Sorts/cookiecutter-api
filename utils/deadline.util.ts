import { ProxyError } from '../types/errors/proxy.error';
import { cancellationError, currentSignal } from './requestContext.util';

// Bounds a whole repository operation, SDK retries included, so a failing database gets its
// 500 well within 10 seconds and every platform timeout.
export const DATABASE_DEADLINE_MS = 8000;
export const DATABASE_ATTEMPT_TIMEOUT_MS = 3000;

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
