import { NotFoundError, ProxyError } from '@errors';
import { BaseEntity, SystemField } from '@models';
import { DEFAULT_LIST_LIMIT, newId, nowIso, withDeadline } from '@utils';
import { DocumentStore } from './document.store';

export type RecordFields<T extends BaseEntity> = Omit<T, SystemField>;

// A field without a value is not stored, in every language and database.
const withoutEmpty = <R extends object>(record: R): R =>
  Object.fromEntries(Object.entries(record).filter(([, value]) => value !== null && value !== undefined)) as R;

// updatedBy names the latest writer, so a write without a user id drops any earlier value.
const updatedBy = (userId?: string): Pick<BaseEntity, 'updatedBy'> => (userId === undefined ? {} : { updatedBy: userId });

// Protected so each resource repository exposes only the operations its resource declares.
export abstract class BaseRepository<T extends BaseEntity> {
  constructor(
    protected readonly store: DocumentStore<T>,
    protected readonly resourceName: string,
    // The fields a client may set, so a replace can keep the ones its body does not accept.
    protected readonly clientFields: readonly string[] = [],
  ) {}

  protected guard = <R>(message: string, op: () => Promise<R>): Promise<R> =>
    withDeadline(
      op().catch((error: unknown) => {
        throw error instanceof NotFoundError ? error : new ProxyError(message, error);
      }),
    );

  protected notFound = (id: string): NotFoundError => NotFoundError.forItem(this.resourceName, id);

  protected addRecord = (fields: Partial<RecordFields<T>>, userId?: string): Promise<T> =>
    this.guard('Error creating item in database.', async () => {
      const now = nowIso();
      const record = withoutEmpty({
        ...fields,
        id: newId(),
        isDeleted: false,
        createdTimestamp: now,
        updatedTimestamp: now,
        ...(userId !== undefined && { createdBy: userId }),
        ...updatedBy(userId),
      } as T);
      await this.store.create(record);
      return record;
    });

  protected getRecord = (id: string): Promise<T> =>
    this.guard('Error retrieving item from database.', () => this.findLive(id));

  protected getRecords = (limit: number = DEFAULT_LIST_LIMIT): Promise<T[]> =>
    this.guard('Error retrieving items from database.', () => this.store.query(limit));

  protected updateRecord = (id: string, fields: Partial<RecordFields<T>>, userId?: string): Promise<T> =>
    this.guard(`Error upserting item with id ${id}.`, async () => {
      const { updatedBy: _previous, ...current } = await this.findLive(id);
      return this.save(withoutEmpty({ ...current, ...fields, id, updatedTimestamp: nowIso(), ...updatedBy(userId) } as T));
    });

  // accepted names every field the replace body may set; the record's other client fields keep their values.
  protected replaceRecord = (id: string, fields: Partial<RecordFields<T>>, accepted: readonly string[], userId?: string): Promise<T> =>
    this.guard(`Error replacing item with id ${id}.`, async () => {
      const current = await this.findLive(id);
      const { createdTimestamp, createdBy } = current;
      const kept = Object.fromEntries(
        Object.entries(current).filter(([field]) => this.clientFields.includes(field) && !accepted.includes(field)),
      );
      return this.save(withoutEmpty({
        ...kept,
        ...fields,
        id,
        isDeleted: false,
        createdTimestamp,
        updatedTimestamp: nowIso(),
        // Omitted, never stored as undefined, when the record has none.
        ...(createdBy !== undefined && { createdBy }),
        ...updatedBy(userId),
      } as T));
    });

  protected deleteRecord = (id: string, userId?: string): Promise<void> =>
    this.guard(`Error deleting record with id ${id}.`, async () => {
      if (!(await this.store.softDelete(id, nowIso(), userId))) {
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
