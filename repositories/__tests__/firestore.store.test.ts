import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { CollectionReference, FieldValue, Firestore } from '@google-cloud/firestore';
import { FirestoreStore, firestoreStoreFactory } from '@repositories';
import { MockFn } from '../../test/mocks';

const id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const record = {
    id,
    name: 'mock',
    isDeleted: false,
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
};
const snapshot = (data?: object) => ({ exists: data !== undefined, data: () => data });

describe('FirestoreStore', () => {
    const get = jest.fn<MockFn>();
    const set = jest.fn<MockFn>();
    const update = jest.fn<MockFn>();
    const doc = jest.fn<MockFn>();
    const where = jest.fn<MockFn>();
    const limit = jest.fn<MockFn>();
    const queryGet = jest.fn<MockFn>();
    const collection = { doc, where } as unknown as CollectionReference;
    const store = new FirestoreStore<typeof record>(collection);

    beforeEach(() => {
        jest.resetAllMocks();
        doc.mockReturnValue({ get, set, update });
        where.mockReturnValue({ limit });
        limit.mockReturnValue({ get: queryGet });
    });

    it('should read a document by id', async () => {
        get.mockResolvedValueOnce(snapshot(record));
        expect(await store.read(id)).toEqual(record);
        expect(doc).toHaveBeenCalledWith(id);
        get.mockResolvedValueOnce(snapshot());
        expect(await store.read(id)).toBeUndefined();
    });

    it('should query live documents up to the limit', async () => {
        queryGet.mockResolvedValue({ docs: [snapshot(record)] });
        expect(await store.query(5)).toEqual([record]);
        expect(where).toHaveBeenCalledWith('isDeleted', '==', false);
        expect(limit).toHaveBeenCalledWith(5);
    });

    it('should set the document on create and write', async () => {
        await store.create(record);
        expect(await store.write(record)).toBe(true);
        expect(doc).toHaveBeenCalledWith(id);
        expect(set).toHaveBeenCalledTimes(2);
        expect(set).toHaveBeenCalledWith(record);
    });

    it('should soft delete only a live document, setting or removing updatedBy', async () => {
        get.mockResolvedValue(snapshot(record));
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z', 'deleter')).toBe(true);
        expect(update).toHaveBeenCalledWith({ isDeleted: true, updatedTimestamp: '2025-01-01T00:00:00.000Z', updatedBy: 'deleter' });
        await store.softDelete(id, '2025-01-01T00:00:00.000Z');
        expect(update).toHaveBeenLastCalledWith({
            isDeleted: true,
            updatedTimestamp: '2025-01-01T00:00:00.000Z',
            updatedBy: FieldValue.delete(),
        });
    });

    it('should report a missing or already deleted document as not deleted', async () => {
        get.mockResolvedValueOnce(snapshot());
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z')).toBe(false);
        get.mockResolvedValueOnce(snapshot({ ...record, isDeleted: true }));
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z')).toBe(false);
        expect(update).not.toHaveBeenCalled();
    });

    it('should open a store per collection', () => {
        const client = { collection: jest.fn<MockFn>().mockReturnValue(collection) };
        expect(firestoreStoreFactory(client as unknown as Firestore)('animals')).toBeInstanceOf(FirestoreStore);
        expect(client.collection).toHaveBeenCalledWith('animals');
    });
});
