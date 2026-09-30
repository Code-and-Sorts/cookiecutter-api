import { describe, it, expect, jest } from '@jest/globals';
import { NotFoundError, ProxyError, ValidationError } from '@errors';
import { cancellationError, detectError, isCancellation, parseJsonBody } from '@utils';
import { MockFn } from '../../test/mocks';

describe('detectError', () => {
    it('should return 400 with the message for ValidationError', () => {
        const log = jest.fn<MockFn>();
        expect(detectError(new ValidationError('name is required.'), log)).toEqual({
            status: 400,
            body: { errorMessage: 'name is required.' },
        });
        expect(log).not.toHaveBeenCalled();
    });

    it('should return 404 with the message for NotFoundError', () => {
        const log = jest.fn<MockFn>();
        expect(detectError(new NotFoundError('Item with id 1 was not found.'), log)).toEqual({
            status: 404,
            body: { errorMessage: 'Item with id 1 was not found.' },
        });
        expect(log).not.toHaveBeenCalled();
    });

    it('should log a ProxyError with its cause and hide it behind a generic 500', () => {
        const log = jest.fn<MockFn>();
        const cause = new Error('connection refused');
        const error = new ProxyError('Error creating item in database.', cause);
        expect(error.cause).toBe(cause);
        expect(detectError(error, log)).toEqual({
            status: 500,
            body: { errorMessage: 'An unexpected error occurred.' },
        });
        expect(log).toHaveBeenCalledWith(expect.any(String), error);
    });

    it('should log an unknown error and hide it behind a generic 500', () => {
        const log = jest.fn<MockFn>();
        const error = new Error('secret details');
        expect(detectError(error, log)).toEqual({
            status: 500,
            body: { errorMessage: 'An unexpected error occurred.' },
        });
        expect(log).toHaveBeenCalledWith(expect.any(String), error);
    });

    it('should log to console.error by default', () => {
        const spy = jest.spyOn(console, 'error').mockImplementation(() => undefined);
        expect(detectError('boom').status).toEqual(500);
        expect(spy).toHaveBeenCalledTimes(1);
        spy.mockRestore();
    });
});

describe('parseJsonBody', () => {
    it('should parse a JSON body', () => {
        expect(parseJsonBody('{"name":"x"}')).toEqual({ name: 'x' });
        expect(parseJsonBody('[1]')).toEqual([1]);
    });

    it.each([undefined, null, '', '   ', '{bad', 'name=x'])('should reject %p with a 400 ValidationError', (raw) => {
        expect(() => parseJsonBody(raw)).toThrow(ValidationError);
        expect(() => parseJsonBody(raw)).toThrow('Request body must be valid JSON.');
    });
});

describe('cancelled requests', () => {
    it('should not log an aborted request at error level', () => {
        const log = jest.fn<MockFn>();
        const warn = jest.spyOn(console, 'warn').mockImplementation(() => undefined);
        const aborted = Object.assign(new Error('This operation was aborted'), { name: 'AbortError' });
        expect(detectError(aborted, log).status).toEqual(500);
        expect(detectError(Object.assign(new Error('aborted'), { code: 'ABORT_ERR' }), log).status).toEqual(500);
        expect(log).not.toHaveBeenCalled();
        expect(warn).toHaveBeenCalledTimes(2);
        warn.mockRestore();
    });
});

describe('cancellationError', () => {
    it('should be treated as a cancellation', () => {
        expect(isCancellation(cancellationError())).toBe(true);
    });
});
