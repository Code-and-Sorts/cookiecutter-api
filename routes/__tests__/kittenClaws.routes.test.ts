import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../../test/mocks';

jest.unstable_mockModule('@config/container', () => ({ kittenClawsController: mockController() }));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
// Passed through a mocked controller, so its content does not matter.
const mockBody = { any: 'body' };
const mockUser = 'mockUser';
let controller: Record<string, jest.Mock<MockFn>>;

let routes: (req: unknown, res: unknown, id?: string) => Promise<void>;

beforeAll(async () => {
    ({ kittenClawsController: controller } = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>);
    ({ kittenClawsRoutes: routes } = (await import('../kittenClaws.routes')) as unknown as { kittenClawsRoutes: typeof routes });
});

const send = async (method: string, id?: string, body?: unknown) => {
    const res = {
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    };
    const rawBody = body === undefined ? undefined : Buffer.from(typeof body === 'string' ? body : JSON.stringify(body));
    await routes({ method, rawBody, query: { limit: '5' }, headers: { 'x-user-id': ` ${mockUser} ` } }, res, id);
    return { status: res.status.mock.calls[0][0], body: res.json.mock.calls[0][0] };
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
