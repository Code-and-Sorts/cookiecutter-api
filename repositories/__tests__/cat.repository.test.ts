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
    const mockFields = { name: 'mockCat' };
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
        expect(opened).toEqual(['animals']);
    });

    it('should expose only its resource operations', () => {
        const operations = ['create', 'get', 'list', 'update', 'replace', 'delete'];
        const actual = operations.filter((operation) => typeof base[operation] === 'function');
        expect(actual).toEqual(['create', 'get', 'list', 'update', 'delete']);
    });

    it('should name the resource in not found errors', () => {
        const error = (base['notFound'] as (id: string) => Error)(mockId);
        expect(error.message).toEqual(`Cat with id ${mockId} was not found.`);
    });

    it('should delegate create to the base repository', async () => {
        const spy = jest.spyOn(base, 'addRecord').mockResolvedValue(mockRecord);
        expect(await mockRepository.create(mockFields, 'mockUser')).toEqual(mockRecord);
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
        expect(await mockRepository.update(mockId, mockFields, 'mockUser')).toEqual(mockRecord);
        expect(spy).toHaveBeenCalledWith(mockId, mockFields, 'mockUser');
    });

    it('should delegate delete to the base repository', async () => {
        const spy = jest.spyOn(base, 'deleteRecord').mockResolvedValue(undefined);
        await mockRepository.delete(mockId, 'mockUser');
        expect(spy).toHaveBeenCalledWith(mockId, 'mockUser');
    });
});
