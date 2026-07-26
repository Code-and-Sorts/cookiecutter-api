import { BaseError } from '../types/errors/base.error';

export const detectError = <T>(error: T): { status: number; body: string } => {
  if (error instanceof BaseError && error.statusCode !== undefined) {
    return {
      status: error.statusCode,
      body: error.message,
    };
  }
  return {
    status: 500,
    body: 'Unknown error occurred.',
  };
};
