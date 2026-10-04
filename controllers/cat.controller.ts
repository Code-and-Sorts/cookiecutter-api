import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { CatService, SchemaValidator } from '@services';
import {
    CatResponse,
    CatRequestSchema,
    CatUpdateSchema,
} from '@models';
import { assertUuid, coerceLimit } from '@utils';

// Ids come only from the path, so a body can never set one.
@injectable()
export class CatController {
  private _service: CatService;
  private _validator: SchemaValidator;

  constructor(
    @inject(CatService) service: CatService,
    @inject(SchemaValidator) validator: SchemaValidator,
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (body: unknown, userId?: string): Promise<CatResponse> => {
    const item = this._validator.validate(body, CatRequestSchema);
    return this._service.create(item, userId);
  };

  get = async (id: string): Promise<CatResponse> => {
    assertUuid('Cat', id);
    return this._service.get(id);
  };

  list = async (limit?: string | number | null): Promise<CatResponse[]> =>
    this._service.list(coerceLimit(limit));

  update = async (id: string, body: unknown, userId?: string): Promise<CatResponse> => {
    assertUuid('Cat', id);
    const item = this._validator.validate(body, CatUpdateSchema);
    return this._service.update(id, item, userId);
  };

  delete = async (id: string, userId?: string): Promise<{ message: string }> => {
    assertUuid('Cat', id);
    await this._service.delete(id, userId);
    return { message: `Cat with id ${id} was deleted successfully.` };
  };
}
