import { ValidationError } from '../types/errors/validation.error';

export const USER_ID_HEADER = 'x-user-id';
export const MAX_USER_ID_LENGTH = 256;
export const USER_ID_TOO_LONG_MESSAGE = `X-User-Id must be at most ${MAX_USER_ID_LENGTH} characters.`;

export const parseUserId = (raw?: string | null): string | undefined => {
  const userId = raw?.trim();
  if (userId && userId.length > MAX_USER_ID_LENGTH) {
    throw new ValidationError(USER_ID_TOO_LONG_MESSAGE);
  }
  return userId || undefined;
};
