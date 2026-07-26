import { inject, injectable } from 'inversify';
import 'reflect-metadata';
import {
    CatService,
    DogService,
} from '@services';
import {
    Cat,
    CatRequestSchema,
    CatUpdate,
    CatUpdateSchema,
    Dog,
    DogRequestSchema,
    DogUpdate,
    DogUpdateSchema,
    GuidSchema,
} from '@models';
import { ValidationError } from '@errors';
import { SchemaValidator } from '@services';

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
export class CatController {
  private _service: CatService;
  private _validator: SchemaValidator;

  constructor(
    @inject(CatService) service: CatService,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (itemRequest: Cat): Promise<Cat> => {
    const item = this._validator.validate(itemRequest, CatRequestSchema);
    return this._service.create(item);
  };

  get = async (id: string): Promise<Cat> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get(id);
  };

  list = async (limit?: string | number): Promise<Cat[]> =>
    this._service.list(coerceLimit(limit));

  update = async (itemRequest: Partial<CatUpdate>): Promise<Cat> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, CatUpdateSchema);
    return this._service.update(item);
  };

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete(id);
  };
}

@injectable()
export class DogController {
  private _service: DogService;
  private _validator: SchemaValidator;

  constructor(
    @inject(DogService) service: DogService,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (itemRequest: Dog): Promise<Dog> => {
    const item = this._validator.validate(itemRequest, DogRequestSchema);
    return this._service.create(item);
  };

  get = async (id: string): Promise<Dog> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get(id);
  };

  list = async (limit?: string | number): Promise<Dog[]> =>
    this._service.list(coerceLimit(limit));

  replace = async (itemRequest: Partial<DogUpdate>): Promise<Dog> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, DogUpdateSchema);
    return this._service.replace(item);
  };

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete(id);
  };
}
