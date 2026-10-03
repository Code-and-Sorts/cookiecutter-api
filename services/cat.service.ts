import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { CatRepository } from '@repositories';
import {
    CatCreateRequest,
    CatReplaceRequest,
    CatResponse,
    CatUpdateRequest,
    toCatResponse,
} from '@models';

@injectable()
export class CatService {
  private _repo: CatRepository;

  constructor(@inject(CatRepository) repo: CatRepository) {
    this._repo = repo;
  }

  create = async (item: CatCreateRequest, userId?: string): Promise<CatResponse> =>
    toCatResponse(await this._repo.create(item, userId));

  get = async (id: string): Promise<CatResponse> => toCatResponse(await this._repo.get(id));

  list = async (limit?: number): Promise<CatResponse[]> => (await this._repo.list(limit)).map(toCatResponse);

  update = async (id: string, fields: CatUpdateRequest, userId?: string): Promise<CatResponse> =>
    toCatResponse(await this._repo.update(id, fields, userId));

  replace = async (id: string, item: CatReplaceRequest, userId?: string): Promise<CatResponse> =>
    toCatResponse(await this._repo.replace(id, item, userId));

  delete = (id: string, userId?: string): Promise<void> => this._repo.delete(id, userId);
}
