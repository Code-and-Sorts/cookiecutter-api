import { inject, injectable } from 'inversify';
import {
    CatRepository,
    DogRepository,
} from '@repositories';
import {
    Cat,
    CatResponse,
    CatResponseSchema,
    CatEntitySchema,
    Dog,
    DogResponse,
    DogResponseSchema,
    DogEntitySchema,
} from '@models';

@injectable()
export class CatService {
  private _repo: CatRepository;

  constructor(@inject(CatRepository) repo: CatRepository) {
    this._repo = repo;
  }

  create = async (item: Cat): Promise<CatResponse> => {
    const newItem = CatEntitySchema.parse(item);
    const createdItem: CatResponse = await this._repo.create(newItem);
    return CatResponseSchema.parse(createdItem);
  };

  get = async (id: string): Promise<CatResponse> => {
    const item = await this._repo.get(id);
    return CatResponseSchema.parse(item);
  };

  list = async (limit?: number): Promise<CatResponse[]> => {
    const items = await this._repo.list(limit);
    return items.map((item) => CatResponseSchema.parse(item));
  };

  update = async (item: Partial<CatResponse>): Promise<CatResponse> => {
    const updatedItem = await this._repo.update(item);
    return CatResponseSchema.parse(updatedItem);
  };

  delete = async (id: string): Promise<void> => this._repo.delete(id);
}

@injectable()
export class DogService {
  private _repo: DogRepository;

  constructor(@inject(DogRepository) repo: DogRepository) {
    this._repo = repo;
  }

  create = async (item: Dog): Promise<DogResponse> => {
    const newItem = DogEntitySchema.parse(item);
    const createdItem: DogResponse = await this._repo.create(newItem);
    return DogResponseSchema.parse(createdItem);
  };

  get = async (id: string): Promise<DogResponse> => {
    const item = await this._repo.get(id);
    return DogResponseSchema.parse(item);
  };

  list = async (limit?: number): Promise<DogResponse[]> => {
    const items = await this._repo.list(limit);
    return items.map((item) => DogResponseSchema.parse(item));
  };

  replace = async (item: Partial<DogResponse>): Promise<DogResponse> => {
    const entity = DogEntitySchema.parse(item);
    const replacedItem = await this._repo.replace(entity);
    return DogResponseSchema.parse(replacedItem);
  };

  delete = async (id: string): Promise<void> => this._repo.delete(id);
}
