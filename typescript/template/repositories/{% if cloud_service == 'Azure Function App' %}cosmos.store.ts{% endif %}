import { Container, Database, PatchOperation } from '@azure/cosmos';
import { BaseEntity } from '@models';
import { DocumentStore, StoreFactory } from './document.store';

// Only an item-level 404 (no substatus) is a missing record; e.g. substatus 1003 (no such container) must stay a 500.
export const isMissingItem = (error: { code?: unknown; substatus?: unknown } | undefined): boolean =>
  error?.code === 404 && !error?.substatus;

export class CosmosStore<T extends BaseEntity> implements DocumentStore<T> {
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

  // A record read back keeps its ETag, so a write that races another fails (412) instead of losing it.
  write = async (item: T): Promise<boolean> => {
    const { _etag: etag } = item as T & { _etag?: string };
    try {
      await this.container.item(item.id, item.id).replace<T>(item, etag ? { accessCondition: { type: 'IfMatch', condition: etag } } : undefined);
      return true;
    } catch (error) {
      if (isMissingItem(error)) {
        return false;
      }
      throw error;
    }
  };

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
    return this.ifLive(this.container.item(id, id).patch({ condition: 'FROM c WHERE c.isDeleted = false', operations }));
  };

  private ifLive = async (operation: Promise<unknown>): Promise<boolean> => {
    try {
      await operation;
      return true;
    } catch (error) {
      // 412: the isDeleted = false precondition failed, so the record is already deleted.
      if (isMissingItem(error) || error?.code === 412) {
        return false;
      }
      throw error;
    }
  };
}

export const cosmosStoreFactory = (database: Database): StoreFactory =>
  <T extends BaseEntity>(name: string) => new CosmosStore<T>(database.container(name));
