
import { CollectionReference } from '@google-cloud/firestore';
import {
    KittenClawsRecord,
} from '@models';
import { BaseRepository } from './base.repository';

export class KittenClawsRepository extends BaseRepository<KittenClawsRecord> {
  constructor(collection: CollectionReference) {
    super(collection);
  }

  create = async (newItem: KittenClawsRecord): Promise<KittenClawsRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<KittenClawsRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<KittenClawsRecord[]> => this.getRecords(limit);
  update = async (item: Partial<KittenClawsRecord>): Promise<KittenClawsRecord> => this.updateRecord(item);
  replace = async (item: KittenClawsRecord): Promise<KittenClawsRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
