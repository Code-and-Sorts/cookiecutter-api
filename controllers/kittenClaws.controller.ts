import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { KittenClawsService, SchemaValidator } from '@services';
import {
    KittenClawsResponse,
    KittenClawsCreateRequestSchema,
    KittenClawsUpdateRequestSchema,
} from '@models';
import { assertUuid, coerceLimit } from '@utils';

// Ids come only from the path, so a body can never set one.
@injectable()
export class KittenClawsController {
  private _service: KittenClawsService;
  private _validator: SchemaValidator;

  constructor(
    @inject(KittenClawsService) service: KittenClawsService,
    @inject(SchemaValidator) validator: SchemaValidator,
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (body: unknown, userId?: string): Promise<KittenClawsResponse> => {
    const item = this._validator.validate(body, KittenClawsCreateRequestSchema);
    return this._service.create(item, userId);
  };

  get = async (id: string): Promise<KittenClawsResponse> => {
    assertUuid('KittenClaws', id);
    return this._service.get(id);
  };

  list = async (limit?: string | number | null): Promise<KittenClawsResponse[]> =>
    this._service.list(coerceLimit(limit));

  update = async (id: string, body: unknown, userId?: string): Promise<KittenClawsResponse> => {
    assertUuid('KittenClaws', id);
    const item = this._validator.validate(body, KittenClawsUpdateRequestSchema);
    return this._service.update(id, item, userId);
  };

  delete = async (id: string, userId?: string): Promise<{ message: string }> => {
    assertUuid('KittenClaws', id);
    await this._service.delete(id, userId);
    return { message: `KittenClaws with id ${id} was deleted successfully.` };
  };
}
