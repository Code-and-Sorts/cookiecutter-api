import {
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
} from '@repositories';
import {
{%- for resource in resources %}
    {{ resource.name }},
    {{ resource.name }}Response,
    {{ resource.name }}ResponseSchema,
    {{ resource.name }}EntitySchema,
    {{ resource.name }}Update,
{%- endfor %}
} from '@models';
{% for resource in resources %}
{%- set r = resource.name %}
export class {{ r }}Service {
  private _repo: {{ r }}Repository;

  constructor(repo: {{ r }}Repository) {
    this._repo = repo;
  }
{%- if "create" in resource.operations %}

  create = async (item: {{ r }}): Promise<{{ r }}Response> => {
    const newItem = {{ r }}EntitySchema.parse(item);
    const createdItem: {{ r }}Response = await this._repo.create(newItem);
    return {{ r }}ResponseSchema.parse(createdItem);
  };
{%- endif %}
{%- if "get_by_id" in resource.operations %}

  get = async (id: string): Promise<{{ r }}Response> => {
    const item = await this._repo.get(id);
    return {{ r }}ResponseSchema.parse(item);
  };
{%- endif %}
{%- if "list" in resource.operations %}

  list = async (limit?: number): Promise<{{ r }}Response[]> => {
    const items = await this._repo.list(limit);
    return items.map((item) => {{ r }}ResponseSchema.parse(item));
  };
{%- endif %}
{%- if "update" in resource.operations %}

  update = async (item: {{ r }}Update): Promise<{{ r }}Response> => {
    const updatedItem = await this._repo.update({ ...item, updatedTimestamp: new Date().toISOString() });
    return {{ r }}ResponseSchema.parse(updatedItem);
  };
{%- endif %}
{%- if "replace" in resource.operations %}

  replace = async (item: {{ r }}Update): Promise<{{ r }}Response> => {
    const existing = await this._repo.get(item.id);
    const entity = {{ r }}EntitySchema.parse({
      ...item,
      id: existing.id,
      isDeleted: false,
      createdTimestamp: existing.createdTimestamp,
      createdBy: existing.createdBy,
      updatedTimestamp: new Date().toISOString(),
    });
    const replacedItem = await this._repo.replace(entity);
    return {{ r }}ResponseSchema.parse(replacedItem);
  };
{%- endif %}
{%- if "delete" in resource.operations %}

  delete = async (id: string): Promise<void> => this._repo.delete(id);
{%- endif %}
}
{% endfor %}