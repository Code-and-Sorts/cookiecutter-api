import { ValidationError } from '../types/errors/validation.error';

export const INVALID_JSON_MESSAGE = 'Request body must be valid JSON.';

// Parses a raw request body. A missing, empty or malformed body is a client error (400),
// never a 500; the schema then checks that the parsed value is an object.
export const parseJsonBody = (raw?: string | null): unknown => {
  if (raw === undefined || raw === null || raw.trim() === '') {
    throw new ValidationError(INVALID_JSON_MESSAGE);
  }
  try {
    return JSON.parse(raw);
  } catch {
    throw new ValidationError(INVALID_JSON_MESSAGE);
  }
};
