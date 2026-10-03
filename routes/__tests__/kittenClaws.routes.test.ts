import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../../test/mocks';

jest.unstable_mockModule('@config/container', () => ({ kittenClawsController: mockController() }));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
// Passed through a mocked controller, so its content does not matter.
const mockBody = { any: 'body' };
const mockUser = 'mockUser';
let controller: Record<string, jest.Mock<MockFn>>;

let routes: (event: APIGatewayProxyEvent, id?: string) => Promise<APIGatewayProxyResult>;

beforeAll(async () => {
    ({ kittenClawsController: controller } = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>);
    ({ kittenClawsRoutes: routes } = await import('../kittenClaws.routes'));
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

describe('kittenClawsRoutes', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should route GET /kittenclaws to list with the limit query', async () => {
        expect((await send('GET')).status).toEqual(200);
        expect(controller.list).toHaveBeenCalledWith('5');
    });

    it('should route GET /kittenclaws/{id} to get', async () => {
        expect((await send('GET', mockId)).status).toEqual(200);
        expect(controller.get).toHaveBeenCalledWith(mockId);
    });

    it('should propagate controller errors to the entry point', async () => {
        controller.get.mockRejectedValueOnce(new NotFoundError('missing'));
        await expect(send('GET', mockId)).rejects.toBeInstanceOf(NotFoundError);
    });

    it('should route POST /kittenclaws to create with the parsed body and user id', async () => {
        expect((await send('POST', undefined, mockBody)).status).toEqual(201);
        expect(controller.post).toHaveBeenCalledWith(mockBody, mockUser);
    });

    it('should reject a malformed or missing POST body as a validation error', async () => {
        await expect(send('POST', undefined, '{bad')).rejects.toBeInstanceOf(ValidationError);
        await expect(send('POST')).rejects.toBeInstanceOf(ValidationError);
        expect(controller.post).not.toHaveBeenCalled();
    });

    it('should route PATCH /kittenclaws/{id} to update with the path id, body and user id', async () => {
        expect((await send('PATCH', mockId, mockBody)).status).toEqual(200);
        expect(controller.update).toHaveBeenCalledWith(mockId, mockBody, mockUser);
    });

    it('should reject a malformed PATCH body as a validation error', async () => {
        await expect(send('PATCH', mockId, '{bad')).rejects.toBeInstanceOf(ValidationError);
        expect(controller.update).not.toHaveBeenCalled();
    });

    it('should return 405 for PUT /kittenclaws/{id} (replace disabled)', async () => {
        expect(await send('PUT', mockId, mockBody)).toEqual(methodNotAllowed);
        expect(controller.replace).not.toHaveBeenCalled();
    });

    it('should route DELETE /kittenclaws/{id} to delete with the user id and return its message', async () => {
        expect(await send('DELETE', mockId)).toEqual({ status: 200, body: { message: 'deleted' } });
        expect(controller.delete).toHaveBeenCalledWith(mockId, mockUser);
    });

    it('should return 405 for item methods on /kittenclaws and collection methods on /kittenclaws/{id}', async () => {
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
