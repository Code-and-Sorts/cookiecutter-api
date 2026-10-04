import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { Container, Database } from '@azure/cosmos';
import { CosmosStore, cosmosStoreFactory, isMissingItem } from '@repositories';
import { MockFn } from '../../test/mocks';

const id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const record = {
    id,
    name: 'mock',
    isDeleted: false,
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
};
const read = { ...record, _etag: 'etag-1' };
const ifMatch = { accessCondition: { type: 'IfMatch', condition: 'etag-1' } };
const cosmosError = (code: number, substatus?: number) => Object.assign(new Error('Cosmos error'), { code, substatus });
const containerMissing = () => cosmosError(404, 1003);

describe('CosmosStore', () => {
    const query = jest.fn<MockFn>();
    const fetchAll = jest.fn<MockFn>();
    const create = jest.fn<MockFn>();
    const replace = jest.fn<MockFn>();
    const patch = jest.fn<MockFn>();
    const item = jest.fn<MockFn>();
    const container = { items: { query, create }, item } as unknown as Container;
    const store = new CosmosStore<typeof record>(container);

    beforeEach(() => {
        jest.resetAllMocks();
        query.mockReturnValue({ fetchAll });
        item.mockReturnValue({ replace, patch });
    });

    it('should read a live record by id', async () => {
        fetchAll.mockResolvedValue({ resources: [record] });
        expect(await store.read(id)).toEqual(record);
        expect(query).toHaveBeenCalledWith({
            query: 'SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false',
            parameters: [{ name: '@id', value: id }],
        });
        fetchAll.mockResolvedValue({ resources: [] });
        expect(await store.read(id)).toBeUndefined();
    });

    it('should query live records up to the limit', async () => {
        fetchAll.mockResolvedValue({ resources: [record] });
        expect(await store.query(100)).toEqual([record]);
        expect(query).toHaveBeenCalledWith({ query: 'SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100' });
    });

    it('should create an item', async () => {
        await store.create(record);
        expect(create).toHaveBeenCalledWith(record);
    });

    it('should replace an item by id and partition key only if the ETag it was read with still matches', async () => {
        expect(await store.write(record, read)).toBe(true);
        expect(item).toHaveBeenCalledWith(id, id);
        expect(replace).toHaveBeenCalledWith(record, ifMatch);
    });

    it('should report a write to a vanished item as not found', async () => {
        replace.mockRejectedValue(cosmosError(404));
        expect(await store.write(record, read)).toBe(false);
    });

    it('should rethrow a write that lost a race', async () => {
        replace.mockRejectedValue(cosmosError(412));
        await expect(store.write(record, read)).rejects.toMatchObject({ code: 412 });
    });

    it('should soft delete only a live item, setting or removing updatedBy', async () => {
        const operations = [
            { op: 'set', path: '/isDeleted', value: true },
            { op: 'set', path: '/updatedTimestamp', value: '2025-01-01T00:00:00.000Z' },
        ];
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z', 'deleter')).toBe(true);
        expect(item).toHaveBeenCalledWith(id, id);
        expect(patch).toHaveBeenCalledWith({
            condition: 'FROM c WHERE c.isDeleted = false',
            operations: [...operations, { op: 'set', path: '/updatedBy', value: 'deleter' }],
        });
        await store.softDelete(id, '2025-01-01T00:00:00.000Z');
        expect(patch).toHaveBeenLastCalledWith({
            condition: 'FROM c WHERE c.isDeleted = false',
            operations: [...operations, { op: 'set', path: '/updatedBy', value: '' }, { op: 'remove', path: '/updatedBy' }],
        });
    });

    it('should report a missing or already deleted item as not deleted', async () => {
        patch.mockRejectedValueOnce(cosmosError(404, 0));
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z')).toBe(false);
        patch.mockRejectedValueOnce(cosmosError(412));
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z')).toBe(false);
    });

    it('should rethrow a missing container and other failures', async () => {
        replace.mockRejectedValue(containerMissing());
        await expect(store.write(record, read)).rejects.toMatchObject({ substatus: 1003 });
        patch.mockRejectedValue(cosmosError(503));
        await expect(store.softDelete(id, '2025-01-01T00:00:00.000Z')).rejects.toMatchObject({ code: 503 });
    });

    it('should only treat an item-level 404 as a missing record', () => {
        expect(isMissingItem({ code: 404 })).toBe(true);
        expect(isMissingItem(cosmosError(404, 0))).toBe(true);
        expect(isMissingItem(containerMissing())).toBe(false);
        expect(isMissingItem({ code: 412 })).toBe(false);
        expect(isMissingItem(undefined)).toBe(false);
    });

    it('should open a store per container', async () => {
        const database = { container: jest.fn<MockFn>().mockReturnValue(container) };
        const opened = cosmosStoreFactory(database as unknown as Database)('animals');
        expect(opened).toBeInstanceOf(CosmosStore);
        expect(database.container).toHaveBeenCalledWith('animals');
    });
});
