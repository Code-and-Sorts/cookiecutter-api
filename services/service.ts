import { inject, injectable } from 'inversify';
import {
    KittenClawsRepository,
} from '@repositories';
import {
    KittenClaws,
    KittenClawsResponse,
    KittenClawsResponseSchema,
    KittenClawsEntitySchema,
} from '@models';

@injectable()
export class KittenClawsService {
  private _repo: KittenClawsRepository;

  constructor(@inject(KittenClawsRepository) repo: KittenClawsRepository) {
    this._repo = repo;
  }

  create = async (item: KittenClaws): Promise<KittenClawsResponse> => {
    const newItem = KittenClawsEntitySchema.parse(item);
    const createdItem: KittenClawsResponse = await this._repo.create(newItem);
    return KittenClawsResponseSchema.parse(createdItem);
  };

  get = async (id: string): Promise<KittenClawsResponse> => {
    const item = await this._repo.get(id);
    return KittenClawsResponseSchema.parse(item);
  };

  list = async (limit?: number): Promise<KittenClawsResponse[]> => {
    const items = await this._repo.list(limit);
    return items.map((item) => KittenClawsResponseSchema.parse(item));
  };

  update = async (item: Partial<KittenClawsResponse>): Promise<KittenClawsResponse> => {
    const updatedItem = await this._repo.update(item);
    return KittenClawsResponseSchema.parse(updatedItem);
  };

  delete = async (id: string): Promise<void> => this._repo.delete(id);
}
