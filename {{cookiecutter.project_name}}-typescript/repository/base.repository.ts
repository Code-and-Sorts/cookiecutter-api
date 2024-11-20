import { Container, PatchOperation } from '@azure/cosmos';
import { ProxyError, NotFoundError } from '@errors';
import { BaseItemRecord } from '@models';

export class BaseRepository<T extends BaseItemRecord> {
  readonly _container: Container;

  constructor(container: Container) {
    this._container = container;
  }

  addRecord = async (item: T): Promise<T> => {
    try {
      const { resource: createdRecord } = await this._container.items.create<T>(item);
      return createdRecord as T;
    } catch (error) {
      throw new ProxyError('Error creating item in database.');
    }
  };

  getRecord = async (id: string): Promise<T> => {
    try {
      const query = `SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false`;
      const { resources: items } = await this._container.items
        .query<T>({ query, parameters: [{ name: '@id', value: id }] })
        .fetchAll();

      if (items.length > 0) {
        return items[0] as T;
      }
    } catch (error) {
      throw new ProxyError('Error creating item in database.');
    }
    throw new NotFoundError(`Record not found for ID ${id}.`);
  };

  getRecords = async (): Promise<T[]> => {
    try {
      const query = `SELECT * FROM c WHERE c.isDeleted = false`;
      const { resources: items } = await this._container.items
        .query<T>({ query })
        .fetchAll();

      return items as T[];
    } catch (error) {
      throw new ProxyError('Error creating item in database.');
    }
  };

  updateRecord = async (updates: T): Promise<T> => {
    try {
      const currentItem = await this.getRecord(updates.id);
      const updatedItem = {
        ...currentItem,
        ...updates,
      };
      const updatedRecord = await this._container.item(updates.id).replace<T>(updatedItem);
      return updatedRecord.resource as T;
    } catch (error) {
      if (error?.code === 404 || error?.statusCode === 404) {
        throw new NotFoundError(`Record with id ${updates.id} not found.`);
      }
      throw new ProxyError(`Error upserting item with id ${updates.id}.`);
    }
  };

  deleteRecord = async (id: string): Promise<void> => {
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
        throw new NotFoundError(`Record with id ${id} not found.`);
      }
      throw new ProxyError(`Error deleting record with id ${id}.`);
    }
  };
}
