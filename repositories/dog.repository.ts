import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { env, Dog, DogRecord } from '@models';
import { BaseRepository } from './base.repository';
import { StoreFactory } from './document.store';

@injectable()
export class DogRepository extends BaseRepository<DogRecord> {
  constructor(@inject(StoreFactory) openStore: StoreFactory) {
    super(openStore(env.FIRESTORE_COLLECTION_ANIMALS), 'Dog');
  }

  create = (item: Dog, userId?: string): Promise<DogRecord> => this.addRecord(item, userId);

  get = (id: string): Promise<DogRecord> => this.getRecord(id);

  list = (limit?: number): Promise<DogRecord[]> => this.getRecords(limit);

  replace = (id: string, item: Dog, userId?: string): Promise<DogRecord> =>
    this.replaceRecord(id, item, userId);

  delete = (id: string, userId?: string): Promise<void> => this.deleteRecord(id, userId);
}
