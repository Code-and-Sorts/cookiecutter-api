{%- from 'shared/_fields.jinja' import DATE_TIME_EXAMPLE, OUT_OF_RANGE_DATE_TIMES -%}
import { describe, it, expect } from '@jest/globals';
import { z } from 'zod';
import { dateTimeSchema, hasUniqueItems } from '@models';

describe('dateTimeSchema', () => {
    it.each([
        ['2026-01-31T09:30:00Z', '2026-01-31T09:30:00.000Z'],
        ['{{ DATE_TIME_EXAMPLE.sent }}', '{{ DATE_TIME_EXAMPLE.stored }}'],
        ['0001-01-01T00:00:00Z', '0001-01-01T00:00:00.000Z'],
        ['9999-12-31T23:59:59.999Z', '9999-12-31T23:59:59.999Z'],
    ])('should store %s in UTC with milliseconds', (sent, stored) => {
        expect(dateTimeSchema.parse(sent)).toEqual(stored);
    });

    it.each(['2026-01-31T09:30:00', '2026-02-30T09:30:00Z'{% for value in OUT_OF_RANGE_DATE_TIMES %}, '{{ value }}'{% endfor %}])('should reject %s', (sent) => {
        expect(dateTimeSchema.safeParse(sent).success).toBe(false);
    });

    it('should compare unique items as instants in UTC', () => {
        const schema = z.array(dateTimeSchema).refine(hasUniqueItems);
        expect(schema.safeParse(['{{ DATE_TIME_EXAMPLE.sent }}', '{{ DATE_TIME_EXAMPLE.stored }}']).success).toBe(false);
    });
});
