import { inject, injectable } from 'inversify';
import 'reflect-metadata';
import { {{cookiecutter.project_class_name}}Service } from '@services';
import {
  {{cookiecutter.project_class_name}},
  {{cookiecutter.project_class_name}}RequestSchema,
  {{cookiecutter.project_class_name}}Update,
  {{cookiecutter.project_class_name}}UpdateSchema,
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
export class {{cookiecutter.project_class_name}}Controller {
  private _service: {{cookiecutter.project_class_name}}Service;
  private _validator: SchemaValidator;

  constructor(
    @inject({{cookiecutter.project_class_name}}Service) service: {{cookiecutter.project_class_name}}Service,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }

  post = async ({{cookiecutter.project_lower_camel_name}}Request: {{cookiecutter.project_class_name}}): Promise<{{cookiecutter.project_class_name}}> => {
    const {{cookiecutter.project_lower_camel_name}} = this._validator.validate({{cookiecutter.project_lower_camel_name}}Request, {{cookiecutter.project_class_name}}RequestSchema);
    return this._service.create{{cookiecutter.project_class_name}}({{cookiecutter.project_lower_camel_name}});
  };

  get = async (id: string): Promise<{{cookiecutter.project_class_name}}> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get{{cookiecutter.project_class_name}}(id);
  };

  list = async (limit?: string | number): Promise<{{cookiecutter.project_class_name}}[]> =>
    this._service.get{{cookiecutter.project_class_name}}s(coerceLimit(limit));

  update = async ({{cookiecutter.project_lower_camel_name}}Request: Partial<{{cookiecutter.project_class_name}}Update>): Promise<{{cookiecutter.project_class_name}}> => {
    if (!{{cookiecutter.project_lower_camel_name}}Request.id) {
      new ValidationError('Missing {{cookiecutter.project_lower_camel_name}} id.')
    }
    this._validator.validate({{cookiecutter.project_lower_camel_name}}Request.id, GuidSchema);
    const {{cookiecutter.project_lower_camel_name}} = this._validator.validate({{cookiecutter.project_lower_camel_name}}Request, {{cookiecutter.project_class_name}}UpdateSchema);
    return this._service.update{{cookiecutter.project_class_name}}({{cookiecutter.project_lower_camel_name}});
  };

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete{{cookiecutter.project_class_name}}(id);
  };
}
