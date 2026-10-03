import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { VisitService, SchemaValidator } from '@services';
import {
    VisitResponse,
    VisitCreateRequestSchema,
    VisitUpdateRequestSchema,
} from '@models';
import { assertUuid } from '@utils';

// Ids come only from the path, so a body can never set one.
@injectable()
export class VisitController {
  private _service: VisitService;
  private _validator: SchemaValidator;

  constructor(
    @inject(VisitService) service: VisitService,
    @inject(SchemaValidator) validator: SchemaValidator,
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async (body: unknown, userId?: string): Promise<VisitResponse> => {
    const item = this._validator.validate(body, VisitCreateRequestSchema);
    return this._service.create(item, userId);
  };

  get = async (id: string): Promise<VisitResponse> => {
    assertUuid('Visit', id);
    return this._service.get(id);
  };

  update = async (id: string, body: unknown, userId?: string): Promise<VisitResponse> => {
    assertUuid('Visit', id);
    const item = this._validator.validate(body, VisitUpdateRequestSchema);
    return this._service.update(id, item, userId);
  };
}
