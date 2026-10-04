import { Container, Database, PatchOperation, Resource } from '@azure/cosmos';
import { BaseItemRecord } from '@models';
import { DocumentStore, StoreFactory } from './document.store';

type CosmosError = { code?: unknown; substatus?: unknown } | undefined;

// Only an item-level 404 (no substatus) is a missing record; e.g. substatus 1003 (no such container) must stay a 500.
export const isMissingItem = (error: CosmosError): boolean => error?.code === 404 && !error?.substatus;

// 412: the isDeleted = false precondition failed, so the record is already deleted.
const isGone = (error: CosmosError): boolean => isMissingItem(error) || error?.code === 412;

export class CosmosStore<T extends BaseItemRecord> implements DocumentStore<T> {
  constructor(private readonly container: Container) {}

  read = async (id: string): Promise<T | undefined> => {
    const query = 'SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false';
    const { resources } = await this.container.items
      .query<T>({ query, parameters: [{ name: '@id', value: id }] })
      .fetchAll();
    return resources[0];
  };

  query = async (limit: number): Promise<T[]> => {
    const query = `SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT ${limit}`;
    const { resources } = await this.container.items.query<T>({ query }).fetchAll();
    return resources;
  };

  create = async (item: T): Promise<void> => {
    await this.container.items.create<T>(item);
  };

  write = (item: T, read: T): Promise<boolean> =>
    this.ifFound(
      this.container.item(item.id, item.id).replace<T>(item, { accessCondition: { type: 'IfMatch', condition: (read as T & Resource)._etag } }),
      isMissingItem,
    );

  softDelete = (id: string, updatedTimestamp: string, updatedBy?: string): Promise<boolean> => {
    const operations: PatchOperation[] = [
      { op: 'set', path: '/isDeleted', value: true },
      { op: 'set', path: '/updatedTimestamp', value: updatedTimestamp },
      { op: 'set', path: '/updatedBy', value: updatedBy ?? '' },
    ];
    if (updatedBy === undefined) {
      // Patch remove fails on a missing path, so the set above guarantees one exists.
      operations.push({ op: 'remove', path: '/updatedBy' });
    }
    return this.ifFound(this.container.item(id, id).patch({ condition: 'FROM c WHERE c.isDeleted = false', operations }), isGone);
  };

  private ifFound = async (operation: Promise<unknown>, notFound: (error: CosmosError) => boolean): Promise<boolean> => {
    try {
      await operation;
      return true;
    } catch (error) {
      if (notFound(error as CosmosError)) {
        return false;
      }
      throw error;
    }
  };
}

export const cosmosStoreFactory = (database: Database): StoreFactory =>
  <T extends BaseItemRecord>(name: string) => new CosmosStore<T>(database.container(name));
