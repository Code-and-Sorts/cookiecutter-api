import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { CatRepository, StoreFactory } from '@repositories';
import { MockFn, mockStore } from '../../test/mocks';

describe('CatRepository', () => {
    const opened: string[] = [];
    const openStore = (name: string) => {
        opened.push(name);
        return mockStore();
    };
    const mockRepository = new CatRepository(openStore as unknown as StoreFactory);
    // Untyped view, since the base methods are protected.
    const base = mockRepository as unknown as Record<string, MockFn>;
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockFields = { tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], name: "sample", breed: "tabby", ageYears: 0, weightKg: 1.5, indoor: true, birthDate: "2026-01-01", microchipId: "6f1c2a3b-4d5e-4f60-8a7b-000000000000", ownerEmail: "unknown@example.com", website: "https://example.com/items/1", tagCode: "ABC-123", tags: [], scores: [1], adoptedAt: "2026-01-15T10:00:00.000Z", lastVisit: "2026-01-01T00:00:00.000Z", notes: "$none" };
    const mockRecord = {
        id: mockId,
        ...mockFields,
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };

    beforeEach(() => {
        jest.restoreAllMocks();
    });

    it('should open the store named by its container setting', () => {
        expect(opened).toEqual(['cats']);
    });

    it('should expose only its resource operations', () => {
        const operations = ['create', 'get', 'list', 'update', 'replace', 'delete'];
        const actual = operations.filter((operation) => typeof base[operation] === 'function');
        expect(actual).toEqual(['create', 'get', 'list', 'update', 'replace', 'delete']);
    });

    it('should name the resource in not found errors', () => {
        const error = (base['notFound'] as (id: string) => Error)(mockId);
        expect(error.message).toEqual(`Cat with id ${mockId} was not found.`);
    });

    it('should delegate create to the base repository', async () => {
        const spy = jest.spyOn(base, 'addRecord').mockResolvedValue(mockRecord);
        expect(await mockRepository.create(mockFields as never, 'mockUser')).toEqual(mockRecord);
        expect(spy).toHaveBeenCalledWith(mockFields, 'mockUser');
    });

    it('should delegate get to the base repository', async () => {
        const spy = jest.spyOn(base, 'getRecord').mockResolvedValue(mockRecord);
        expect(await mockRepository.get(mockId)).toEqual(mockRecord);
        expect(spy).toHaveBeenCalledWith(mockId);
    });

    it('should delegate list to the base repository', async () => {
        const spy = jest.spyOn(base, 'getRecords').mockResolvedValue([mockRecord]);
        expect(await mockRepository.list(5)).toEqual([mockRecord]);
        expect(spy).toHaveBeenCalledWith(5);
    });

    it('should delegate update to the base repository', async () => {
        const spy = jest.spyOn(base, 'updateRecord').mockResolvedValue(mockRecord);
        expect(await mockRepository.update(mockId, mockFields as never, 'mockUser')).toEqual(mockRecord);
        expect(spy).toHaveBeenCalledWith(mockId, mockFields, 'mockUser');
    });

    it('should delegate replace to the base repository', async () => {
        const spy = jest.spyOn(base, 'replaceRecord').mockResolvedValue(mockRecord);
        expect(await mockRepository.replace(mockId, mockFields as never, 'mockUser')).toEqual(mockRecord);
        expect(spy).toHaveBeenCalledWith(mockId, mockFields, ["tenantId", "priority", "rank", "labels", "name", "breed", "ageYears", "weightKg", "indoor", "birthDate", "ownerEmail", "website", "tagCode", "tags", "scores", "adoptedAt", "lastVisit", "notes"], 'mockUser');
    });

    it('should delegate delete to the base repository', async () => {
        const spy = jest.spyOn(base, 'deleteRecord').mockResolvedValue(undefined);
        await mockRepository.delete(mockId, 'mockUser');
        expect(spy).toHaveBeenCalledWith(mockId, 'mockUser');
    });
});
