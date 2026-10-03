import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import {
    env,
    KittenClawsRecord,
    KittenClawsCreateRequest,
    KittenClawsUpdateRequest,
} from '@models';
import { BaseRepository } from './base.repository';
import { StoreFactory } from './document.store';

@injectable()
export class KittenClawsRepository extends BaseRepository<KittenClawsRecord> {
  constructor(@inject(StoreFactory) openStore: StoreFactory) {
    super(openStore(env.COSMOS_CONTAINER_KITTENCLAWS), 'KittenClaws');
  }

  create = (item: KittenClawsCreateRequest, userId?: string): Promise<KittenClawsRecord> => this.addRecord(item, userId);

  get = (id: string): Promise<KittenClawsRecord> => this.getRecord(id);

  list = (limit?: number): Promise<KittenClawsRecord[]> => this.getRecords(limit);

  update = (id: string, fields: KittenClawsUpdateRequest, userId?: string): Promise<KittenClawsRecord> =>
    this.updateRecord(id, fields, userId);

  delete = (id: string, userId?: string): Promise<void> => this.deleteRecord(id, userId);
}
