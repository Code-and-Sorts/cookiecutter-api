import type { ServiceIdentifier } from 'inversify';
import { BaseItemRecord } from '@models';

export interface DocumentStore<T extends BaseItemRecord> {
  read(id: string): Promise<T | undefined>;
  query(limit: number): Promise<T[]>;
  create(item: T): Promise<void>;
  /** Resolves false when the item no longer exists. */
  write(item: T): Promise<boolean>;
  /** Resolves false when the item is missing or already deleted. */
  softDelete(id: string, updatedTimestamp: string): Promise<boolean>;
}

export type StoreFactory = <T extends BaseItemRecord>(name: string) => DocumentStore<T>;

export const StoreFactory: ServiceIdentifier<StoreFactory> = Symbol.for('StoreFactory');
