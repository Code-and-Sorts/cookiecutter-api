import { inject, injectable } from 'inversify';
import 'reflect-metadata';
import {
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
} from '@services';
import {
{%- for resource in resources %}
    {{ resource.name }},
    {{ resource.name }}RequestSchema,
    {{ resource.name }}Update,
    {{ resource.name }}UpdateSchema,
{%- endfor %}
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
{% for resource in resources %}
{%- set r = resource.name %}
@injectable()
export class {{ r }}Controller {
  private _service: {{ r }}Service;
  private _validator: SchemaValidator;

  constructor(
    @inject({{ r }}Service) service: {{ r }}Service,
    @inject(SchemaValidator) validator: SchemaValidator
  ) {
    this._service = service;
    this._validator = validator;
  }
{%- if "create" in resource.operations %}

  post = async (itemRequest: {{ r }}): Promise<{{ r }}> => {
    const item = this._validator.validate(itemRequest, {{ r }}RequestSchema);
    return this._service.create(item);
  };
{%- endif %}
{%- if "get_by_id" in resource.operations %}

  get = async (id: string): Promise<{{ r }}> => {
    this._validator.validate(id, GuidSchema);
    return this._service.get(id);
  };
{%- endif %}
{%- if "list" in resource.operations %}

  list = async (limit?: string | number): Promise<{{ r }}[]> =>
    this._service.list(coerceLimit(limit));
{%- endif %}
{%- if "update" in resource.operations %}

  update = async (itemRequest: Partial<{{ r }}Update>): Promise<{{ r }}> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, {{ r }}UpdateSchema);
    return this._service.update(item);
  };
{%- endif %}
{%- if "replace" in resource.operations %}

  replace = async (itemRequest: Partial<{{ r }}Update>): Promise<{{ r }}> => {
    if (!itemRequest.id) {
      new ValidationError('Missing item id.')
    }
    this._validator.validate(itemRequest.id, GuidSchema);
    const item = this._validator.validate(itemRequest, {{ r }}UpdateSchema);
    return this._service.replace(item);
  };
{%- endif %}
{%- if "delete" in resource.operations %}

  delete = async (id: string): Promise<void> => {
    this._validator.validate(id, GuidSchema);
    await this._service.delete(id);
  };
{%- endif %}
}
{% endfor %}