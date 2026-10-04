import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { DogService, SchemaValidator } from '@services';
import {
    DogResponse,
    DogRequestSchema,
} from '@models';
import { assertUuid, coerceLimit } from '@utils';

// Ids come only from the path, so a body can never set one.
@injectable()
export class DogController {
  private _service: DogService;
  private _validator: SchemaValidator;

  constructor(
    @inject(DogService) service: DogService,
    @inject(SchemaValidator) validator: SchemaValidator,
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (body: unknown, userId?: string): Promise<DogResponse> => {
    const item = this._validator.validate(body, DogRequestSchema);
    return this._service.create(item, userId);
  };

  get = async (id: string): Promise<DogResponse> => {
    assertUuid('Dog', id);
    return this._service.get(id);
  };

  list = async (limit?: string | number | null): Promise<DogResponse[]> =>
    this._service.list(coerceLimit(limit));

  replace = async (id: string, body: unknown, userId?: string): Promise<DogResponse> => {
    assertUuid('Dog', id);
    const item = this._validator.validate(body, DogRequestSchema);
    return this._service.replace(id, item, userId);
  };

  delete = async (id: string, userId?: string): Promise<{ message: string }> => {
    assertUuid('Dog', id);
    await this._service.delete(id, userId);
    return { message: `Dog with id ${id} was deleted successfully.` };
  };
}
