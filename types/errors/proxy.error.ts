import { BaseError } from './base.error';

// `cause` keeps the SDK error for the logs; clients only ever see a generic 500.
export class ProxyError extends BaseError {
  constructor(message: string, cause?: unknown) {
    super(message, undefined, cause === undefined ? undefined : { cause });
    this.name = 'ProxyError';
    this.statusCode = 502;
  }
}
