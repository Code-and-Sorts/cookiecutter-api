import { BaseError } from '../types/errors/base.error';

export const UNEXPECTED_ERROR_MESSAGE = 'An unexpected error occurred.';

export type ErrorResponse = { status: number; body: { errorMessage: string } };

// Maps an error to a JSON error response. Expected client errors (4xx) carry their message;
// anything else is logged with its stack trace and answered with a generic 500, so exception
// text and SDK diagnostics never reach the client.
export const detectError = (
  error: unknown,
  logError: (...args: unknown[]) => void = console.error,
): ErrorResponse => {
  if (error instanceof BaseError && error.statusCode !== undefined && error.statusCode < 500) {
    return {
      status: error.statusCode,
      body: { errorMessage: error.message },
    };
  }
  logError('Unexpected error while handling the request.', error);
  return {
    status: 500,
    body: { errorMessage: UNEXPECTED_ERROR_MESSAGE },
  };
};
