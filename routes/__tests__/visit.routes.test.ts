import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../../test/mocks';

jest.unstable_mockModule('@config/container', () => ({ visitController: mockController() }));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
// Passed through a mocked controller, so its content does not matter.
const mockBody = { any: 'body' };
const mockUser = 'mockUser';
let controller: Record<string, jest.Mock<MockFn>>;

let routes: (event: APIGatewayProxyEvent, id?: string) => Promise<APIGatewayProxyResult>;

beforeAll(async () => {
    ({ visitController: controller } = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>);
    ({ visitRoutes: routes } = await import('../visit.routes'));
});

const send = async (method: string, id?: string, body?: unknown) => {
    const event = {
        httpMethod: method,
        queryStringParameters: { limit: '5' },
        headers: { 'X-User-Id': ` ${mockUser} ` },
        body: body === undefined ? null : typeof body === 'string' ? body : JSON.stringify(body),
        isBase64Encoded: false,
    } as unknown as APIGatewayProxyEvent;
    const result = await routes(event, id);
    expect(result.headers).toEqual({ 'Content-Type': 'application/json' });
    return { status: result.statusCode, body: JSON.parse(result.body) as unknown };
};

const methodNotAllowed = { status: 405, body: { errorMessage: 'Method not allowed.' } };

describe('visitRoutes', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should return 405 for GET /visits (list disabled)', async () => {
        expect(await send('GET')).toEqual(methodNotAllowed);
        expect(controller.list).not.toHaveBeenCalled();
    });

    it('should route GET /visits/{id} to get', async () => {
        expect((await send('GET', mockId)).status).toEqual(200);
        expect(controller.get).toHaveBeenCalledWith(mockId);
    });

    it('should propagate controller errors to the entry point', async () => {
        controller.get.mockRejectedValueOnce(new NotFoundError('missing'));
        await expect(send('GET', mockId)).rejects.toBeInstanceOf(NotFoundError);
    });

    it('should route POST /visits to create with the parsed body and user id', async () => {
        expect((await send('POST', undefined, mockBody)).status).toEqual(201);
        expect(controller.post).toHaveBeenCalledWith(mockBody, mockUser);
    });

    it('should reject a malformed or missing POST body as a validation error', async () => {
        await expect(send('POST', undefined, '{bad')).rejects.toBeInstanceOf(ValidationError);
        await expect(send('POST')).rejects.toBeInstanceOf(ValidationError);
        expect(controller.post).not.toHaveBeenCalled();
    });

    it('should route PATCH /visits/{id} to update with the path id, body and user id', async () => {
        expect((await send('PATCH', mockId, mockBody)).status).toEqual(200);
        expect(controller.update).toHaveBeenCalledWith(mockId, mockBody, mockUser);
    });

    it('should reject a malformed PATCH body as a validation error', async () => {
        await expect(send('PATCH', mockId, '{bad')).rejects.toBeInstanceOf(ValidationError);
        expect(controller.update).not.toHaveBeenCalled();
    });

    it('should return 405 for PUT /visits/{id} (replace disabled)', async () => {
        expect(await send('PUT', mockId, mockBody)).toEqual(methodNotAllowed);
        expect(controller.replace).not.toHaveBeenCalled();
    });

    it('should return 405 for DELETE /visits/{id} (delete disabled)', async () => {
        expect(await send('DELETE', mockId)).toEqual(methodNotAllowed);
        expect(controller.delete).not.toHaveBeenCalled();
    });

    it('should return 405 for item methods on /visits and collection methods on /visits/{id}', async () => {
        for (const method of ['PATCH', 'PUT', 'DELETE']) {
            expect(await send(method, undefined, mockBody)).toEqual(methodNotAllowed);
        }
        expect(await send('POST', mockId, mockBody)).toEqual(methodNotAllowed);
    });

    it('should return 405 for an unsupported method', async () => {
        expect(await send('OPTIONS')).toEqual(methodNotAllowed);
        expect(await send('OPTIONS', mockId)).toEqual(methodNotAllowed);
    });
});
