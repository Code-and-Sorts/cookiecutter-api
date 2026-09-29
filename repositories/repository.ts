import 'reflect-metadata';
import { inject, injectable } from 'inversify';

import { Firestore } from '@google-cloud/firestore';
import {
    env,
    KittenClawsRecord,
} from '@models';
import { BaseRepository } from './base.repository';

@injectable()
export class KittenClawsRepository extends BaseRepository<KittenClawsRecord> {
  constructor(@inject(Firestore) client: Firestore) {
    super(client.collection(env.FIRESTORE_COLLECTION_KITTIES));
  }

  create = async (newItem: KittenClawsRecord): Promise<KittenClawsRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<KittenClawsRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<KittenClawsRecord[]> => this.getRecords(limit);
  update = async (item: Partial<KittenClawsRecord> & { id: string }): Promise<KittenClawsRecord> => this.updateRecord(item);
  replace = async (item: KittenClawsRecord): Promise<KittenClawsRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}

