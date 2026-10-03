import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { VisitController } from '@controllers';
import { VisitService, SchemaValidator } from '@services';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn, invalidBodies } from '../../test/mocks';

describe('VisitController', () => {
    const mockGet = jest.fn<MockFn>();
    const mockList = jest.fn<MockFn>();
    const mockCreate = jest.fn<MockFn>();
    const mockUpdate = jest.fn<MockFn>();
    const mockReplace = jest.fn<MockFn>();
    const mockDelete = jest.fn<MockFn>();

    class MockVisitService {
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
    const mockService = new MockVisitService() as unknown as VisitService;
    const mockController = new VisitController(mockService, new SchemaValidator());

    const expectNotFound = async (call: Promise<unknown>, id: string) => {
        await expect(call).rejects.toBeInstanceOf(NotFoundError);
        await expect(call).rejects.toMatchObject({ statusCode: 404, message: `Visit with id ${id} was not found.` });
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
        reason: [42, null],
        visitedOn: ["not-a-date", null, "2026-02-30", "2026-13-01", "0000-01-01"],
        cost: ["1", null, -1],
        paid: ["true", null],
        checkedAt: ["not-a-list", null, [null]],
    };
    const boundaryValues: [string, unknown][] = [
        ['tenantId', "s"],
        ['tenantId', "😺"],
        ['tenantId', "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss"],
        ['tenantId', "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
        ['priority', 0],
        ['priority', 1.0],
        ['cost', 0],
    ];

    describe('post', () => {
        const valid: Record<string, unknown> = { tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], reason: "sample", visitedOn: "2026-01-01", cost: 1.5 };

        it('should pass the validated body and user id to the service', async () => {
            await mockController.post(valid, 'mockUser');
            expect(mockCreate).toHaveBeenCalledWith(valid, 'mockUser');
        });

        it('should give fields left out their defaults', async () => {
            await mockController.post({ rank: 1.5, reason: "sample", visitedOn: "2026-01-01" });
            const sent = mockCreate.mock.calls[0][0] as Record<string, unknown>;
            expect(sent.tenantId).toEqual("public");
            expect(sent.region).toEqual("eu");
            expect(sent).not.toHaveProperty('priority');
            expect(sent.labels).toEqual([]);
            expect(sent).not.toHaveProperty('cost');
        });

        it.each(invalidBodies(valid, ["rank", "reason", "visitedOn"], { paid: false, checkedAt: ["2026-01-15T10:00:00.000Z"] }, rejected))('should return 400 for %j', async (body) => {
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
        const valid: Record<string, unknown> = { tenantId: "public", priority: 1, rank: 1.5, labels: [], visitedOn: "2026-01-01", cost: 1.5, paid: false, checkedAt: ["2026-01-15T10:00:00.000Z"] };

        it('should pass the validated body and user id to the service', async () => {
            await mockController.update(mockValidGuid, valid, 'mockUser');
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, valid, 'mockUser');
        });

        it('should accept an empty body and leave every field out', async () => {
            await mockController.update(mockValidGuid, {});
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, {}, undefined);
        });

        it('should pass null to clear priority', async () => {
            await mockController.update(mockValidGuid, { priority: null });
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, { priority: null }, undefined);
        });

        it.each(invalidBodies(valid, [], { region: "eu", reason: "sample" }, rejected))('should return 400 for %j', async (body) => {
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
});
