{%- from 'shared/_fields.jinja' import DATE_TIME_CASES, DATE_TIME_EXAMPLE, INVALID_DATE_TIMES, INVALID_DATES -%}
import { describe, it, expect } from '@jest/globals';
import { z } from 'zod';
import { dateSchema, dateTimeSchema, hasUniqueItems } from '@models';

describe('dateTimeSchema', () => {
    it.each([
{%- for case in DATE_TIME_CASES %}
        ['{{ case.sent }}', '{{ case.stored }}'],
{%- endfor %}
    ])('should store %s in UTC with milliseconds', (sent, stored) => {
        expect(dateTimeSchema.parse(sent)).toEqual(stored);
    });

    it.each(['2026-02-30T09:30:00Z'{% for value in INVALID_DATE_TIMES %}, '{{ value }}'{% endfor %}])('should reject %s', (sent) => {
        expect(dateTimeSchema.safeParse(sent).success).toBe(false);
    });

    it('should compare unique items as instants in UTC', () => {
        const schema = z.array(dateTimeSchema).refine(hasUniqueItems);
        expect(schema.safeParse(['{{ DATE_TIME_EXAMPLE.sent }}', '{{ DATE_TIME_EXAMPLE.stored }}']).success).toBe(false);
    });
});

describe('dateSchema', () => {
    it.each([{% for value in INVALID_DATES %}'{{ value }}'{{ ", " if not loop.last }}{% endfor %}])('should reject %s', (sent) => {
        expect(dateSchema.safeParse(sent).success).toBe(false);
    });

    it('should accept the first and last dates', () => {
        expect(dateSchema.safeParse('0001-01-01').success).toBe(true);
        expect(dateSchema.safeParse('9999-12-31').success).toBe(true);
    });
});
