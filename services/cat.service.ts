import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { CatRepository } from '@repositories';
import {
    Cat,
    CatResponse,
    CatUpdate,
    toCatResponse,
} from '@models';

@injectable()
export class CatService {
  private _repo: CatRepository;

  constructor(@inject(CatRepository) repo: CatRepository) {
    this._repo = repo;
  }

  create = async (item: Cat, userId?: string): Promise<CatResponse> =>
    toCatResponse(await this._repo.create(item, userId));

  get = async (id: string): Promise<CatResponse> => toCatResponse(await this._repo.get(id));

  list = async (limit?: number): Promise<CatResponse[]> => (await this._repo.list(limit)).map(toCatResponse);

  update = async (id: string, fields: CatUpdate, userId?: string): Promise<CatResponse> =>
    toCatResponse(await this._repo.update(id, fields, userId));

  delete = (id: string, userId?: string): Promise<void> => this._repo.delete(id, userId);
}
