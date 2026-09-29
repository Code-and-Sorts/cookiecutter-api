{% if cloud_service == 'Azure Function App' -%}
import { Container, PatchOperation } from '@azure/cosmos';
import { ProxyError, NotFoundError } from '@errors';
import { BaseItemRecord } from '@models';
import { DEFAULT_LIST_LIMIT } from '@utils';

// Shared data access for every resource. Timestamps are ISO-8601 UTC with milliseconds.
// createdBy/updatedBy are only stored when set. The CRUD methods are protected: each resource
// repository exposes public methods only for the operations its resource declares.
export abstract class BaseRepository<T extends BaseItemRecord> {
  protected readonly _container: Container;
  protected readonly _resourceName: string;

  constructor(container: Container, resourceName: string) {
    this._container = container;
    this._resourceName = resourceName;
  }

  // Missing and soft-deleted records are both reported as not found.
  protected notFound = (id: string): NotFoundError =>
    new NotFoundError(`${this._resourceName} with id ${id} was not found.`);

  protected addRecord = async (item: T): Promise<T> => {
    try {
      const { resource: createdRecord } = await this._container.items.create<T>(item);
      return createdRecord as T;
    } catch (error) {
      throw new ProxyError('Error creating item in database.', error);
    }
  };

  protected getRecord = async (id: string): Promise<T> => {
    try {
      const query = `SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false`;
      const { resources: items } = await this._container.items
        .query<T>({ query, parameters: [{ name: '@id', value: id }] })
        .fetchAll();

      if (items.length > 0) {
        return items[0] as T;
      }
    } catch (error) {
      throw new ProxyError('Error retrieving item from database.', error);
    }
    throw this.notFound(id);
  };

  protected getRecords = async (limit: number = DEFAULT_LIST_LIMIT): Promise<T[]> => {
    try {
      const query = `SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT ${limit}`;
      const { resources: items } = await this._container.items
        .query<T>({ query })
        .fetchAll();

      return items as T[];
    } catch (error) {
      throw new ProxyError('Error retrieving items from database.', error);
    }
  };

  protected updateRecord = async (updates: Partial<T> & { id: string }): Promise<T> => {
    try {
      const currentItem = await this.getRecord(updates.id);
      const updatedItem = {
        ...currentItem,
        ...updates,
      };
      const updatedRecord = await this._container.item(updates.id, updates.id).replace<T>(updatedItem);
      return updatedRecord.resource as T;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      if (error?.code === 404) {
        throw this.notFound(updates.id);
      }
      throw new ProxyError(`Error upserting item with id ${updates.id}.`, error);
    }
  };

  protected replaceRecord = async (item: T): Promise<T> => {
    try {
      const existing = await this.getRecord(item.id);
      // Replace keeps the stored created fields; createdBy is omitted (never undefined) when absent.
      const { createdBy: _createdBy, ...rest } = item;
      const written = {
        ...rest,
        createdTimestamp: existing.createdTimestamp,
        ...(existing.createdBy !== undefined && { createdBy: existing.createdBy }),
      } as T;
      await this._container.item(item.id, item.id).replace<T>(written);
      return written;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error replacing item with id ${item.id}.`, error);
    }
  };

  protected deleteRecord = async (id: string): Promise<void> => {
    try {
      const operations: PatchOperation[] = [
        { op: 'set', path: '/isDeleted', value: true },
        {
          op: 'set',
          path: '/updatedTimestamp',
          value: new Date().toISOString()
        }
      ];
      const condition = 'FROM c WHERE c.isDeleted = false';

      await this._container.item(id, id).patch({ condition, operations });
    } catch (error) {
      if (error?.code === 404 || error?.code === 412) {
        throw this.notFound(id);
      }
      throw new ProxyError(`Error deleting record with id ${id}.`, error);
    }
  };
}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { CollectionReference } from '@google-cloud/firestore';
import { ProxyError, NotFoundError } from '@errors';
import { BaseItemRecord } from '@models';
import { DEFAULT_LIST_LIMIT } from '@utils';

// Shared data access for every resource. Timestamps are ISO-8601 UTC with milliseconds.
// createdBy/updatedBy are only stored when set. The CRUD methods are protected: each resource
// repository exposes public methods only for the operations its resource declares.
export abstract class BaseRepository<T extends BaseItemRecord> {
  protected readonly _collection: CollectionReference;
  protected readonly _resourceName: string;

  constructor(collection: CollectionReference, resourceName: string) {
    this._collection = collection;
    this._resourceName = resourceName;
  }

  // Missing and soft-deleted records are both reported as not found.
  protected notFound = (id: string): NotFoundError =>
    new NotFoundError(`${this._resourceName} with id ${id} was not found.`);

  protected addRecord = async (item: T): Promise<T> => {
    try {
      const docRef = this._collection.doc(item.id);
      await docRef.set(item);
      return item;
    } catch (error) {
      throw new ProxyError('Error creating item in database.', error);
    }
  };

  protected getRecord = async (id: string): Promise<T> => {
    try {
      const doc = await this._collection.doc(id).get();

      if (!doc.exists) {
        throw this.notFound(id);
      }

      const data = doc.data() as T;
      if (data.isDeleted) {
        throw this.notFound(id);
      }

      return data;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError('Error retrieving item from database.', error);
    }
  };

  protected getRecords = async (limit: number = DEFAULT_LIST_LIMIT): Promise<T[]> => {
    try {
      const snapshot = await this._collection
        .where('isDeleted', '==', false)
        .limit(limit)
        .get();

      return snapshot.docs.map((doc) => doc.data() as T);
    } catch (error) {
      throw new ProxyError('Error retrieving items from database.', error);
    }
  };

  protected updateRecord = async (updates: Partial<T> & { id: string }): Promise<T> => {
    try {
      const currentItem = await this.getRecord(updates.id);
      const updatedItem = {
        ...currentItem,
        ...updates,
      };
      const docRef = this._collection.doc(updates.id);
      await docRef.update(updatedItem);
      return updatedItem;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error upserting item with id ${updates.id}.`, error);
    }
  };

  protected replaceRecord = async (item: T): Promise<T> => {
    try {
      const existing = await this.getRecord(item.id);
      // Replace keeps the stored created fields; createdBy is omitted (never undefined) when absent.
      const { createdBy: _createdBy, ...rest } = item;
      const written = {
        ...rest,
        createdTimestamp: existing.createdTimestamp,
        ...(existing.createdBy !== undefined && { createdBy: existing.createdBy }),
      } as T;
      await this._collection.doc(item.id).set(written);
      return written;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error replacing item with id ${item.id}.`, error);
    }
  };

  protected deleteRecord = async (id: string): Promise<void> => {
    try {
      const doc = await this._collection.doc(id).get();

      if (!doc.exists || (doc.data() as T).isDeleted) {
        throw this.notFound(id);
      }

      await this._collection.doc(id).update({
        isDeleted: true,
        updatedTimestamp: new Date().toISOString(),
      });
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error deleting record with id ${id}.`, error);
    }
  };
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBDocumentClient, PutCommand, GetCommand, ScanCommand, UpdateCommand } from '@aws-sdk/lib-dynamodb';
import type { ServiceIdentifier } from 'inversify';
import { ProxyError, NotFoundError } from '@errors';
import { BaseItemRecord } from '@models';
import { DEFAULT_LIST_LIMIT } from '@utils';

