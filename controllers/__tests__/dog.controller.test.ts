import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { DogController } from '@controllers';
import { DogService, SchemaValidator } from '@services';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, invalidBodies } from '../../test/mocks';

describe('DogController', () => {
    const mockGet = jest.fn<MockFn>();
    const mockList = jest.fn<MockFn>();
    const mockCreate = jest.fn<MockFn>();
    const mockUpdate = jest.fn<MockFn>();
    const mockReplace = jest.fn<MockFn>();
    const mockDelete = jest.fn<MockFn>();

    class MockDogService {
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
    const mockService = new MockDogService() as unknown as DogService;
    const mockController = new DogController(mockService, new SchemaValidator());

    const expectNotFound = async (call: Promise<unknown>, id: string) => {
        await expect(call).rejects.toBeInstanceOf(NotFoundError);
        await expect(call).rejects.toMatchObject({ statusCode: 404, message: `Dog with id ${id} was not found.` });
    };

    const expectBadRequest = async (call: Promise<unknown>) => {
        await expect(call).rejects.toBeInstanceOf(ValidationError);
        await expect(call).rejects.toMatchObject({ statusCode: 400 });
    };

    const rejected: Record<string, unknown[]> = {
        tenantId: [42, null, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
        region: ["__invalid__", null, "EU"],
        priority: ["1", -1, 1.5, 9007199254740992],
        rank: ["1", null],
        labels: ["not-a-list", null, ["item1", "item1"]],
        name: [42, null, "", ""],
    };
    const boundaryValues: [string, unknown][] = [
        ['tenantId', "s"],
        ['tenantId', "😺"],
        ['tenantId', "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss"],
        ['tenantId', "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
        ['priority', 0],
        ['priority', 1.0],
        ['name', "s"],
        ['name', "😺"],
    ];

    describe('post', () => {
        const valid: Record<string, unknown> = { tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], name: "sample" };

        it('should pass the validated body and user id to the service', async () => {
            await mockController.post(valid, 'mockUser');
            expect(mockCreate).toHaveBeenCalledWith(valid, 'mockUser');
        });

        it('should give fields left out their defaults', async () => {
            await mockController.post({ rank: 1.5, name: "sample" });
            const sent = mockCreate.mock.calls[0][0] as Record<string, unknown>;
            expect(sent.tenantId).toEqual("public");
            expect(sent.region).toEqual("eu");
            expect(sent).not.toHaveProperty('priority');
            expect(sent.labels).toEqual([]);
        });

        it.each(invalidBodies(valid, ["rank", "name"], {}, rejected))('should return 400 for %j', async (body) => {
            await expectBadRequest(mockController.post(body));
            expect(mockCreate).not.toHaveBeenCalled();
        });

        it.each(boundaryValues.filter(([name]) => name in valid))('should accept the edge value %s = %j', async (name, value) => {
            await mockController.post({ ...valid, [name]: value });
            const sent = mockCreate.mock.calls[0][0] as Record<string, unknown>;
            expect(sent[name]).toEqual(value);
        });
    });

    describe('replace', () => {
        const valid: Record<string, unknown> = { tenantId: "public", priority: 1, rank: 1.5, labels: [], name: "sample" };

        it('should pass the validated body and user id to the service', async () => {
            await mockController.replace(mockValidGuid, valid, 'mockUser');
            expect(mockReplace).toHaveBeenCalledWith(mockValidGuid, valid, 'mockUser');
        });

        it('should give fields left out their defaults', async () => {
            await mockController.replace(mockValidGuid, { rank: 1.5, name: "sample" });
            const sent = mockReplace.mock.calls[0][1] as Record<string, unknown>;
            expect(sent.tenantId).toEqual("public");
            expect(sent).not.toHaveProperty('priority');
            expect(sent.labels).toEqual([]);
        });

        it.each(invalidBodies(valid, ["rank", "name"], { region: "eu" }, rejected))('should return 400 for %j', async (body) => {
            await expectBadRequest(mockController.replace(mockValidGuid, body));
            expect(mockReplace).not.toHaveBeenCalled();
        });

        it.each(boundaryValues.filter(([name]) => name in valid))('should accept the edge value %s = %j', async (name, value) => {
            await mockController.replace(mockValidGuid, { ...valid, [name]: value });
            const sent = mockReplace.mock.calls[0][1] as Record<string, unknown>;
            expect(sent[name]).toEqual(value);
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.replace(mockInvalidGuid, valid), mockInvalidGuid);
            expect(mockReplace).not.toHaveBeenCalled();
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
                message: `Dog with id ${mockValidGuid} was deleted successfully.`,
            });
            expect(mockDelete).toHaveBeenCalledWith(mockValidGuid, 'mockUser');
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.delete(mockInvalidGuid), mockInvalidGuid);
            expect(mockDelete).not.toHaveBeenCalled();
        });
    });
});
