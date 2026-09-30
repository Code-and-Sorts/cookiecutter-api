import { describe, it, expect, jest, afterEach } from '@jest/globals';
import { ProxyError } from '@errors';
import { DATABASE_DEADLINE_MS, currentSignal, runWithSignal, withDeadline } from '@utils';

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

    it('should reject with an AbortError as soon as the request is cancelled', async () => {
        const controller = new AbortController();
        const pending = runWithSignal(controller.signal, () => withDeadline(new Promise<never>(() => undefined)));
        controller.abort();
        await expect(pending).rejects.toMatchObject({ name: 'AbortError', code: 'ABORT_ERR' });
    });

    it('should reject at once when the request was already cancelled', async () => {
        const controller = new AbortController();
        controller.abort();
        const pending = runWithSignal(controller.signal, () => withDeadline(Promise.reject(new Error('late'))));
        await expect(pending).rejects.toMatchObject({ name: 'AbortError' });
    });

    it('should remove its abort listener once the operation settles', async () => {
        const controller = new AbortController();
        const remove = jest.spyOn(controller.signal, 'removeEventListener');
        await runWithSignal(controller.signal, () => withDeadline(Promise.resolve('ok')));
        expect(remove).toHaveBeenCalledWith('abort', expect.any(Function));
    });

    it('should expose the signal only inside the request context', () => {
        const controller = new AbortController();
        expect(currentSignal()).toBeUndefined();
        runWithSignal(controller.signal, () => expect(currentSignal()).toBe(controller.signal));
    });
});
