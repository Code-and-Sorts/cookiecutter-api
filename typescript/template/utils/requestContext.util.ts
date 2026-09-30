import { AsyncLocalStorage } from 'node:async_hooks';

// Lets singleton repositories see whether the current request's client has gone away.
type RequestContext = { signal: AbortSignal };

const storage = new AsyncLocalStorage<RequestContext>();

export const runWithSignal = <T>(signal: AbortSignal, fn: () => T): T => storage.run({ signal }, fn);

export const currentSignal = (): AbortSignal | undefined => storage.getStore()?.signal;

// Named AbortError so detectError logs it only as a warning.
export const cancellationError = (): Error =>
  Object.assign(new Error('The request was cancelled by the client.'), { name: 'AbortError', code: 'ABORT_ERR' });
