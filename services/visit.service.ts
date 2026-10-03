import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { VisitRepository } from '@repositories';
import {
    VisitCreateRequest,
    VisitResponse,
    VisitUpdateRequest,
    toVisitResponse,
} from '@models';

@injectable()
export class VisitService {
  private _repo: VisitRepository;

  constructor(@inject(VisitRepository) repo: VisitRepository) {
    this._repo = repo;
  }

  create = async (item: VisitCreateRequest, userId?: string): Promise<VisitResponse> =>
    toVisitResponse(await this._repo.create(item, userId));

  get = async (id: string): Promise<VisitResponse> => toVisitResponse(await this._repo.get(id));

  update = async (id: string, fields: VisitUpdateRequest, userId?: string): Promise<VisitResponse> =>
    toVisitResponse(await this._repo.update(id, fields, userId));
}
