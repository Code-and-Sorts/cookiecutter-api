import { DynamoDBDocumentClient, GetCommand, PutCommand, ScanCommand, UpdateCommand } from '@aws-sdk/lib-dynamodb';
import { BaseItemRecord } from '@models';
import { DocumentStore, StoreFactory } from './document.store';

export class DynamoStore<T extends BaseItemRecord> implements DocumentStore<T> {
  constructor(
    private readonly client: DynamoDBDocumentClient,
    private readonly tableName: string,
  ) {}

  read = async (id: string): Promise<T | undefined> => {
    const { Item } = await this.client.send(new GetCommand({ TableName: this.tableName, Key: { id } }));
    return Item as T | undefined;
  };

  query = async (limit: number): Promise<T[]> => {
    // Scan's Limit counts items read before the filter, so page until enough live records are found.
    const items: T[] = [];
    let startKey: Record<string, unknown> | undefined;
    do {
      const { Items, LastEvaluatedKey } = await this.client.send(new ScanCommand({
        TableName: this.tableName,
        FilterExpression: 'isDeleted = :val',
        ExpressionAttributeValues: { ':val': false },
        Limit: limit,
        ExclusiveStartKey: startKey,
      }));
      items.push(...((Items || []) as T[]));
      startKey = LastEvaluatedKey;
    } while (startKey && items.length < limit);
    return items.slice(0, limit);
  };

  create = async (item: T): Promise<void> => {
    await this.client.send(new PutCommand({ TableName: this.tableName, Item: item }));
  };

  write = async (item: T): Promise<boolean> => {
    await this.create(item);
    return true;
  };

  softDelete = async (id: string, updatedTimestamp: string, updatedBy?: string): Promise<boolean> => {
    const set = 'SET isDeleted = :deleted, updatedTimestamp = :ts';
    try {
      await this.client.send(new UpdateCommand({
        TableName: this.tableName,
        Key: { id },
        ConditionExpression: 'isDeleted = :live',
        UpdateExpression: updatedBy === undefined ? `${set} REMOVE updatedBy` : `${set}, updatedBy = :by`,
        // DynamoDB rejects expression values the expression does not use.
        ExpressionAttributeValues: {
          ':live': false,
          ':deleted': true,
          ':ts': updatedTimestamp,
          ...(updatedBy !== undefined && { ':by': updatedBy }),
        },
      }));
      return true;
    } catch (error) {
      if (error?.name === 'ConditionalCheckFailedException') {
        return false;
      }
      throw error;
    }
  };
}

export const dynamoStoreFactory = (client: DynamoDBDocumentClient): StoreFactory =>
  <T extends BaseItemRecord>(name: string) => new DynamoStore<T>(client, name);
