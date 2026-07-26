import { BaseError } from './base.error';

export class ValidationError extends BaseError {
  constructor(message: string) {
    super(message);
    this.name = 'ValidationError';
    this.stack = new Error().stack;
    this.statusCode = 422;
  }
}
