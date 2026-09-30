import { ProxyError } from '../types/errors/proxy.error';

// Upper bound for one repository operation (every database call it makes, SDK retries
// included). A failing database then ends in the generic 500 well within 10 seconds and
// every platform timeout. The SDK clients in config/container.ts also cap their own
// per-attempt timeouts and retries (see DATABASE_ATTEMPT_TIMEOUT_MS) so work stops early too.
export const DATABASE_DEADLINE_MS = 8000;
export const DATABASE_ATTEMPT_TIMEOUT_MS = 3000;

// Resolves or rejects like operation, or rejects with a ProxyError once ms have passed.
export const withDeadline = <T>(operation: Promise<T>, ms: number = DATABASE_DEADLINE_MS): Promise<T> => {
  let timer: NodeJS.Timeout | undefined;
  const deadline = new Promise<never>((_, reject) => {
    timer = setTimeout(() => reject(new ProxyError(`Database operation timed out after ${ms} ms.`)), ms);
  });
  return Promise.race([operation, deadline]).finally(() => clearTimeout(timer));
};
