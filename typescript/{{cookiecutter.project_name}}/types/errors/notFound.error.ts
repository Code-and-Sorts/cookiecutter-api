import { BaseError } from './base.error';

export class NotFoundError extends BaseError {
  constructor(message: string, asserter?: Function) {
    super(message, asserter);
    this.name = 'NotFoundError';
    this.stack = new Error().stack;
    this.statusCode = 404;
  }
}
