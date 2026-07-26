
import { CollectionReference } from '@google-cloud/firestore';
import { ProxyError, NotFoundError } from '@errors';
import { BaseItemRecord } from '@models';

// Default cap on list reads to avoid unbounded queries.
const DEFAULT_LIST_LIMIT = 100;

export class BaseRepository<T extends BaseItemRecord> {
  readonly _collection: CollectionReference;

  constructor(collection: CollectionReference) {
    this._collection = collection;
  }

  addRecord = async (item: T): Promise<T> => {
    try {
      const docRef = this._collection.doc(item.id);
      await docRef.set(item);
      return item;
    } catch (error) {
      throw new ProxyError('Error creating item in database.');
    }
  };

  getRecord = async (id: string): Promise<T> => {
    try {
      const doc = await this._collection.doc(id).get();

      if (!doc.exists) {
        throw new NotFoundError(`Record not found for ID ${id}.`);
      }

      const data = doc.data() as T;
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
      const snapshot = await this._collection
        .where('isDeleted', '==', false)
        .limit(limit)
        .get();

      return snapshot.docs.map((doc) => doc.data() as T);
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
      const docRef = this._collection.doc(updates.id);
      await docRef.update(updatedItem);
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
      await this._collection.doc(item.id).set(item);
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
      const doc = await this._collection.doc(id).get();

      if (!doc.exists || (doc.data() as T).isDeleted) {
        throw new NotFoundError(`Record with id ${id} not found.`);
      }

      await this._collection.doc(id).update({
        isDeleted: true,
        updatedTimestamp: new Date().toISOString(),
      });
    } catch (error) {
      if (error instanceof NotFoundError) {
        throw error;
      }
      throw new ProxyError(`Error deleting record with id ${id}.`);
    }
  };
}
