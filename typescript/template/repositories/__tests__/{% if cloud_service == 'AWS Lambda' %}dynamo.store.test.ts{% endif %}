import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import { DynamoStore, dynamoStoreFactory } from '@repositories';
import { MockFn } from '../../test/mocks';

const id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const record = {
    id,
    name: 'mock',
    isDeleted: false,
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
};
const other = { ...record, id: 'cb8b2d40-edcc-4ac7-93ba-207408b23c8a' };

describe('DynamoStore', () => {
    const send = jest.fn<MockFn>();
    const client = { send } as unknown as DynamoDBDocumentClient;
    const store = new DynamoStore<typeof record>(client, 'animals');
    const input = (call: number) => send.mock.calls[call][0].input;

    beforeEach(() => {
        jest.resetAllMocks();
    });

    it('should get an item by id', async () => {
        send.mockResolvedValueOnce({ Item: record });
        expect(await store.read(id)).toEqual(record);
        expect(input(0)).toEqual({ TableName: 'animals', Key: { id } });
        send.mockResolvedValueOnce({});
        expect(await store.read(id)).toBeUndefined();
    });

    it('should scan live items', async () => {
        send.mockResolvedValue({ Items: [record, other] });
        expect(await store.query(100)).toEqual([record, other]);
        expect(input(0)).toMatchObject({ TableName: 'animals', FilterExpression: 'isDeleted = :val', Limit: 100 });
    });

    it('should return an empty list when the scan finds nothing', async () => {
        send.mockResolvedValue({ Items: undefined });
        expect(await store.query(100)).toEqual([]);
    });

    it('should keep scanning until the limit is reached or the table ends', async () => {
        send
            .mockResolvedValueOnce({ Items: [], LastEvaluatedKey: { id: 'a' } })
            .mockResolvedValueOnce({ Items: [record], LastEvaluatedKey: { id: 'b' } })
            .mockResolvedValueOnce({ Items: [other] });
        expect(await store.query(5)).toEqual([record, other]);
        expect(send).toHaveBeenCalledTimes(3);
        expect(input(1).ExclusiveStartKey).toEqual({ id: 'a' });
    });

    it('should stop scanning once the limit is reached', async () => {
        send.mockResolvedValue({ Items: [record, other], LastEvaluatedKey: { id: 'a' } });
        expect(await store.query(1)).toEqual([record]);
        expect(send).toHaveBeenCalledTimes(1);
    });

    it('should put the item on create and write', async () => {
        send.mockResolvedValue({});
        await store.create(record);
        expect(await store.write(record)).toBe(true);
        expect(input(0)).toEqual({ TableName: 'animals', Item: record });
        expect(input(1)).toEqual({ TableName: 'animals', Item: record });
    });

    it('should soft delete only a live item, setting or removing updatedBy', async () => {
        send.mockResolvedValue({});
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z', 'deleter')).toBe(true);
        expect(input(0)).toEqual({
            TableName: 'animals',
            Key: { id },
            ConditionExpression: 'isDeleted = :live',
            UpdateExpression: 'SET isDeleted = :deleted, updatedTimestamp = :ts, updatedBy = :by',
            ExpressionAttributeValues: { ':live': false, ':deleted': true, ':ts': '2025-01-01T00:00:00.000Z', ':by': 'deleter' },
        });
        await store.softDelete(id, '2025-01-01T00:00:00.000Z');
        expect(input(1)).toMatchObject({
            UpdateExpression: 'SET isDeleted = :deleted, updatedTimestamp = :ts REMOVE updatedBy',
            ExpressionAttributeValues: { ':live': false, ':deleted': true, ':ts': '2025-01-01T00:00:00.000Z' },
        });
    });

    it('should report a missing or already deleted item as not deleted', async () => {
        send.mockRejectedValue(Object.assign(new Error('The conditional request failed'), { name: 'ConditionalCheckFailedException' }));
        expect(await store.softDelete(id, '2025-01-01T00:00:00.000Z')).toBe(false);
    });

    it('should rethrow other delete failures', async () => {
        const error = new Error('down');
        send.mockRejectedValue(error);
        await expect(store.softDelete(id, '2025-01-01T00:00:00.000Z')).rejects.toBe(error);
    });

    it('should open a store per table', () => {
        expect(dynamoStoreFactory(client)('animals')).toBeInstanceOf(DynamoStore);
    });
});
