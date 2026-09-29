import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';

import { APIGatewayProxyEvent } from 'aws-lambda';
import { NotFoundError } from '@errors';

type MockFn = (...args: any[]) => any;


jest.unstable_mockModule('@config/container', () => {
    const mockController = () => ({
        list: jest.fn<MockFn>().mockResolvedValue([]),
        get: jest.fn<MockFn>().mockResolvedValue({}),
        post: jest.fn<MockFn>().mockResolvedValue({}),
        update: jest.fn<MockFn>().mockResolvedValue({}),
        replace: jest.fn<MockFn>().mockResolvedValue({}),
        delete: jest.fn<MockFn>().mockResolvedValue(undefined),
    });
    return {
        kittenClawsController: mockController(),
    };
});

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
const mockBody = { id: 'body-id', name: 'mockName' };
let controllers: Record<string, Record<string, jest.Mock<MockFn>>>;

beforeAll(async () => {
    controllers = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>;
});

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
