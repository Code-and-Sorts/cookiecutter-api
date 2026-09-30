import { BaseError } from './base.error';

export class NotFoundError extends BaseError {
  constructor(message: string, asserter?: Function) {
    super(message, asserter);
    this.name = 'NotFoundError';
    this.statusCode = 404;
  }

  static forItem(resource: string, id: string): NotFoundError {
    return new NotFoundError(`${resource} with id ${id} was not found.`);
  }
}
