import { describe, it, expect, jest, afterEach } from '@jest/globals';
import { ProxyError } from '@errors';
import { DATABASE_DEADLINE_MS, withDeadline } from '@utils';

describe('withDeadline', () => {
    afterEach(() => jest.useRealTimers());

    it('should keep the database deadline under 10 seconds', () => {
        expect(DATABASE_DEADLINE_MS).toBeLessThan(10000);
    });

    it('should resolve with the operation result', async () => {
        await expect(withDeadline(Promise.resolve('ok'), 50)).resolves.toEqual('ok');
    });

    it('should pass through the operation error', async () => {
        const error = new Error('boom');
        await expect(withDeadline(Promise.reject(error), 50)).rejects.toBe(error);
    });

    it('should reject with a ProxyError when the operation hangs', async () => {
        jest.useFakeTimers();
        const pending = withDeadline(new Promise<never>(() => undefined));
        const assertion = expect(pending).rejects.toBeInstanceOf(ProxyError);
        jest.advanceTimersByTime(DATABASE_DEADLINE_MS);
        await assertion;
        await expect(pending).rejects.toThrow(`Database operation timed out after ${DATABASE_DEADLINE_MS} ms.`);
    });
});
