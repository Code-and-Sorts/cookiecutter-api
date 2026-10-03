{%- from 'shared/_fields.jinja' import PATTERNS -%}
import { z } from 'zod';

export const DATE_TIME_PATTERN = new RegExp({{ PATTERNS.date_time | tojson }});
export const UUID_PATTERN = new RegExp({{ PATTERNS.uuid | tojson }});
export const EMAIL_PATTERN = new RegExp({{ PATTERNS.email | tojson }});
export const URI_PATTERN = new RegExp({{ PATTERNS.uri | tojson }});

export const dateSchema = z.iso.date();

// Any RFC 3339 offset is accepted; the value is held in UTC with milliseconds, like the system timestamps.
export const dateTimeSchema = z.iso
    .datetime({ offset: true })
    .regex(DATE_TIME_PATTERN)
    .transform((value) => new Date(value).toISOString());

export const uuidSchema = z.string().regex(UUID_PATTERN, 'must be a UUID');

export const hasUniqueItems = (values: unknown[]): boolean => new Set(values).size === values.length;
