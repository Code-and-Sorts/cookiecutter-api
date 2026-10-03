import { describe, it, expect } from '@jest/globals';
import { z } from 'zod';
import { ValidationError } from '@errors';
import { SchemaValidator } from '@services';

describe('SchemaValidator', () => {
    const validator = new SchemaValidator();
    const schema = z.object({ label: z.string().min(1) }).strict();
    const nested = z.object({ count: z.number().min(2), tags: z.array(z.string()) }).strict();

    const messageFor = (value: unknown, s: z.ZodType<unknown> = schema): string => {
        try {
            validator.validate(value, s);
        } catch (error) {
            expect(error).toBeInstanceOf(ValidationError);
            expect((error as ValidationError).statusCode).toEqual(400);
            return (error as ValidationError).message;
        }
        throw new Error('expected a ValidationError');
    };

    it('should return the parsed value when valid', () => {
        expect(validator.validate({ label: 'mockLabel' }, schema)).toEqual({ label: 'mockLabel' });
    });

    it('should report a missing required field', () => {
        expect(messageFor({})).toEqual('label is required.');
    });

    it('should report a field of the wrong type without coercion', () => {
        expect(messageFor({ label: 1 })).toEqual('label must be a string.');
        expect(messageFor({ label: true })).toEqual('label must be a string.');
        expect(messageFor({ label: null })).toEqual('label must be a string.');
        expect(messageFor({ count: 3, tags: {} }, nested)).toEqual('tags must be an array.');
    });

    it('should report an empty string', () => {
        expect(messageFor({ label: '' })).toEqual('label must not be empty.');
    });

    it('should report unknown fields', () => {
        expect(messageFor({ label: 'mockLabel', id: 'x' })).toEqual('Unknown field: id.');
        expect(messageFor({ label: 'mockLabel', id: 'x', isDeleted: true })).toEqual('Unknown fields: id, isDeleted.');
    });

    it('should report a body that is not an object', () => {
        expect(messageFor([])).toEqual('Request body must be a JSON object.');
        expect(messageFor('mockLabel')).toEqual('Request body must be a JSON object.');
        expect(messageFor(null)).toEqual('Request body must be a JSON object.');
    });

    it('should fall back to the Zod message for other issues', () => {
        expect(messageFor({ count: 1, tags: [] }, nested)).toEqual('count: Too small: expected number to be >=2');
    });

    it('should join several issues', () => {
        expect(messageFor({ label: 1, extra: true })).toEqual('label must be a string. Unknown field: extra.');
    });
});
