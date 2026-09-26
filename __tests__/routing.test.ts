
import { NotFoundError } from '@errors';
import * as container from '@config/container';


jest.mock('@google-cloud/functions-framework', () => ({ http: jest.fn() }));
jest.mock('@config/container', () => {
    const mockController = () => ({
        list: jest.fn().mockResolvedValue([]),
        get: jest.fn().mockResolvedValue({}),
        post: jest.fn().mockResolvedValue({}),
        update: jest.fn().mockResolvedValue({}),
        replace: jest.fn().mockResolvedValue({}),
        delete: jest.fn().mockResolvedValue(undefined),
    });
    return {
        kittenClawsController: mockController(),
    };
});

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
const mockBody = { id: 'body-id', name: 'mockName' };
const controllers = container as unknown as Record<string, Record<string, jest.Mock>>;

let api: (req: unknown, res: unknown) => Promise<void>;

beforeAll(async () => {
    const ff = await import('@google-cloud/functions-framework');
    await import('../main');
    api = (ff.http as jest.Mock).mock.calls[0][1];
});

const send = async (method: string, path: string, body?: unknown) => {
    const res = {
        status: jest.fn().mockReturnThis(),
        json: jest.fn().mockReturnThis(),
        send: jest.fn().mockReturnThis(),
    };
    await api({ method, path, body, query: {} }, res);
    return res.status.mock.calls[0][0];
};

describe('routing', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should answer the health check', async () => {
        expect(await send('GET', '/health')).toEqual(200);
    });

    it('should return 404 for an unknown endpoint', async () => {
        expect(await send('GET', '/unknown')).toEqual(404);
    });

    describe('kitties', () => {
        const controller = () => controllers['kittenClawsController'];

        it('should route GET /kitties to list', async () => {
            expect(await send('GET', '/kitties')).toEqual(200);
            expect(controller().list).toHaveBeenCalledTimes(1);
        });

        it('should route GET /kitties/{id} to get', async () => {
            expect(await send('GET', `/kitties/${mockId}`)).toEqual(200);
            expect(controller().get).toHaveBeenCalledTimes(1);
        });

        it('should route POST /kitties to create', async () => {
            expect(await send('POST', '/kitties', { name: 'mockName' })).toEqual(201);
            expect(controller().post).toHaveBeenCalledTimes(1);
        });

        it('should route PATCH /kitties/{id} to update', async () => {
            expect(await send('PATCH', `/kitties/${mockId}`, mockBody)).toEqual(200);
            expect(controller().update).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });

        it('should return 405 for PUT /kitties/{id} (replace disabled)', async () => {
            expect(await send('PUT', `/kitties/${mockId}`, mockBody)).toEqual(405);
            expect(controller().replace).not.toHaveBeenCalled();
        });

        it('should route DELETE /kitties/{id} to delete', async () => {
            expect(await send('DELETE', `/kitties/${mockId}`)).toEqual(200);
            expect(controller().delete).toHaveBeenCalledTimes(1);
        });

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect(await send('GET', `/kitties/${mockId}`)).toEqual(404);
        });
    });
});
