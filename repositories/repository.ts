import 'reflect-metadata';
import { inject, injectable } from 'inversify';

import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import {
    env,
    CatRecord,
    DogRecord,
} from '@models';
import { BaseRepository, DocumentClient } from './base.repository';

@injectable()
export class CatRepository extends BaseRepository<CatRecord> {
  constructor(@inject(DocumentClient) client: DynamoDBDocumentClient) {
    super(client, env.DYNAMODB_TABLE_NAME_ANIMALS);
  }

  create = async (newItem: CatRecord): Promise<CatRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<CatRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<CatRecord[]> => this.getRecords(limit);
  update = async (item: Partial<CatRecord> & { id: string }): Promise<CatRecord> => this.updateRecord(item);
  replace = async (item: CatRecord): Promise<CatRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}

@injectable()
export class DogRepository extends BaseRepository<DogRecord> {
  constructor(@inject(DocumentClient) client: DynamoDBDocumentClient) {
    super(client, env.DYNAMODB_TABLE_NAME_ANIMALS);
  }

  create = async (newItem: DogRecord): Promise<DogRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<DogRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<DogRecord[]> => this.getRecords(limit);
  update = async (item: Partial<DogRecord> & { id: string }): Promise<DogRecord> => this.updateRecord(item);
  replace = async (item: DogRecord): Promise<DogRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}

