
import { DynamoDBDocumentClient, PutCommand, GetCommand, ScanCommand, UpdateCommand } from '@aws-sdk/lib-dynamodb';
import { ProxyError, NotFoundError } from '@errors';
import { BaseItemRecord } from '@models';

// Default cap on list reads to avoid unbounded scans.
const DEFAULT_LIST_LIMIT = 100;

export class BaseRepository<T extends BaseItemRecord> {
  readonly _docClient: DynamoDBDocumentClient;
  readonly _tableName: string;

  constructor(docClient: DynamoDBDocumentClient, tableName: string) {
    this._docClient = docClient;
    this._tableName = tableName;
  }

  addRecord = async (item: T): Promise<T> => {
    try {
      await this._docClient.send(new PutCommand({
        TableName: this._tableName,
        Item: item as Record<string, unknown>,
      }));
      return item;
    } catch (error) {
      throw new ProxyError('Error creating item in database.');
    }
  };

  getRecord = async (id: string): Promise<T> => {
    try {
      const { Item } = await this._docClient.send(new GetCommand({
        TableName: this._tableName,
        Key: { id },
      }));

      if (!Item) {
        throw new NotFoundError(`Record not found for ID ${id}.`);
      }

      const data = Item as T;
      if (data.isDeleted) {
        throw new NotFoundError(`Record not found for ID ${id}.`);
      }

      return data;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError('Error retrieving item from database.');
    }
  };

  getRecords = async (limit: number = DEFAULT_LIST_LIMIT): Promise<T[]> => {
    try {
      const { Items } = await this._docClient.send(new ScanCommand({
        TableName: this._tableName,
        FilterExpression: 'isDeleted = :val',
        ExpressionAttributeValues: { ':val': false },
        Limit: limit,
      }));

      return ((Items || []) as T[]).slice(0, limit);
    } catch (error) {
      throw new ProxyError('Error retrieving items from database.');
    }
  };

  updateRecord = async (updates: T): Promise<T> => {
    try {
      const currentItem = await this.getRecord(updates.id);
      const updatedItem = {
        ...currentItem,
        ...updates,
      };
      await this._docClient.send(new PutCommand({
        TableName: this._tableName,
        Item: updatedItem as Record<string, unknown>,
      }));
      return updatedItem;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error upserting item with id ${updates.id}.`);
    }
  };

  replaceRecord = async (item: T): Promise<T> => {
    try {
      await this.getRecord(item.id);
      await this._docClient.send(new PutCommand({
        TableName: this._tableName,
        Item: item as Record<string, unknown>,
      }));
      return item;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error replacing item with id ${item.id}.`);
    }
  };

  deleteRecord = async (id: string): Promise<void> => {
    try {
      const { Item } = await this._docClient.send(new GetCommand({
        TableName: this._tableName,
        Key: { id },
      }));

      if (!Item || (Item as T).isDeleted) {
        throw new NotFoundError(`Record with id ${id} not found.`);
      }

      await this._docClient.send(new UpdateCommand({
        TableName: this._tableName,
        Key: { id },
        UpdateExpression: 'SET isDeleted = :del, updatedTimestamp = :ts',
        ExpressionAttributeValues: {
          ':del': true,
          ':ts': new Date().toISOString(),
        },
      }));
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error deleting record with id ${id}.`);
    }
  };
}
