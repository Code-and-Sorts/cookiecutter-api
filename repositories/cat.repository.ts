import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { env, Cat, CatUpdate, CatRecord } from '@models';
import { BaseRepository } from './base.repository';
import { StoreFactory } from './document.store';

@injectable()
export class CatRepository extends BaseRepository<CatRecord> {
  constructor(@inject(StoreFactory) openStore: StoreFactory) {
    super(openStore(env.FIRESTORE_COLLECTION_ANIMALS), 'Cat');
  }

  create = (item: Cat, userId?: string): Promise<CatRecord> => this.addRecord(item, userId);

  get = (id: string): Promise<CatRecord> => this.getRecord(id);

  list = (limit?: number): Promise<CatRecord[]> => this.getRecords(limit);

  update = (id: string, fields: CatUpdate, userId?: string): Promise<CatRecord> =>
    this.updateRecord(id, fields, userId);

  delete = (id: string, userId?: string): Promise<void> => this.deleteRecord(id, userId);
}
