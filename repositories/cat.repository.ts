import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import {
    env,
    CatRecord,
    CatCreateRequest,
    CatReplaceRequest,
    CatReplaceRequestSchema,
    CatUpdateRequest,
} from '@models';
import { BaseRepository } from './base.repository';
import { StoreFactory } from './document.store';

const REPLACE_FIELDS = Object.keys(CatReplaceRequestSchema.shape);

@injectable()
export class CatRepository extends BaseRepository<CatRecord> {
  constructor(@inject(StoreFactory) openStore: StoreFactory) {
    super(openStore(env.FIRESTORE_COLLECTION_CATS), 'Cat');
  }

  create = (item: CatCreateRequest, userId?: string): Promise<CatRecord> => this.addRecord(item, userId);

  get = (id: string): Promise<CatRecord> => this.getRecord(id);

  list = (limit?: number): Promise<CatRecord[]> => this.getRecords(limit);

  update = (id: string, fields: CatUpdateRequest, userId?: string): Promise<CatRecord> =>
    this.updateRecord(id, fields, userId);

  replace = (id: string, item: CatReplaceRequest, userId?: string): Promise<CatRecord> =>
    this.replaceRecord(id, item, REPLACE_FIELDS, userId);

  delete = (id: string, userId?: string): Promise<void> => this.deleteRecord(id, userId);
}
