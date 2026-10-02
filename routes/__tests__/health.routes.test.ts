import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import { MockFn } from '../../test/mocks';

let routes: (req: unknown, res: unknown) => Promise<void>;

beforeAll(async () => {
    ({ healthRoutes: routes } = (await import('../health.routes')) as unknown as { healthRoutes: typeof routes });
});

const send = async (method: string) => {
    const res = {
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    };
    await routes({ method, query: {} }, res);
    return { status: res.status.mock.calls[0][0], body: res.json.mock.calls[0][0] };
};

describe('health routes', () => {
    it('should answer GET /health with ok', async () => {
        expect(await send('GET')).toEqual({ status: 200, body: { status: 'ok' } });
    });

    it('should return 405 for other methods', async () => {
        expect(await send('POST')).toEqual({ status: 405, body: { errorMessage: 'Method not allowed.' } });
    });
});
