import { inject, injectable } from 'inversify';
import { {{project_class_name}}Repository } from '@repositories';
import {
  {{project_class_name}},
  {{project_class_name}}Response,
  {{project_class_name}}ResponseSchema,
  {{project_class_name}}EntitySchema,
} from '@models';

@injectable()
export class {{project_class_name}}Service {
  private _repo: {{project_class_name}}Repository;

  constructor(@inject({{project_class_name}}Repository) repo: {{project_class_name}}Repository) {
    this._repo = repo;
  }

  create{{project_class_name}} = async ({{project_lower_camel_name}}: {{project_class_name}}): Promise<{{project_class_name}}Response> => {
    const new{{project_class_name}} = {{project_class_name}}EntitySchema.parse({{project_lower_camel_name}});
    const created{{project_class_name}}: {{project_class_name}}Response = await this._repo.create(new{{project_class_name}});
    return {{project_class_name}}ResponseSchema.parse(created{{project_class_name}});
  };

  get{{project_class_name}} = async (id: string): Promise<{{project_class_name}}Response> => {
    const {{project_lower_camel_name}} = await this._repo.get(id);
    return {{project_class_name}}ResponseSchema.parse({{project_lower_camel_name}});
  };

  get{{project_class_name}}s = async (limit?: number): Promise<{{project_class_name}}Response[]> => {
    const {{project_lower_camel_name}} = await this._repo.list(limit);
    return {{project_lower_camel_name}}.map(({{project_lower_camel_name}}) => {{project_class_name}}ResponseSchema.parse({{project_lower_camel_name}}));
  };

  update{{project_class_name}} = async ({{project_lower_camel_name}}: Partial<{{project_class_name}}Response>) => {
    const updated{{project_class_name}} = await this._repo.update({{project_lower_camel_name}});
    return {{project_class_name}}ResponseSchema.parse(updated{{project_class_name}});
  };

  delete{{project_class_name}} = async (id: string): Promise<void> => this._repo.delete(id);
}
