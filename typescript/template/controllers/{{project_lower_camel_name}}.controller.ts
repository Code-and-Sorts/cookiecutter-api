import { inject, injectable } from 'inversify';
import 'reflect-metadata';
import { {{project_class_name}}Service } from '@services';
import {
  {{project_class_name}},
  {{project_class_name}}RequestSchema,
  {{project_class_name}}Update,
  {{project_class_name}}UpdateSchema,
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
export class {{project_class_name}}Controller {
  private _service: {{project_class_name}}Service;
  private _validator: SchemaValidator;

  constructor(
    @inject({{project_class_name}}Service) service: {{project_class_name}}Service,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async ({{project_lower_camel_name}}Request: {{project_class_name}}): Promise<{{project_class_name}}> => {
    const {{project_lower_camel_name}} = this._validator.validate({{project_lower_camel_name}}Request, {{project_class_name}}RequestSchema);
    return this._service.create{{project_class_name}}({{project_lower_camel_name}});
  };

  get = async (id: string): Promise<{{project_class_name}}> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get{{project_class_name}}(id);
  };

  list = async (limit?: string | number): Promise<{{project_class_name}}[]> =>
    this._service.get{{project_class_name}}s(coerceLimit(limit));

  update = async ({{project_lower_camel_name}}Request: Partial<{{project_class_name}}Update>): Promise<{{project_class_name}}> => {
    if (!{{project_lower_camel_name}}Request.id) {
      new ValidationError('Missing {{project_lower_camel_name}} id.')
    }
    this._validator.validate({{project_lower_camel_name}}Request.id, GuidSchema);
    const {{project_lower_camel_name}} = this._validator.validate({{project_lower_camel_name}}Request, {{project_class_name}}UpdateSchema);
    return this._service.update{{project_class_name}}({{project_lower_camel_name}});
  };

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete{{project_class_name}}(id);
  };
}
