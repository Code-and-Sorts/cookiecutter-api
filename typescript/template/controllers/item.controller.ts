import { inject, injectable } from 'inversify';
import 'reflect-metadata';
import { ItemService } from '@services';
import {
  Item,
  ItemRequestSchema,
  ItemUpdate,
  ItemUpdateSchema,
  GuidSchema,
} from '@models';
import { ValidationError } from '@errors';
import { SchemaValidator } from '@services';

// Bounds for list pagination, protecting the datastore from unbounded reads.
const DEFAULT_LIST_LIMIT = 100;
const MAX_LIST_LIMIT = 1000;

const coerceLimit = (raw?: string | number): number => {
  const parsed = typeof raw === 'number' ? raw : parseInt(raw ?? '', 10);
  if (!Number.isFinite(parsed) || parsed < 1) {
    return DEFAULT_LIST_LIMIT;
  }
  return Math.min(parsed, MAX_LIST_LIMIT);
};

@injectable()
export class ItemController {
  private _service: ItemService;
  private _validator: SchemaValidator;

  constructor(
    @inject(ItemService) service: ItemService,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (itemRequest: Item): Promise<Item> => {
    const item = this._validator.validate(itemRequest, ItemRequestSchema);
    return this._service.create(item);
  };

  get = async (id: string): Promise<Item> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get(id);
  };

  list = async (limit?: string | number): Promise<Item[]> =>
    this._service.list(coerceLimit(limit));

  update = async (itemRequest: Partial<ItemUpdate>): Promise<Item> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, ItemUpdateSchema);
    return this._service.update(item);
  };

  replace = async (itemRequest: Partial<ItemUpdate>): Promise<Item> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, ItemUpdateSchema);
    return this._service.replace(item);
  };

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete(id);
  };
}
