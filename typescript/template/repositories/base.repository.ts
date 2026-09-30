import { NotFoundError, ProxyError } from '@errors';
import { BaseItemRecord } from '@models';
import { DEFAULT_LIST_LIMIT, newId, nowIso, withDeadline } from '@utils';
import { DocumentStore } from './document.store';

export type RecordFields<T extends BaseItemRecord> = Omit<T, keyof BaseItemRecord>;

// Protected so each resource repository exposes only the operations its resource declares.
export abstract class BaseRepository<T extends BaseItemRecord> {
  constructor(
    protected readonly store: DocumentStore<T>,
    protected readonly resourceName: string,
  ) {}

  protected guard = <R>(message: string, op: () => Promise<R>): Promise<R> =>
    withDeadline(
      op().catch((error: unknown) => {
        throw error instanceof NotFoundError ? error : new ProxyError(message, error);
      }),
    );

  protected notFound = (id: string): NotFoundError => NotFoundError.forItem(this.resourceName, id);

  protected addRecord = (fields: RecordFields<T>): Promise<T> =>
    this.guard('Error creating item in database.', async () => {
      const now = nowIso();
      const record = { ...fields, id: newId(), isDeleted: false, createdTimestamp: now, updatedTimestamp: now } as T;
      await this.store.create(record);
      return record;
    });

  protected getRecord = (id: string): Promise<T> =>
    this.guard('Error retrieving item from database.', () => this.findLive(id));

  protected getRecords = (limit: number = DEFAULT_LIST_LIMIT): Promise<T[]> =>
    this.guard('Error retrieving items from database.', () => this.store.query(limit));

  protected updateRecord = (id: string, fields: Partial<RecordFields<T>>): Promise<T> =>
    this.guard(`Error upserting item with id ${id}.`, async () =>
      this.save({ ...(await this.findLive(id)), ...fields, id, updatedTimestamp: nowIso() }),
    );

  protected replaceRecord = (id: string, fields: RecordFields<T>): Promise<T> =>
    this.guard(`Error replacing item with id ${id}.`, async () => {
      const { createdTimestamp, createdBy } = await this.findLive(id);
      return this.save({
        ...fields,
        id,
        isDeleted: false,
        createdTimestamp,
        updatedTimestamp: nowIso(),
        // Omitted, never stored as undefined, when the record has none.
        ...(createdBy !== undefined && { createdBy }),
      } as T);
    });

  protected deleteRecord = (id: string): Promise<void> =>
    this.guard(`Error deleting record with id ${id}.`, async () => {
      if (!(await this.store.softDelete(id, nowIso()))) {
        throw this.notFound(id);
      }
    });

  private findLive = async (id: string): Promise<T> => {
    const record = await this.store.read(id);
    if (!record || record.isDeleted) {
      throw this.notFound(id);
    }
    return record;
  };

  private save = async (record: T): Promise<T> => {
    if (!(await this.store.write(record))) {
      throw this.notFound(record.id);
    }
    return record;
  };
}
