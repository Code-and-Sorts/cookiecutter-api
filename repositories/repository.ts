
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import {
    CatRecord,
    DogRecord,
} from '@models';
import { BaseRepository } from './base.repository';

export class CatRepository extends BaseRepository<CatRecord> {
  constructor(docClient: DynamoDBDocumentClient, tableName: string) {
    super(docClient, tableName);
  }

  create = async (newItem: CatRecord): Promise<CatRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<CatRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<CatRecord[]> => this.getRecords(limit);
  update = async (item: Partial<CatRecord>): Promise<CatRecord> => this.updateRecord(item);
  replace = async (item: CatRecord): Promise<CatRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}

export class DogRepository extends BaseRepository<DogRecord> {
  constructor(docClient: DynamoDBDocumentClient, tableName: string) {
    super(docClient, tableName);
  }

  create = async (newItem: DogRecord): Promise<DogRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<DogRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<DogRecord[]> => this.getRecords(limit);
  update = async (item: Partial<DogRecord>): Promise<DogRecord> => this.updateRecord(item);
  replace = async (item: DogRecord): Promise<DogRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
