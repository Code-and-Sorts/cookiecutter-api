import { HttpResponseInit } from '@azure/functions';
import { BaseError } from '../types/errors/base.error';

export const detectError = <T>(error: T) => {
  if (error instanceof BaseError && error.statusCode !== undefined) {
    return {
      status: error.statusCode,
      body: error.message,
    } as HttpResponseInit;
  }
  return {
    status: 500,
    body: 'Unknown error occurred.',
  } as HttpResponseInit;
};
