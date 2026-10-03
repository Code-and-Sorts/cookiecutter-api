import { z } from 'zod';

// The value formats every language checks the same way.
export const DATE_TIME_PATTERN = /^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]{1,9})?(Z|[+-][0-9]{2}:[0-9]{2})$/;
export const UUID_PATTERN = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
export const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
export const URI_PATTERN = /^[A-Za-z][A-Za-z0-9+.-]*:\S+$/;

export const dateSchema = z.iso.date();

// Any RFC 3339 offset is accepted; the value is held in UTC with milliseconds, like the system timestamps.
export const dateTimeSchema = z.iso
    .datetime({ offset: true })
    .regex(DATE_TIME_PATTERN)
    .transform((value) => new Date(value).toISOString());

export const uuidSchema = z.string().regex(UUID_PATTERN, 'must be a UUID');

export const hasUniqueItems = (values: unknown[]): boolean => new Set(values).size === values.length;
