import { HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { NotFoundError } from '@errors';
import * as container from '@config/container';

jest.mock('@azure/functions', () => ({ app: { http: jest.fn() } }));
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

type Registration = { methods: string[]; route: string; handler: (request: HttpRequest, context: InvocationContext) => Promise<HttpResponseInit> };
let registrations: Record<string, Registration>;

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    await import('../functions/index');
    registrations = Object.fromEntries((app.http as jest.Mock).mock.calls.map(([name, options]) => [name, options]));
});

const call = async (name: string, id?: string, body?: unknown) => {
    const request = {
        url: `http://localhost/api/${name}`,
        params: id ? { id } : {},
        query: new URLSearchParams(),
        json: async () => body,
    } as unknown as HttpRequest;
    return registrations[name].handler(request, { log: jest.fn() } as unknown as InvocationContext);
};

describe('routing', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should register an anonymous health check', async () => {
        expect(registrations.health.route).toEqual('health');
        expect((await registrations.health.handler(undefined, undefined)).status).toEqual(200);
    });

    describe('cats', () => {
        const controller = () => controllers['catController'];

        it('should register list', async () => {
            expect(registrations['listCat']).toMatchObject({ methods: ['GET'], route: 'cats' });
            expect((await call('listCat')).status).toEqual(200);
            expect(controller().list).toHaveBeenCalledTimes(1);
        });

        it('should register get by id', async () => {
            expect(registrations['getByIdCat']).toMatchObject({ methods: ['GET'], route: 'cats/{id}' });
            expect((await call('getByIdCat', mockId)).status).toEqual(200);
            expect(controller().get).toHaveBeenCalledWith(mockId);
        });

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect((await call('getByIdCat', mockId)).status).toEqual(404);
        });

        it('should register create', async () => {
            expect(registrations['createCat']).toMatchObject({ methods: ['POST'], route: 'cats' });
            expect((await call('createCat', undefined, { name: 'mockName' })).status).toEqual(201);
            expect(controller().post).toHaveBeenCalledWith({ name: 'mockName' });
        });

        it('should register update with the path id taking precedence', async () => {
            expect(registrations['updateCat']).toMatchObject({ methods: ['PATCH'], route: 'cats/{id}' });
            expect((await call('updateCat', mockId, mockBody)).status).toEqual(200);
            expect(controller().update).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });

        it('should register delete', async () => {
            expect(registrations['deleteCat']).toMatchObject({ methods: ['DELETE'], route: 'cats/{id}' });
            expect((await call('deleteCat', mockId)).status).toEqual(200);
            expect(controller().delete).toHaveBeenCalledWith(mockId);
        });

        it('should not register disabled operations', () => {
            expect(registrations['replaceCat']).toBeUndefined();
        });
    });

    describe('dogs', () => {
        const controller = () => controllers['dogController'];

        it('should register list', async () => {
            expect(registrations['listDog']).toMatchObject({ methods: ['GET'], route: 'dogs' });
            expect((await call('listDog')).status).toEqual(200);
            expect(controller().list).toHaveBeenCalledTimes(1);
        });

        it('should register get by id', async () => {
            expect(registrations['getByIdDog']).toMatchObject({ methods: ['GET'], route: 'dogs/{id}' });
            expect((await call('getByIdDog', mockId)).status).toEqual(200);
            expect(controller().get).toHaveBeenCalledWith(mockId);
        });

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect((await call('getByIdDog', mockId)).status).toEqual(404);
        });

        it('should register create', async () => {
            expect(registrations['createDog']).toMatchObject({ methods: ['POST'], route: 'dogs' });
            expect((await call('createDog', undefined, { name: 'mockName' })).status).toEqual(201);
            expect(controller().post).toHaveBeenCalledWith({ name: 'mockName' });
        });

        it('should register replace with the path id taking precedence', async () => {
            expect(registrations['replaceDog']).toMatchObject({ methods: ['PUT'], route: 'dogs/{id}' });
            expect((await call('replaceDog', mockId, mockBody)).status).toEqual(200);
            expect(controller().replace).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });

        it('should register delete', async () => {
            expect(registrations['deleteDog']).toMatchObject({ methods: ['DELETE'], route: 'dogs/{id}' });
            expect((await call('deleteDog', mockId)).status).toEqual(200);
            expect(controller().delete).toHaveBeenCalledWith(mockId);
        });

        it('should not register disabled operations', () => {
            expect(registrations['updateDog']).toBeUndefined();
        });
    });
});
