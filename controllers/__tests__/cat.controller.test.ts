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
        tenantId: [42, null, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
        region: ["__invalid__", null, "EU"],
        priority: ["1", -1, 1.5, 9007199254740992],
        rank: ["1", null],
        labels: ["not-a-list", null, ["item1", "item1"]],
        name: [42, null, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
        breed: ["__invalid__", null, "SIAMESE"],
        ageYears: ["1", null, -1, 41, 1.5, 9007199254740992],
        weightKg: ["1", 0, 100],
        indoor: ["true", null],
        birthDate: ["not-a-date", null, "2026-02-30", "2026-13-01", "0000-01-01"],
        microchipId: ["not-a-uuid", null],
        ownerEmail: [42, null, "not-an-email"],
        website: [42, "not a uri"],
        tagCode: [42, null, "!"],
        tags: ["not-a-list", null, ["item1", "item1"], ["item1", "item2", "item3", "item4"]],
        scores: ["not-a-list", [null], []],
        adoptedAt: ["not-a-date-time", "2026-01-31T09:30:00", "2026-01-31T24:00:00Z", "2026-01-31T23:59:60Z", "2026-01-31T09:30:00+14:60", "0000-12-31T23:00:00-01:00", "0001-01-01T00:00:00+01:00", "9999-12-31T23:59:59-01:00"],
        lastVisit: ["not-a-date-time", null, "2026-01-31T09:30:00", "2026-01-31T24:00:00Z", "2026-01-31T23:59:60Z", "2026-01-31T09:30:00+14:60", "0000-12-31T23:00:00-01:00", "0001-01-01T00:00:00+01:00", "9999-12-31T23:59:59-01:00"],
        notes: [42, null],
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
        ['name', "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss"],
        ['name', "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
        ['ageYears', 0],
        ['ageYears', 40],
        ['ageYears', 0.0],
        ['tags', ["item1", "item2", "item3"]],
        ['scores', [1]],
        ['adoptedAt', "0001-01-01T00:00:00.000Z"],
        ['adoptedAt', "9999-12-31T23:59:59.999Z"],
        ['lastVisit', "0001-01-01T00:00:00.000Z"],
        ['lastVisit', "9999-12-31T23:59:59.999Z"],
    ];

    describe('post', () => {
        const valid: Record<string, unknown> = { tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], name: "sample", breed: "tabby", ageYears: 0, weightKg: 1.5, indoor: true, birthDate: "2026-01-01", microchipId: "6f1c2a3b-4d5e-4f60-8a7b-000000000000", ownerEmail: "unknown@example.com", website: "https://example.com/items/1", tagCode: "ABC-123", tags: [], scores: [1], adoptedAt: "2026-01-15T10:00:00.000Z", lastVisit: "2026-01-01T00:00:00.000Z", notes: "$none" };

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
            expect(sent.breed).toEqual("tabby");
            expect(sent.ageYears).toEqual(0);
            expect(sent.weightKg).toEqual(null);
            expect(sent.indoor).toEqual(true);
            expect(sent.birthDate).toEqual(expect.any(String));
            expect(sent.microchipId).toEqual(expect.any(String));
            expect(sent.ownerEmail).toEqual("unknown@example.com");
            expect(sent).not.toHaveProperty('website');
            expect(sent).not.toHaveProperty('tagCode');
            expect(sent.tags).toEqual([]);
            expect(sent).not.toHaveProperty('scores');
            expect(sent.adoptedAt).toEqual(expect.any(String));
            expect(sent.lastVisit).toEqual("2026-01-01T00:00:00.000Z");
            expect(sent.notes).toEqual("$none");
        });

        it('should hold adoptedAt in UTC with milliseconds', async () => {
            await mockController.post({ ...valid, adoptedAt: "2026-01-31T11:30:00.1239+02:00" });
            const sent = mockCreate.mock.calls[0][0] as Record<string, unknown>;
            expect(sent.adoptedAt).toEqual("2026-01-31T09:30:00.123Z");
        });

        it('should hold lastVisit in UTC with milliseconds', async () => {
            await mockController.post({ ...valid, lastVisit: "2026-01-31T11:30:00.1239+02:00" });
            const sent = mockCreate.mock.calls[0][0] as Record<string, unknown>;
            expect(sent.lastVisit).toEqual("2026-01-31T09:30:00.123Z");
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

    describe('update', () => {
        const valid: Record<string, unknown> = { tenantId: "public", priority: 1, rank: 1.5, labels: [], name: "sample", ageYears: 0, weightKg: 1.5, indoor: true, ownerEmail: "unknown@example.com", website: "https://example.com/items/1", tags: [], adoptedAt: "2026-01-15T10:00:00.000Z", notes: "$none" };

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

        it('should pass null to clear weightKg', async () => {
            await mockController.update(mockValidGuid, { weightKg: null });
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, { weightKg: null }, undefined);
        });

        it('should pass null to clear website', async () => {
            await mockController.update(mockValidGuid, { website: null });
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, { website: null }, undefined);
        });

        it('should pass null to clear adoptedAt', async () => {
            await mockController.update(mockValidGuid, { adoptedAt: null });
            expect(mockUpdate).toHaveBeenCalledWith(mockValidGuid, { adoptedAt: null }, undefined);
        });

        it('should hold adoptedAt in UTC with milliseconds', async () => {
            await mockController.update(mockValidGuid, { ...valid, adoptedAt: "2026-01-31T11:30:00.1239+02:00" });
            const sent = mockUpdate.mock.calls[0][1] as Record<string, unknown>;
            expect(sent.adoptedAt).toEqual("2026-01-31T09:30:00.123Z");
        });

        it.each(invalidBodies(valid, [], { region: "eu", breed: "tabby", birthDate: "2026-01-01", microchipId: "6f1c2a3b-4d5e-4f60-8a7b-000000000000", tagCode: "ABC-123", scores: [1], lastVisit: "2026-01-01T00:00:00.000Z" }, rejected))('should return 400 for %j', async (body) => {
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

    describe('replace', () => {
        const valid: Record<string, unknown> = { tenantId: "public", priority: 1, rank: 1.5, labels: [], name: "sample", breed: "tabby", ageYears: 0, weightKg: 1.5, indoor: true, birthDate: "2026-01-01", ownerEmail: "unknown@example.com", website: "https://example.com/items/1", tagCode: "ABC-123", tags: [], scores: [1], adoptedAt: "2026-01-15T10:00:00.000Z", lastVisit: "2026-01-01T00:00:00.000Z", notes: "$none" };

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
            expect(sent.breed).toEqual("tabby");
            expect(sent.ageYears).toEqual(0);
            expect(sent.weightKg).toEqual(null);
            expect(sent.indoor).toEqual(true);
            expect(sent.birthDate).toEqual(expect.any(String));
            expect(sent.ownerEmail).toEqual("unknown@example.com");
            expect(sent).not.toHaveProperty('website');
            expect(sent).not.toHaveProperty('tagCode');
            expect(sent.tags).toEqual([]);
            expect(sent).not.toHaveProperty('scores');
            expect(sent.adoptedAt).toEqual(expect.any(String));
            expect(sent.lastVisit).toEqual("2026-01-01T00:00:00.000Z");
            expect(sent.notes).toEqual("$none");
        });

        it('should hold adoptedAt in UTC with milliseconds', async () => {
            await mockController.replace(mockValidGuid, { ...valid, adoptedAt: "2026-01-31T11:30:00.1239+02:00" });
            const sent = mockReplace.mock.calls[0][1] as Record<string, unknown>;
            expect(sent.adoptedAt).toEqual("2026-01-31T09:30:00.123Z");
        });

        it('should hold lastVisit in UTC with milliseconds', async () => {
            await mockController.replace(mockValidGuid, { ...valid, lastVisit: "2026-01-31T11:30:00.1239+02:00" });
            const sent = mockReplace.mock.calls[0][1] as Record<string, unknown>;
            expect(sent.lastVisit).toEqual("2026-01-31T09:30:00.123Z");
        });

        it.each(invalidBodies(valid, ["rank", "name"], { region: "eu", microchipId: "6f1c2a3b-4d5e-4f60-8a7b-000000000000" }, rejected))('should return 400 for %j', async (body) => {
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
