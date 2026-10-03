import { describe, it, expect } from '@jest/globals';
import { z } from 'zod';
import { dateSchema, dateTimeSchema, hasUniqueItems } from '@models';

describe('dateTimeSchema', () => {
    it.each([
        ['2026-01-31T11:30:00.1239+02:00', '2026-01-31T09:30:00.123Z'],
        ['2026-01-31T00:30:00.123456789+23:59', '2026-01-30T00:31:00.123Z'],
        ['2026-01-31T23:30:00-23:59', '2026-02-01T23:29:00.000Z'],
        ['0001-01-01T00:00:00-00:00', '0001-01-01T00:00:00.000Z'],
        ['9999-12-31T23:59:59.999Z', '9999-12-31T23:59:59.999Z'],
    ])('should store %s in UTC with milliseconds', (sent, stored) => {
        expect(dateTimeSchema.parse(sent)).toEqual(stored);
    });

    it.each(['2026-02-30T09:30:00Z', '2026-01-31T09:30:00', '2026-01-31T24:00:00Z', '2026-01-31T23:59:60Z', '2026-01-31T09:30:00+14:60', '0000-12-31T23:00:00-01:00', '0001-01-01T00:00:00+01:00', '9999-12-31T23:59:59-01:00'])('should reject %s', (sent) => {
        expect(dateTimeSchema.safeParse(sent).success).toBe(false);
    });

    it('should compare unique items as instants in UTC', () => {
        const schema = z.array(dateTimeSchema).refine(hasUniqueItems);
        expect(schema.safeParse(['2026-01-31T11:30:00.1239+02:00', '2026-01-31T09:30:00.123Z']).success).toBe(false);
    });
});

describe('dateSchema', () => {
    it.each(['2026-02-30', '2026-13-01', '0000-01-01'])('should reject %s', (sent) => {
        expect(dateSchema.safeParse(sent).success).toBe(false);
    });

    it('should accept the first and last dates', () => {
        expect(dateSchema.safeParse('0001-01-01').success).toBe(true);
        expect(dateSchema.safeParse('9999-12-31').success).toBe(true);
    });
});
