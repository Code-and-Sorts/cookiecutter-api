import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { BaseRepository, DocumentStore } from '@repositories';
import { NotFoundError, ProxyError } from '@errors';
import { BaseItemRecord } from '@models';
import { DATABASE_DEADLINE_MS, DEFAULT_LIST_LIMIT } from '@utils';
import { mockStore } from '../../test/mocks';

type Item = BaseItemRecord & { name: string };

// Exposes the protected methods so they are tested whichever operations resources use.
class TestRepository extends BaseRepository<Item> {
    declare public addRecord: (fields: { name: string }, userId?: string) => Promise<Item>;
    declare public getRecord: (id: string) => Promise<Item>;
    declare public getRecords: (limit?: number) => Promise<Item[]>;
    declare public updateRecord: (id: string, fields: { name?: string }, userId?: string) => Promise<Item>;
    declare public replaceRecord: (id: string, fields: { name: string }, userId?: string) => Promise<Item>;
    declare public deleteRecord: (id: string, userId?: string) => Promise<void>;
}

const id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const isoTimestamp = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/;
const lostRace = Object.assign(new Error('Precondition failed'), { code: 412 });
const stored: Item = {
    id,
    name: 'stored',
    isDeleted: false,
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
};

describe('BaseRepository', () => {
    let store: ReturnType<typeof mockStore>;
    let repository: TestRepository;

    beforeEach(() => {
        jest.restoreAllMocks();
        store = mockStore();
        store.write.mockResolvedValue(true);
        store.softDelete.mockResolvedValue(true);
        repository = new TestRepository(store as unknown as DocumentStore<Item>, 'Item');
    });

    const expectProxyError = async (call: Promise<unknown>, message: string, cause: unknown) => {
        await expect(call).rejects.toBeInstanceOf(ProxyError);
        await expect(call).rejects.toMatchObject({ statusCode: 502, message, cause });
    };

    const expectNotFound = async (call: Promise<unknown>) => {
        await expect(call).rejects.toBeInstanceOf(NotFoundError);
        await expect(call).rejects.toMatchObject({ statusCode: 404, message: `Item with id ${id} was not found.` });
    };

    describe('addRecord', () => {
        it('should store a new live record with a UUID, one timestamp and the user id for both fields', async () => {
            // A second clock reading would differ, so equal timestamps prove the clock was read once.
            jest.spyOn(Date.prototype, 'toISOString')
                .mockReturnValueOnce('2026-01-01T00:00:00.000Z')
                .mockReturnValueOnce('2026-01-01T00:00:00.001Z');
            const result = await repository.addRecord({ name: 'new' }, 'creator');
            expect(result).toEqual({
                id: expect.stringMatching(/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/),
                name: 'new',
                isDeleted: false,
                createdTimestamp: '2026-01-01T00:00:00.000Z',
                updatedTimestamp: '2026-01-01T00:00:00.000Z',
                createdBy: 'creator',
                updatedBy: 'creator',
            });
            expect(store.create).toHaveBeenCalledWith(result);
        });

        it('should omit createdBy and updatedBy without a user id', async () => {
            const result = await repository.addRecord({ name: 'new' });
            expect('createdBy' in result || 'updatedBy' in result).toBe(false);
        });

        it('should wrap a store failure in a proxy error', async () => {
            const cause = new Error('down');
            store.create.mockRejectedValue(cause);
            await expectProxyError(repository.addRecord({ name: 'new' }), 'Error creating item in database.', cause);
        });
    });

    describe('getRecord', () => {
        it('should return a live record', async () => {
            store.read.mockResolvedValue(stored);
            expect(await repository.getRecord(id)).toEqual(stored);
            expect(store.read).toHaveBeenCalledWith(id);
        });

        it('should report missing and soft-deleted records as not found', async () => {
            store.read.mockResolvedValueOnce(undefined);
            await expectNotFound(repository.getRecord(id));
            store.read.mockResolvedValue({ ...stored, isDeleted: true });
            await expectNotFound(repository.getRecord(id));
        });

        it('should wrap a store failure in a proxy error', async () => {
            const cause = new Error('down');
            store.read.mockRejectedValue(cause);
            await expectProxyError(repository.getRecord(id), 'Error retrieving item from database.', cause);
        });
    });

    describe('getRecords', () => {
        it('should query with the given or default limit', async () => {
            store.query.mockResolvedValue([stored]);
            expect(await repository.getRecords(5)).toEqual([stored]);
            expect(store.query).toHaveBeenCalledWith(5);
            await repository.getRecords();
            expect(store.query).toHaveBeenLastCalledWith(DEFAULT_LIST_LIMIT);
        });

        it('should wrap a store failure in a proxy error', async () => {
            const cause = new Error('down');
            store.query.mockRejectedValue(cause);
            await expectProxyError(repository.getRecords(), 'Error retrieving items from database.', cause);
        });
    });

    describe('updateRecord', () => {
        it('should merge the fields and refresh updatedTimestamp and updatedBy only', async () => {
            store.read.mockResolvedValue(stored);
            const result = await repository.updateRecord(id, { name: 'updated' }, 'editor');
            expect(result).toEqual({ ...stored, name: 'updated', updatedBy: 'editor', updatedTimestamp: expect.stringMatching(isoTimestamp) });
            expect(result.updatedTimestamp).not.toEqual(stored.updatedTimestamp);
            expect(store.write).toHaveBeenCalledWith(result, stored);
        });

        it('should drop the stored updatedBy without a user id', async () => {
            store.read.mockResolvedValue(stored);
            const result = await repository.updateRecord(id, { name: 'updated' });
            expect(result.createdBy).toEqual(stored.createdBy);
            expect('updatedBy' in result).toBe(false);
        });

        it('should report a missing record or one that vanished before the write as not found', async () => {
            store.read.mockResolvedValueOnce(undefined);
            await expectNotFound(repository.updateRecord(id, { name: 'updated' }));
            store.read.mockResolvedValue(stored);
            store.write.mockResolvedValue(false);
            await expectNotFound(repository.updateRecord(id, { name: 'updated' }));
        });

        it('should wrap a store failure, such as a write that lost a race, in a proxy error', async () => {
            const cause = lostRace;
            store.read.mockResolvedValue(stored);
            store.write.mockRejectedValue(cause);
            await expectProxyError(repository.updateRecord(id, { name: 'updated' }), `Error upserting item with id ${id}.`, cause);
        });
    });

    describe('replaceRecord', () => {
        it('should write the fields as a live record that keeps the stored created fields', async () => {
            store.read.mockResolvedValue(stored);
            const result = await repository.replaceRecord(id, { name: 'replaced' }, 'editor');
            expect(result).toEqual({
                id,
                name: 'replaced',
                isDeleted: false,
                createdBy: 'mockUser',
                updatedBy: 'editor',
                createdTimestamp: stored.createdTimestamp,
                updatedTimestamp: expect.stringMatching(isoTimestamp),
            });
            expect(store.write).toHaveBeenCalledWith(result, stored);
        });

        it('should omit createdBy when the stored record has none and updatedBy without a user id', async () => {
            const { createdBy: _createdBy, ...withoutCreatedBy } = stored;
            store.read.mockResolvedValue(withoutCreatedBy);
            const result = await repository.replaceRecord(id, { name: 'replaced' });
            expect('createdBy' in result || 'updatedBy' in result).toBe(false);
        });

        it('should report a missing record as not found', async () => {
            store.read.mockResolvedValue(undefined);
            await expectNotFound(repository.replaceRecord(id, { name: 'replaced' }));
            expect(store.write).not.toHaveBeenCalled();
        });

        it('should wrap a store failure, such as a write that lost a race, in a proxy error', async () => {
            const cause = lostRace;
            store.read.mockResolvedValue(stored);
            store.write.mockRejectedValue(cause);
            await expectProxyError(repository.replaceRecord(id, { name: 'replaced' }), `Error replacing item with id ${id}.`, cause);
        });
    });

    describe('deleteRecord', () => {
        it('should soft delete with a fresh updatedTimestamp and the user id, if any', async () => {
            await repository.deleteRecord(id, 'deleter');
            expect(store.softDelete).toHaveBeenCalledWith(id, expect.stringMatching(isoTimestamp), 'deleter');
            await repository.deleteRecord(id);
            expect(store.softDelete).toHaveBeenLastCalledWith(id, expect.stringMatching(isoTimestamp), undefined);
        });

        it('should report a missing or already deleted record as not found', async () => {
            store.softDelete.mockResolvedValue(false);
            await expectNotFound(repository.deleteRecord(id));
        });

        it('should wrap a store failure in a proxy error', async () => {
            const cause = new Error('down');
            store.softDelete.mockRejectedValue(cause);
            await expectProxyError(repository.deleteRecord(id), `Error deleting record with id ${id}.`, cause);
        });
    });

    it('should fail an operation that outlives the database deadline', async () => {
        jest.useFakeTimers();
        try {
            store.read.mockReturnValue(new Promise(() => undefined));
            const assertion = expect(repository.getRecord(id)).rejects.toBeInstanceOf(ProxyError);
            jest.advanceTimersByTime(DATABASE_DEADLINE_MS);
            await assertion;
        } finally {
            jest.useRealTimers();
        }
    });
});
