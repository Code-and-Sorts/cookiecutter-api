import { describe, it, expect } from '@jest/globals';
import { coerceLimit, DEFAULT_LIST_LIMIT, MAX_LIST_LIMIT } from '@utils';

describe('coerceLimit', () => {
    it('should default the limit when not provided or invalid', () => {
        expect(coerceLimit()).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit(null)).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit('')).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit('abc')).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit('5abc')).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit('2.5')).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit('-3')).toEqual(DEFAULT_LIST_LIMIT);
        expect(coerceLimit(0)).toEqual(DEFAULT_LIST_LIMIT);
    });

    it('should parse and clamp the limit', () => {
        expect(coerceLimit('5')).toEqual(5);
        expect(coerceLimit(7)).toEqual(7);
        expect(coerceLimit('999999')).toEqual(MAX_LIST_LIMIT);
    });
});
