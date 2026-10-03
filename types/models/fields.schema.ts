import { z } from 'zod';

// The u flag makes JavaScript count code points, as every other language does.
export const DATE_PATTERN = new RegExp("^(000[1-9]|00[1-9][0-9]|0[1-9][0-9]{2}|[1-9][0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$", 'u');
export const DATE_TIME_PATTERN = new RegExp("^(000[1-9]|00[1-9][0-9]|0[1-9][0-9]{2}|[1-9][0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\\.[0-9]{1,9})?(Z|[+-]([01][0-9]|2[0-3]):[0-5][0-9])$", 'u');
export const UUID_PATTERN = new RegExp("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", 'u');
export const EMAIL_PATTERN = new RegExp("^[^@ \\t\\n]+@[^@ \\t\\n]+\\.[^@ \\t\\n]+$", 'u');
export const URI_PATTERN = new RegExp("^[A-Za-z][A-Za-z0-9+.-]*:[^ \\t\\n]+$", 'u');

export const dateSchema = z.iso.date().regex(DATE_PATTERN);

// Any RFC 3339 offset is accepted; the value is held in UTC with milliseconds, like the system timestamps.
export const dateTimeSchema = z.iso
    .datetime({ offset: true })
    .regex(DATE_TIME_PATTERN)
    .transform((value) => new Date(value))
    .refine((date) => date.getUTCFullYear() >= 1 && date.getUTCFullYear() <= 9999, 'must fall between the years 0001 and 9999 in UTC')
    .transform((date) => date.toISOString());

export const uuidSchema = z.string().regex(UUID_PATTERN, 'must be a UUID');

export const hasUniqueItems = (values: unknown[]): boolean => new Set(values).size === values.length;
