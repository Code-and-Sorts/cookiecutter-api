import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { CatController } from '@controllers';
import { CatService, SchemaValidator } from '@services';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, invalidBodies } from '../../test/mocks';

describe('CatController', () => {
    const mockGet = jest.fn<MockFn>();
    const mockList = jest.fn<MockFn>();
    const mockCreate = jest.fn<MockFn>();
    const mockUpdate = jest.fn<MockFn>();
    const mockReplace = jest.fn<MockFn>();
    const mockDelete = jest.fn<MockFn>();

    class MockCatService {
        get = mockGet;
        list = mockList;
        create = mockCreate;
        update = mockUpdate;
        replace = mockReplace;
        delete = mockDelete;
    }

    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockValidGuid = '91ed1c70-5412-449a-b949-80542a4eb3d5';
    const mockInvalidGuid = 'invalid-guid';
    const mockService = new MockCatService() as unknown as CatService;
    const mockController = new CatController(mockService, new SchemaValidator());

    const expectNotFound = async (call: Promise<unknown>, id: string) => {
        await expect(call).rejects.toBeInstanceOf(NotFoundError);
        await expect(call).rejects.toMatchObject({ statusCode: 404, message: `Cat with id ${id} was not found.` });
    };

    const expectBadRequest = async (call: Promise<unknown>) => {
        await expect(call).rejects.toBeInstanceOf(ValidationError);
        await expect(call).rejects.toMatchObject({ statusCode: 400 });
    };

    const rejected: Record<string, unknown[]> = {
        name: [42, null, "", ""],
    };
    const boundaryValues: [string, unknown][] = [
        ['name', "s"],
        ['name', "😺"],
    ];

    describe('post', () => {
        const valid: Record<string, unknown> = { name: "sample" };

        it('should pass the validated body and user id to the service', async () => {
            await mockController.post(valid, 'mockUser');
            expect(mockCreate).toHaveBeenCalledWith(valid, 'mockUser');
        });

        it.each(invalidBodies(valid, ["name"], {}, rejected))('should return 400 for %j', async (body) => {
            await expectBadRequest(mockController.post(body));
            expect(mockCreate).not.toHaveBeenCalled();
        });

        it.each(boundaryValues.filter(([name]) => name in valid))('should accept the edge value %s = %j', async (name, value) => {
            await mockController.post({ ...valid, [name]: value });
            const sent = mockCreate.mock.calls[0][0] as Record<string, unknown>;
            expect(sent[name]).toEqual(value);
        });
    });

    describe('update', () => {
        const valid: Record<string, unknown> = { name: "sample" };

        it('should pass the validated body and user id to the service', async () => {
            await mockController.update(mockValidGuid, valid, 'mockUser');
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, valid, 'mockUser');
        });

        it('should accept an empty body and leave every field out', async () => {
            await mockController.update(mockValidGuid, {});
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, {}, undefined);
        });

        it.each(invalidBodies(valid, [], {}, rejected))('should return 400 for %j', async (body) => {
            await expectBadRequest(mockController.update(mockValidGuid, body));
            expect(mockUpdate).not.toHaveBeenCalled();
        });

        it.each(boundaryValues.filter(([name]) => name in valid))('should accept the edge value %s = %j', async (name, value) => {
            await mockController.update(mockValidGuid, { ...valid, [name]: value });
            const sent = mockUpdate.mock.calls[0][1] as Record<string, unknown>;
            expect(sent[name]).toEqual(value);
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.update(mockInvalidGuid, valid), mockInvalidGuid);
            expect(mockUpdate).not.toHaveBeenCalled();
        });
    });

    describe('get', () => {
        it('should call the service with the id', async () => {
            await mockController.get(mockValidGuid);
            expect(mockGet).toHaveBeenCalledWith(mockValidGuid);
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.get(mockInvalidGuid), mockInvalidGuid);
            expect(mockGet).not.toHaveBeenCalled();
        });
    });

    describe('list', () => {
        it('should default the limit when not provided', async () => {
            await mockController.list();
            expect(mockList).toHaveBeenCalledWith(100);
        });

        it('should coerce and clamp the limit query param', async () => {
            await mockController.list('5');
            expect(mockList).toHaveBeenCalledWith(5);

            await mockController.list('999999');
            expect(mockList).toHaveBeenCalledWith(1000);

            await mockController.list('abc');
            expect(mockList).toHaveBeenCalledWith(100);
        });
    });

    describe('delete', () => {
        it('should call the service and return the confirmation message', async () => {
            expect(await mockController.delete(mockValidGuid, 'mockUser')).toEqual({
                message: `Cat with id ${mockValidGuid} was deleted successfully.`,
            });
            expect(mockDelete).toHaveBeenCalledWith(mockValidGuid, 'mockUser');
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.delete(mockInvalidGuid), mockInvalidGuid);
            expect(mockDelete).not.toHaveBeenCalled();
        });
    });
});
