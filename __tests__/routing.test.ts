import { describe, it, expect, beforeEach, beforeAll, afterEach, jest } from '@jest/globals';
import { APIGatewayProxyEvent } from 'aws-lambda';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../test/mocks';

jest.unstable_mockModule('@config/container', () => ({
    kittenClawsController: mockController(),
}));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
let controllers: Record<string, Record<string, jest.Mock<MockFn>>>;

beforeAll(async () => {
    controllers = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>;
});

let handler: (event: APIGatewayProxyEvent) => Promise<{ statusCode: number; headers?: unknown; body: string }>;

beforeAll(async () => {
    ({ handler } = await import('../lambda'));
});

const send = async (method: string, path: string, body?: unknown) => {
    const segments = path.split('/').filter(Boolean);
    const event = {
        httpMethod: method,
        resource: segments.length > 1 ? `/${segments[0]}/{id}${segments.slice(2).map((s) => `/${s}`).join('')}` : path,
        path,
        pathParameters: segments.length > 1 ? { id: segments[1] } : null,
        queryStringParameters: null,
        body: body === undefined ? null : typeof body === 'string' ? body : JSON.stringify(body),
        isBase64Encoded: false,
    } as unknown as APIGatewayProxyEvent;
    const result = await handler(event);
    expect(result.headers).toEqual({ 'Content-Type': 'application/json' });
    return { status: result.statusCode, body: JSON.parse(result.body) as unknown };
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
        expect(await send('GET', `/kittenclaws/${mockId}/extra`)).toEqual(notFound);
    });

    it('should dispatch GET /kittenclaws to the KittenClaws routes', async () => {
        expect((await send('GET', '/kittenclaws')).status).toEqual(200);
        expect(controllers['kittenClawsController'].list).toHaveBeenCalledTimes(1);
    });

    it('should map route errors to JSON error responses', async () => {
        const method = controllers['kittenClawsController'].list;
        method.mockRejectedValueOnce(new NotFoundError('KittenClaws with id x was not found.'));
        expect(await send('GET', '/kittenclaws')).toEqual({ status: 404, body: { errorMessage: 'KittenClaws with id x was not found.' } });
        method.mockRejectedValueOnce(new ValidationError('name is required.'));
        expect(await send('GET', '/kittenclaws')).toEqual({ status: 400, body: { errorMessage: 'name is required.' } });
        expect(consoleError).not.toHaveBeenCalled();
    });

    it('should log unexpected errors and answer with a generic 500', async () => {
        const method = controllers['kittenClawsController'].list;
        const error = new Error('secret details');
        method.mockRejectedValueOnce(error);
        expect(await send('GET', '/kittenclaws')).toEqual({ status: 500, body: { errorMessage: 'An unexpected error occurred.' } });
        expect(consoleError).toHaveBeenCalledWith(expect.any(String), error);
    });

    it('should answer a malformed JSON body with a JSON 400', async () => {
        expect(await send('POST', '/kittenclaws', '{bad')).toEqual({
            status: 400,
            body: { errorMessage: 'Request body must be valid JSON.' },
        });
        expect(controllers['kittenClawsController'].post).not.toHaveBeenCalled();
    });
});
