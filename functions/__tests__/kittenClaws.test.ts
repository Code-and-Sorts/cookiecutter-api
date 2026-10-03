import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
import { HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../../test/mocks';

jest.unstable_mockModule('@azure/functions', () => ({ app: { http: jest.fn<MockFn>() } }));
jest.unstable_mockModule('@config/container', () => ({ kittenClawsController: mockController() }));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
const mockBody = { name: 'mockName' };
const mockUser = 'mockUser';

type Registration = { methods: string[]; route: string; authLevel: string; handler: (request: HttpRequest, context: InvocationContext) => Promise<HttpResponseInit> };
let registrations: Record<string, Registration>;
let controller: Record<string, jest.Mock<MockFn>>;
let context: { log: jest.Mock<MockFn>; error: jest.Mock<MockFn> };

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    ({ kittenClawsController: controller } = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>);
    await import('../kittenClaws');
    registrations = Object.fromEntries((app.http as jest.Mock<MockFn>).mock.calls.map(([name, options]) => [name, options]));
});

const call = async (name: string, id?: string, body?: unknown) => {
    const request = {
        url: `http://localhost/api/${name}`,
        params: id ? { id } : {},
        headers: new Headers({ 'X-User-Id': ` ${mockUser} ` }),
        query: new URLSearchParams('limit=5'),
        text: async () => (body === undefined ? '' : typeof body === 'string' ? body : JSON.stringify(body)),
    } as unknown as HttpRequest;
    const response = await registrations[name].handler(request, context as unknown as InvocationContext);
    expect(response.headers).toEqual({ 'Content-Type': 'application/json' });
    return { status: response.status, body: JSON.parse(response.body as string) as unknown };
};

describe('kittenclaws functions', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        context = { log: jest.fn<MockFn>(), error: jest.fn<MockFn>() };
    });

    it('should register list', async () => {
        expect(registrations['listKittenClaws']).toMatchObject({ methods: ['GET'], route: 'kittenclaws', authLevel: 'function' });
        expect(await call('listKittenClaws')).toEqual({ status: 200, body: [] });
        expect(controller.list).toHaveBeenCalledWith('5');
    });

    it('should register get by id', async () => {
        expect(registrations['getByIdKittenClaws']).toMatchObject({ methods: ['GET'], route: 'kittenclaws/{id}', authLevel: 'function' });
        expect((await call('getByIdKittenClaws', mockId)).status).toEqual(200);
        expect(controller.get).toHaveBeenCalledWith(mockId);
    });

    it('should register create with the body and user id', async () => {
        expect(registrations['createKittenClaws']).toMatchObject({ methods: ['POST'], route: 'kittenclaws', authLevel: 'function' });
        expect((await call('createKittenClaws', undefined, mockBody)).status).toEqual(201);
        expect(controller.post).toHaveBeenCalledWith(mockBody, mockUser);
    });

    it('should answer a malformed or missing create body with a JSON 400', async () => {
        const invalid = { status: 400, body: { errorMessage: 'Request body must be valid JSON.' } };
        expect(await call('createKittenClaws', undefined, '{bad')).toEqual(invalid);
        expect(await call('createKittenClaws')).toEqual(invalid);
        expect(controller.post).not.toHaveBeenCalled();
    });

    it('should register update with the path id, body and user id', async () => {
        expect(registrations['updateKittenClaws']).toMatchObject({ methods: ['PATCH'], route: 'kittenclaws/{id}', authLevel: 'function' });
        expect((await call('updateKittenClaws', mockId, mockBody)).status).toEqual(200);
        expect(controller.update).toHaveBeenCalledWith(mockId, mockBody, mockUser);
    });

    it('should answer a malformed update body with a JSON 400', async () => {
        expect((await call('updateKittenClaws', mockId, '{bad')).status).toEqual(400);
        expect(controller.update).not.toHaveBeenCalled();
    });

    it('should register delete with the user id and return its message', async () => {
        expect(registrations['deleteKittenClaws']).toMatchObject({ methods: ['DELETE'], route: 'kittenclaws/{id}', authLevel: 'function' });
        expect(await call('deleteKittenClaws', mockId)).toEqual({ status: 200, body: { message: 'deleted' } });
        expect(controller.delete).toHaveBeenCalledWith(mockId, mockUser);
    });

    it('should map expected errors to JSON error responses without logging them', async () => {
        controller.list.mockRejectedValueOnce(new NotFoundError('KittenClaws with id x was not found.'));
        expect(await call('listKittenClaws', undefined, mockBody)).toEqual({ status: 404, body: { errorMessage: 'KittenClaws with id x was not found.' } });
        controller.list.mockRejectedValueOnce(new ValidationError('name is required.'));
        expect(await call('listKittenClaws', undefined, mockBody)).toEqual({ status: 400, body: { errorMessage: 'name is required.' } });
        expect(context.error).not.toHaveBeenCalled();
    });

    it('should log unexpected errors and answer with a generic 500', async () => {
        const error = new Error('secret details');
        controller.list.mockRejectedValueOnce(error);
        expect(await call('listKittenClaws', undefined, mockBody)).toEqual({ status: 500, body: { errorMessage: 'An unexpected error occurred.' } });
        expect(context.error).toHaveBeenCalledWith(expect.any(String), error);
    });

    it('should not register disabled operations', () => {
        expect(registrations['replaceKittenClaws']).toBeUndefined();
    });
});
