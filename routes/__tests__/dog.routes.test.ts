import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../../test/mocks';

jest.unstable_mockModule('@config/container', () => ({ dogController: mockController() }));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
const mockBody = { name: 'mockName' };
const mockUser = 'mockUser';
let controller: Record<string, jest.Mock<MockFn>>;

let routes: (event: APIGatewayProxyEvent, id?: string) => Promise<APIGatewayProxyResult>;

beforeAll(async () => {
    ({ dogController: controller } = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>);
    ({ dogRoutes: routes } = await import('../dog.routes'));
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

describe('dogRoutes', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should route GET /dogs to list with the limit query', async () => {
        expect((await send('GET')).status).toEqual(200);
        expect(controller.list).toHaveBeenCalledWith('5');
    });

    it('should route GET /dogs/{id} to get', async () => {
        expect((await send('GET', mockId)).status).toEqual(200);
        expect(controller.get).toHaveBeenCalledWith(mockId);
    });

    it('should propagate controller errors to the entry point', async () => {
        controller.get.mockRejectedValueOnce(new NotFoundError('missing'));
        await expect(send('GET', mockId)).rejects.toBeInstanceOf(NotFoundError);
    });

    it('should route POST /dogs to create with the parsed body and user id', async () => {
        expect((await send('POST', undefined, mockBody)).status).toEqual(201);
        expect(controller.post).toHaveBeenCalledWith(mockBody, mockUser);
    });

    it('should reject a malformed or missing POST body as a validation error', async () => {
        await expect(send('POST', undefined, '{bad')).rejects.toBeInstanceOf(ValidationError);
        await expect(send('POST')).rejects.toBeInstanceOf(ValidationError);
        expect(controller.post).not.toHaveBeenCalled();
    });

    it('should return 405 for PATCH /dogs/{id} (update disabled)', async () => {
        expect(await send('PATCH', mockId, mockBody)).toEqual(methodNotAllowed);
        expect(controller.update).not.toHaveBeenCalled();
    });

    it('should route PUT /dogs/{id} to replace with the path id, body and user id', async () => {
        expect((await send('PUT', mockId, mockBody)).status).toEqual(200);
        expect(controller.replace).toHaveBeenCalledWith(mockId, mockBody, mockUser);
    });

    it('should reject a malformed PUT body as a validation error', async () => {
        await expect(send('PUT', mockId, 'not json')).rejects.toBeInstanceOf(ValidationError);
        expect(controller.replace).not.toHaveBeenCalled();
    });

    it('should route DELETE /dogs/{id} to delete with the user id and return its message', async () => {
        expect(await send('DELETE', mockId)).toEqual({ status: 200, body: { message: 'deleted' } });
        expect(controller.delete).toHaveBeenCalledWith(mockId, mockUser);
    });

    it('should return 405 for item methods on /dogs and collection methods on /dogs/{id}', async () => {
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
