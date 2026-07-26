import { inject, injectable } from 'inversify';
import 'reflect-metadata';
import {
    KittenClawsService,
} from '@services';
import {
    KittenClaws,
    KittenClawsRequestSchema,
    KittenClawsUpdate,
    KittenClawsUpdateSchema,
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
export class KittenClawsController {
  private _service: KittenClawsService;
  private _validator: SchemaValidator;

  constructor(
    @inject(KittenClawsService) service: KittenClawsService,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (itemRequest: KittenClaws): Promise<KittenClaws> => {
    const item = this._validator.validate(itemRequest, KittenClawsRequestSchema);
    return this._service.create(item);
  };

  get = async (id: string): Promise<KittenClaws> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get(id);
  };

  list = async (limit?: string | number): Promise<KittenClaws[]> =>
    this._service.list(coerceLimit(limit));

  update = async (itemRequest: Partial<KittenClawsUpdate>): Promise<KittenClaws> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, KittenClawsUpdateSchema);
    return this._service.update(item);
  };

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete(id);
  };
}
