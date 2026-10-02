import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { DogRepository } from '@repositories';
import {
    Dog,
    DogResponse,
    toDogResponse,
} from '@models';

@injectable()
export class DogService {
  private _repo: DogRepository;

  constructor(@inject(DogRepository) repo: DogRepository) {
    this._repo = repo;
  }

  create = async (item: Dog, userId?: string): Promise<DogResponse> =>
    toDogResponse(await this._repo.create(item, userId));

  get = async (id: string): Promise<DogResponse> => toDogResponse(await this._repo.get(id));

  list = async (limit?: number): Promise<DogResponse[]> => (await this._repo.list(limit)).map(toDogResponse);

  replace = async (id: string, item: Dog, userId?: string): Promise<DogResponse> =>
    toDogResponse(await this._repo.replace(id, item, userId));

  delete = (id: string, userId?: string): Promise<void> => this._repo.delete(id, userId);
}
