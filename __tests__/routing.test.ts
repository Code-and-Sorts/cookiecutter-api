
import { APIGatewayProxyEvent } from 'aws-lambda';
import { NotFoundError } from '@errors';
import * as container from '@config/container';


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
        catController: mockController(),
        dogController: mockController(),
    };
});

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
const mockBody = { id: 'body-id', name: 'mockName' };
const controllers = container as unknown as Record<string, Record<string, jest.Mock>>;

let handler: (event: APIGatewayProxyEvent) => Promise<{ statusCode: number }>;

beforeAll(async () => {
    ({ handler } = await import('../lambda'));
});

const send = async (method: string, path: string, body?: unknown) => {
    const [endpoint, id] = path.split('/').filter(Boolean);
    const event = {
        httpMethod: method,
        resource: id ? `/${endpoint}/{id}` : `/${endpoint}`,
        path,
        pathParameters: id ? { id } : null,
        queryStringParameters: null,
        body: body === undefined ? null : JSON.stringify(body),
    } as unknown as APIGatewayProxyEvent;
    return (await handler(event)).statusCode;
};

describe('routing', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should answer the health check', async () => {
        expect(await send('GET', '/health')).toEqual(200);
    });

    it('should return 404 for an unknown endpoint', async () => {
        expect(await send('GET', '/unknown')).toEqual(404);
    });

    describe('cats', () => {
        const controller = () => controllers['catController'];

        it('should route GET /cats to list', async () => {
            expect(await send('GET', '/cats')).toEqual(200);
            expect(controller().list).toHaveBeenCalledTimes(1);
        });

        it('should route GET /cats/{id} to get', async () => {
            expect(await send('GET', `/cats/${mockId}`)).toEqual(200);
            expect(controller().get).toHaveBeenCalledTimes(1);
        });

        it('should route POST /cats to create', async () => {
            expect(await send('POST', '/cats', { name: 'mockName' })).toEqual(201);
            expect(controller().post).toHaveBeenCalledTimes(1);
        });

        it('should route PATCH /cats/{id} to update', async () => {
            expect(await send('PATCH', `/cats/${mockId}`, mockBody)).toEqual(200);
            expect(controller().update).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });

        it('should return 405 for PUT /cats/{id} (replace disabled)', async () => {
            expect(await send('PUT', `/cats/${mockId}`, mockBody)).toEqual(405);
            expect(controller().replace).not.toHaveBeenCalled();
        });

        it('should route DELETE /cats/{id} to delete', async () => {
            expect(await send('DELETE', `/cats/${mockId}`)).toEqual(200);
            expect(controller().delete).toHaveBeenCalledTimes(1);
        });

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect(await send('GET', `/cats/${mockId}`)).toEqual(404);
        });
    });

    describe('dogs', () => {
        const controller = () => controllers['dogController'];

        it('should route GET /dogs to list', async () => {
            expect(await send('GET', '/dogs')).toEqual(200);
            expect(controller().list).toHaveBeenCalledTimes(1);
        });

        it('should route GET /dogs/{id} to get', async () => {
            expect(await send('GET', `/dogs/${mockId}`)).toEqual(200);
            expect(controller().get).toHaveBeenCalledTimes(1);
        });

        it('should route POST /dogs to create', async () => {
            expect(await send('POST', '/dogs', { name: 'mockName' })).toEqual(201);
            expect(controller().post).toHaveBeenCalledTimes(1);
        });

        it('should return 405 for PATCH /dogs/{id} (update disabled)', async () => {
            expect(await send('PATCH', `/dogs/${mockId}`, mockBody)).toEqual(405);
            expect(controller().update).not.toHaveBeenCalled();
        });

        it('should route PUT /dogs/{id} to replace', async () => {
            expect(await send('PUT', `/dogs/${mockId}`, mockBody)).toEqual(200);
            expect(controller().replace).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });

        it('should route DELETE /dogs/{id} to delete', async () => {
            expect(await send('DELETE', `/dogs/${mockId}`)).toEqual(200);
            expect(controller().delete).toHaveBeenCalledTimes(1);
        });

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect(await send('GET', `/dogs/${mockId}`)).toEqual(404);
        });
    });
});
