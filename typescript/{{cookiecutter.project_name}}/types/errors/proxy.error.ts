import { BaseError } from './base.error';

export class ProxyError extends BaseError {
  constructor(message: string) {
    super(message);
    this.name = 'ProxyError';
    this.stack = new Error().stack;
    this.statusCode = 502;
  }
}
