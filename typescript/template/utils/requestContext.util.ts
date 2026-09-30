import { AsyncLocalStorage } from 'node:async_hooks';

// Per-request context carried through async calls, so singletons (repositories) can see
// whether the client that made the current request has gone away.
type RequestContext = { signal: AbortSignal };

const storage = new AsyncLocalStorage<RequestContext>();

// Runs fn with signal as the current request's cancellation signal.
export const runWithSignal = <T>(signal: AbortSignal, fn: () => T): T => storage.run({ signal }, fn);

// The current request's cancellation signal, if the entry point provides one.
export const currentSignal = (): AbortSignal | undefined => storage.getStore()?.signal;

// The error used when a request is cancelled; detectError logs it as a warning only.
export const cancellationError = (): Error =>
  Object.assign(new Error('The request was cancelled by the client.'), { name: 'AbortError', code: 'ABORT_ERR' });
