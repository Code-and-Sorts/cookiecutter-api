import { BaseError } from './base.error';

// A failed call to the database. The original SDK error is kept as `cause` so it is
// logged with the stack trace; clients only ever see a generic 500 response.
export class ProxyError extends BaseError {
  constructor(message: string, cause?: unknown) {
    super(message, undefined, cause === undefined ? undefined : { cause });
    this.name = 'ProxyError';
    this.statusCode = 502;
  }
}
