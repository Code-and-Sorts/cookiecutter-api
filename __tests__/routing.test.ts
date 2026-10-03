import { describe, it, expect, beforeEach, beforeAll, afterEach, jest } from '@jest/globals';
import { NotFoundError, ValidationError } from '@errors';
import { EventEmitter } from 'node:events';
import { currentSignal } from '@utils';
import { MockFn, mockController } from '../test/mocks';

jest.unstable_mockModule('@google-cloud/functions-framework', () => ({ http: jest.fn<MockFn>() }));
jest.unstable_mockModule('@config/container', () => ({
    catController: mockController(),
    dogController: mockController(),
}));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
let controllers: Record<string, Record<string, jest.Mock<MockFn>>>;

beforeAll(async () => {
    controllers = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>;
});

type Handler = (req: unknown, res: unknown) => Promise<void>;
let api: Handler;
let finalHandler: (req: unknown, res: unknown) => (error?: unknown) => void;

beforeAll(async () => {
    const ff = await import('@google-cloud/functions-framework');
    const main = await import('../main');
    api = main.api as unknown as Handler;
    finalHandler = main.frameworkFinalHandler as unknown as typeof finalHandler;
    expect(ff.http).toHaveBeenCalledWith('api', main.api);
});

const mockResponse = () =>
    Object.assign(new EventEmitter(), {
        headersSent: false,
        writableEnded: false,
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    });

const send = async (method: string, path: string, body?: unknown) => {
    const res = mockResponse();
    const rawBody = body === undefined ? undefined : Buffer.from(typeof body === 'string' ? body : JSON.stringify(body));
    await api({ method, path, rawBody, query: {}, headers: {} }, res);
    return { status: res.status.mock.calls[0][0], body: res.json.mock.calls[0][0] };
};

const notFound = { status: 404, body: { errorMessage: 'Not found.' } };

describe('routing', () => {
    let consoleError: jest.SpiedFunction<typeof console.error>;

    beforeEach(() => {
        jest.clearAllMocks();
        consoleError = jest.spyOn(console, 'error').mockImplementation(() => undefined);
    });

    afterEach(() => consoleError.mockRestore());

    it('should dispatch GET /health to the health check', async () => {
        expect(await send('GET', '/health')).toEqual({ status: 200, body: { status: 'ok' } });
    });

    it('should only match the health path exactly', async () => {
        expect(await send('GET', '/health/extra')).toEqual(notFound);
    });

    it('should return a JSON 404 for an unknown endpoint', async () => {
        expect(await send('GET', '/unknown')).toEqual(notFound);
        expect(await send('GET', '/')).toEqual(notFound);
    });

    it('should return a JSON 404 below an item path', async () => {
        expect(await send('GET', `/cats/${mockId}/extra`)).toEqual(notFound);
    });

    it('should dispatch GET /cats to the Cat routes', async () => {
        expect((await send('GET', '/cats')).status).toEqual(200);
        expect(controllers['catController'].list).toHaveBeenCalledTimes(1);
    });

    it('should dispatch GET /dogs to the Dog routes', async () => {
        expect((await send('GET', '/dogs')).status).toEqual(200);
        expect(controllers['dogController'].list).toHaveBeenCalledTimes(1);
    });

    it('should map route errors to JSON error responses', async () => {
        const method = controllers['catController'].list;
        method.mockRejectedValueOnce(new NotFoundError('Cat with id x was not found.'));
        expect(await send('GET', '/cats')).toEqual({ status: 404, body: { errorMessage: 'Cat with id x was not found.' } });
        method.mockRejectedValueOnce(new ValidationError('name is required.'));
        expect(await send('GET', '/cats')).toEqual({ status: 400, body: { errorMessage: 'name is required.' } });
        expect(consoleError).not.toHaveBeenCalled();
    });

    it('should log unexpected errors and answer with a generic 500', async () => {
        const method = controllers['catController'].list;
        const error = new Error('secret details');
        method.mockRejectedValueOnce(error);
        expect(await send('GET', '/cats')).toEqual({ status: 500, body: { errorMessage: 'An unexpected error occurred.' } });
        expect(consoleError).toHaveBeenCalledWith(expect.any(String), error);
    });

    it('should answer a malformed JSON body with a JSON 400', async () => {
        expect(await send('POST', '/cats', '{bad')).toEqual({
            status: 400,
            body: { errorMessage: 'Request body must be valid JSON.' },
        });
        expect(controllers['catController'].post).not.toHaveBeenCalled();
    });
});

describe('client disconnects', () => {
    it('should abort the request signal when the response closes before it is sent', async () => {
        const method = controllers['catController'].list;
        let signal: AbortSignal | undefined;
        let release: () => void = () => undefined;
        method.mockImplementationOnce(() => {
            signal = currentSignal();
            return new Promise((resolve) => {
                release = () => resolve({});
            });
        });
        const res = mockResponse();
        const rawBody = Buffer.from(JSON.stringify({ name: 'mockName' }));
        const pending = api({ method: 'GET', path: '/cats', rawBody, query: {}, headers: {} }, res);
        await new Promise((resolve) => setImmediate(resolve));
        expect(signal?.aborted).toBe(false);
        res.emit('close');
        expect(signal?.aborted).toBe(true);
        release();
        await pending;
        expect(res.listenerCount('close')).toEqual(0);
    });

    it('should not abort once the response has been sent', async () => {
        const method = controllers['catController'].list;
        let signal: AbortSignal | undefined;
        method.mockImplementationOnce(async () => {
            signal = currentSignal();
            return {};
        });
        const res = mockResponse();
        const rawBody = Buffer.from(JSON.stringify({ name: 'mockName' }));
        const pending = api({ method: 'GET', path: '/cats', rawBody, query: {}, headers: {} }, res);
        await pending;
        res.writableEnded = true;
        res.emit('close');
        expect(signal?.aborted).toBe(false);
    });
});

describe('frameworkFinalHandler', () => {
    const run = (error?: unknown, headersSent = false) => {
        const res = { ...mockResponse(), headersSent };
        finalHandler({}, res)(error);
        return res;
    };

    it('should answer a request no route handled with a JSON 404', () => {
        const res = run();
        expect(res.status).toHaveBeenCalledWith(404);
        expect(res.json).toHaveBeenCalledWith({ errorMessage: 'Not found.' });
    });

    it('should answer a body-parser failure with a JSON 400', () => {
        const res = run(Object.assign(new SyntaxError('Unexpected token'), { status: 400, type: 'entity.parse.failed' }));
        expect(res.status).toHaveBeenCalledWith(400);
        expect(res.json).toHaveBeenCalledWith({ errorMessage: 'Request body must be valid JSON.' });
    });

    it('should log other errors and answer with a generic 500', () => {
        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => undefined);
        const res = run(new Error('boom'));
        expect(res.status).toHaveBeenCalledWith(500);
        expect(res.json).toHaveBeenCalledWith({ errorMessage: 'An unexpected error occurred.' });
        expect(consoleError).toHaveBeenCalledTimes(1);
        consoleError.mockRestore();
    });

    it('should leave a response that was already sent alone', () => {
        const res = run(new Error('late'), true);
        expect(res.status).not.toHaveBeenCalled();
    });
});