export const DocumentClient: ServiceIdentifier<DynamoDBDocumentClient> = Symbol.for('DynamoDBDocumentClient');

// Shared data access for every resource. Timestamps are ISO-8601 UTC with milliseconds.
// createdBy/updatedBy are only stored when set. The CRUD methods are protected: each resource
// repository exposes public methods only for the operations its resource declares.
export abstract class BaseRepository<T extends BaseItemRecord> {
  protected readonly _docClient: DynamoDBDocumentClient;
  protected readonly _tableName: string;
  protected readonly _resourceName: string;

  constructor(docClient: DynamoDBDocumentClient, tableName: string, resourceName: string) {
    this._docClient = docClient;
    this._tableName = tableName;
    this._resourceName = resourceName;
  }

  // Missing and soft-deleted records are both reported as not found.
  protected notFound = (id: string): NotFoundError =>
    new NotFoundError(`${this._resourceName} with id ${id} was not found.`);

  protected addRecord = async (item: T): Promise<T> => {
    try {
      await this._docClient.send(new PutCommand({
        TableName: this._tableName,
        Item: item as Record<string, unknown>,
      }));
      return item;
    } catch (error) {
      throw new ProxyError('Error creating item in database.', error);
    }
  };

  protected getRecord = async (id: string): Promise<T> => {
    try {
      const { Item } = await this._docClient.send(new GetCommand({
        TableName: this._tableName,
        Key: { id },
      }));

      if (!Item) {
        throw this.notFound(id);
      }

      const data = Item as T;
      if (data.isDeleted) {
        throw this.notFound(id);
      }

      return data;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError('Error retrieving item from database.', error);
    }
  };

  protected getRecords = async (limit: number = DEFAULT_LIST_LIMIT): Promise<T[]> => {
    try {
      // A scan's Limit counts items read before the filter, so keep paging until enough
      // live records are found or the table is exhausted.
      const items: T[] = [];
      let startKey: Record<string, unknown> | undefined;
      do {
        const { Items, LastEvaluatedKey } = await this._docClient.send(new ScanCommand({
          TableName: this._tableName,
          FilterExpression: 'isDeleted = :val',
          ExpressionAttributeValues: { ':val': false },
          Limit: limit,
          ExclusiveStartKey: startKey,
        }));
        items.push(...((Items || []) as T[]));
        startKey = LastEvaluatedKey;
      } while (startKey && items.length < limit);

      return items.slice(0, limit);
    } catch (error) {
      throw new ProxyError('Error retrieving items from database.', error);
    }
  };

  protected updateRecord = async (updates: Partial<T> & { id: string }): Promise<T> => {
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
      throw new ProxyError(`Error upserting item with id ${updates.id}.`, error);
    }
  };

  protected replaceRecord = async (item: T): Promise<T> => {
    try {
      const existing = await this.getRecord(item.id);
      // Replace keeps the stored created fields; createdBy is omitted (never undefined) when absent.
      const { createdBy: _createdBy, ...rest } = item;
      const written = {
        ...rest,
        createdTimestamp: existing.createdTimestamp,
        ...(existing.createdBy !== undefined && { createdBy: existing.createdBy }),
      } as T;
      await this._docClient.send(new PutCommand({
        TableName: this._tableName,
        Item: written as Record<string, unknown>,
      }));
      return written;
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error replacing item with id ${item.id}.`, error);
    }
  };

  protected deleteRecord = async (id: string): Promise<void> => {
    try {
      const { Item } = await this._docClient.send(new GetCommand({
        TableName: this._tableName,
        Key: { id },
      }));

      if (!Item || (Item as T).isDeleted) {
        throw this.notFound(id);
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
      throw new ProxyError(`Error deleting record with id ${id}.`, error);
    }
  };
}
{%- endif %}
