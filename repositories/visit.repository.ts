import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import {
    env,
    VisitRecord,
    VisitCreateRequest,
    VisitUpdateRequest,
} from '@models';
import { BaseRepository } from './base.repository';
import { StoreFactory } from './document.store';

@injectable()
export class VisitRepository extends BaseRepository<VisitRecord> {
  constructor(@inject(StoreFactory) openStore: StoreFactory) {
    super(openStore(env.DYNAMODB_TABLE_NAME_VISITS), 'Visit');
  }

  create = (item: VisitCreateRequest, userId?: string): Promise<VisitRecord> =>
    this.addRecord({ paid: false, ...item }, userId);

  get = (id: string): Promise<VisitRecord> => this.getRecord(id);

  update = (id: string, fields: VisitUpdateRequest, userId?: string): Promise<VisitRecord> =>
    this.updateRecord(id, fields, userId);
}
