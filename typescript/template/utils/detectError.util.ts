import { BaseError } from '../types/errors/base.error';

export const UNEXPECTED_ERROR_MESSAGE = 'An unexpected error occurred.';
export const NOT_FOUND_MESSAGE = 'Not found.';
export const METHOD_NOT_ALLOWED_MESSAGE = 'Method not allowed.';

export const isCancellation = (error: unknown): boolean =>
  error instanceof Error && (error.name === 'AbortError' || (error as { code?: unknown }).code === 'ABORT_ERR');

export type ErrorResponse = { status: number; body: { errorMessage: string } };

// Anything but a 4xx gets a generic 500 so exception text and SDK diagnostics never reach the client.
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
  if (isCancellation(error)) {
    // A client that went away is not an application failure.
    console.warn('Request was cancelled before it completed.');
  } else {
    logError('Unexpected error while handling the request.', error);
  }
  return {
    status: 500,
    body: { errorMessage: UNEXPECTED_ERROR_MESSAGE },
  };
};
