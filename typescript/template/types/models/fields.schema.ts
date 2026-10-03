{%- from 'shared/_fields.jinja' import PATTERNS -%}
import { z } from 'zod';

// The u flag makes JavaScript count code points, as every other language does.
export const DATE_PATTERN = new RegExp({{ PATTERNS.date | tojson }}, 'u');
export const DATE_TIME_PATTERN = new RegExp({{ PATTERNS.date_time | tojson }}, 'u');
export const UUID_PATTERN = new RegExp({{ PATTERNS.uuid | tojson }}, 'u');
export const EMAIL_PATTERN = new RegExp({{ PATTERNS.email | tojson }}, 'u');
export const URI_PATTERN = new RegExp({{ PATTERNS.uri | tojson }}, 'u');

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
