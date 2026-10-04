import type { ServiceIdentifier } from 'inversify';
import { BaseItemRecord } from '@models';

export interface DocumentStore<T extends BaseItemRecord> {
  read(id: string): Promise<T | undefined>;
  query(limit: number): Promise<T[]>;
  create(item: T): Promise<void>;
  write(item: T, read: T): Promise<boolean>;
  /** Resolves false when the item is missing or already deleted; removes updatedBy when none is given. */
  softDelete(id: string, updatedTimestamp: string, updatedBy?: string): Promise<boolean>;
}

export type StoreFactory = <T extends BaseItemRecord>(name: string) => DocumentStore<T>;

export const StoreFactory: ServiceIdentifier<StoreFactory> = Symbol.for('StoreFactory');
