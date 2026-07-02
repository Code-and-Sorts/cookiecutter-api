import { inject, injectable } from 'inversify';
import { ItemRepository } from '@repositories';
import {
  Item,
  ItemResponse,
  ItemResponseSchema,
  ItemEntitySchema,
} from '@models';

@injectable()
export class ItemService {
  private _repo: ItemRepository;

  constructor(@inject(ItemRepository) repo: ItemRepository) {
    this._repo = repo;
  }

  create = async (item: Item): Promise<ItemResponse> => {
    const newItem = ItemEntitySchema.parse(item);
    const createdItem: ItemResponse = await this._repo.create(newItem);
    return ItemResponseSchema.parse(createdItem);
  };

  get = async (id: string): Promise<ItemResponse> => {
    const item = await this._repo.get(id);
    return ItemResponseSchema.parse(item);
  };

  list = async (limit?: number): Promise<ItemResponse[]> => {
    const items = await this._repo.list(limit);
    return items.map((item) => ItemResponseSchema.parse(item));
  };

  update = async (item: Partial<ItemResponse>): Promise<ItemResponse> => {
    const updatedItem = await this._repo.update(item);
    return ItemResponseSchema.parse(updatedItem);
  };

  replace = async (item: Partial<ItemResponse>): Promise<ItemResponse> => {
    const entity = ItemEntitySchema.parse(item);
    const replacedItem = await this._repo.replace(entity);
    return ItemResponseSchema.parse(replacedItem);
  };

  delete = async (id: string): Promise<void> => this._repo.delete(id);
}
