import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
import { HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, mockController } from '../../test/mocks';

jest.unstable_mockModule('@azure/functions', () => ({ app: { http: jest.fn<MockFn>() } }));
jest.unstable_mockModule('@config/container', () => ({ visitController: mockController() }));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
// Passed through a mocked controller, so its content does not matter.
const mockBody = { any: 'body' };
const mockUser = 'mockUser';

type Registration = { methods: string[]; route: string; authLevel: string; handler: (request: HttpRequest, context: InvocationContext) => Promise<HttpResponseInit> };
let registrations: Record<string, Registration>;
let controller: Record<string, jest.Mock<MockFn>>;
let context: { log: jest.Mock<MockFn>; error: jest.Mock<MockFn> };

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    ({ visitController: controller } = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>);
    await import('../visit');
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

describe('visits functions', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        context = { log: jest.fn<MockFn>(), error: jest.fn<MockFn>() };
    });

    it('should register get by id', async () => {
        expect(registrations['getByIdVisit']).toMatchObject({ methods: ['GET'], route: 'visits/{id}', authLevel: 'function' });
        expect((await call('getByIdVisit', mockId)).status).toEqual(200);
        expect(controller.get).toHaveBeenCalledWith(mockId);
    });

    it('should register create with the body and user id', async () => {
        expect(registrations['createVisit']).toMatchObject({ methods: ['POST'], route: 'visits', authLevel: 'function' });
        expect((await call('createVisit', undefined, mockBody)).status).toEqual(201);
        expect(controller.post).toHaveBeenCalledWith(mockBody, mockUser);
    });

    it('should answer a malformed or missing create body with a JSON 400', async () => {
        const invalid = { status: 400, body: { errorMessage: 'Request body must be valid JSON.' } };
        expect(await call('createVisit', undefined, '{bad')).toEqual(invalid);
        expect(await call('createVisit')).toEqual(invalid);
        expect(controller.post).not.toHaveBeenCalled();
    });

    it('should register update with the path id, body and user id', async () => {
        expect(registrations['updateVisit']).toMatchObject({ methods: ['PATCH'], route: 'visits/{id}', authLevel: 'function' });
        expect((await call('updateVisit', mockId, mockBody)).status).toEqual(200);
        expect(controller.update).toHaveBeenCalledWith(mockId, mockBody, mockUser);
    });

    it('should answer a malformed update body with a JSON 400', async () => {
        expect((await call('updateVisit', mockId, '{bad')).status).toEqual(400);
        expect(controller.update).not.toHaveBeenCalled();
    });

    it('should map expected errors to JSON error responses without logging them', async () => {
        controller.get.mockRejectedValueOnce(new NotFoundError('Visit with id x was not found.'));
        expect(await call('getByIdVisit', mockId, mockBody)).toEqual({ status: 404, body: { errorMessage: 'Visit with id x was not found.' } });
        controller.get.mockRejectedValueOnce(new ValidationError('label is required.'));
        expect(await call('getByIdVisit', mockId, mockBody)).toEqual({ status: 400, body: { errorMessage: 'label is required.' } });
        expect(context.error).not.toHaveBeenCalled();
    });

    it('should log unexpected errors and answer with a generic 500', async () => {
        const error = new Error('secret details');
        controller.get.mockRejectedValueOnce(error);
        expect(await call('getByIdVisit', mockId, mockBody)).toEqual({ status: 500, body: { errorMessage: 'An unexpected error occurred.' } });
        expect(context.error).toHaveBeenCalledWith(expect.any(String), error);
    });

    it('should not register disabled operations', () => {
        expect(registrations['listVisit']).toBeUndefined();
        expect(registrations['replaceVisit']).toBeUndefined();
        expect(registrations['deleteVisit']).toBeUndefined();
    });
});
