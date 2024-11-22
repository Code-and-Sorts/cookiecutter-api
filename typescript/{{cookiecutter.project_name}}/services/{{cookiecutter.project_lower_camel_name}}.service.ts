import { inject, injectable } from 'inversify';
import { {{cookiecutter.project_class_name}}Repository } from '@repositories';
import {
  {{cookiecutter.project_class_name}},
  {{cookiecutter.project_class_name}}Response,
  {{cookiecutter.project_class_name}}ResponseSchema,
  {{cookiecutter.project_class_name}}EntitySchema,
} from '@models';

@injectable()
export class {{cookiecutter.project_class_name}}Service {
  private _repo: {{cookiecutter.project_class_name}}Repository;

  constructor(@inject({{cookiecutter.project_class_name}}Repository) repo: {{cookiecutter.project_class_name}}Repository) {
    this._repo = repo;
  }

  create{{cookiecutter.project_class_name}} = async ({{cookiecutter.project_lower_camel_name}}: {{cookiecutter.project_class_name}}): Promise<{{cookiecutter.project_class_name}}Response> => {
    const new{{cookiecutter.project_class_name}} = {{cookiecutter.project_class_name}}EntitySchema.parse({{cookiecutter.project_lower_camel_name}});
    const created{{cookiecutter.project_class_name}}: {{cookiecutter.project_class_name}}Response = await this._repo.create(new{{cookiecutter.project_class_name}});
    return {{cookiecutter.project_class_name}}ResponseSchema.parse(created{{cookiecutter.project_class_name}});
  };

  get{{cookiecutter.project_class_name}} = async (id: string): Promise<{{cookiecutter.project_class_name}}Response> => {
    const {{cookiecutter.project_lower_camel_name}} = await this._repo.get(id);
    return {{cookiecutter.project_class_name}}ResponseSchema.parse({{cookiecutter.project_lower_camel_name}});
  };

  get{{cookiecutter.project_class_name}}s = async (): Promise<{{cookiecutter.project_class_name}}Response[]> => {
    const {{cookiecutter.project_lower_camel_name}} = await this._repo.list();
    return {{cookiecutter.project_lower_camel_name}}.map(({{cookiecutter.project_lower_camel_name}}) => {{cookiecutter.project_class_name}}ResponseSchema.parse({{cookiecutter.project_lower_camel_name}}));
  };

  update{{cookiecutter.project_class_name}} = async ({{cookiecutter.project_lower_camel_name}}: Partial<{{cookiecutter.project_class_name}}Response>) => {
    const updated{{cookiecutter.project_class_name}} = await this._repo.update({{cookiecutter.project_lower_camel_name}});
    return {{cookiecutter.project_class_name}}ResponseSchema.parse(updated{{cookiecutter.project_class_name}});
  };

  delete{{cookiecutter.project_class_name}} = async (id: string): Promise<void> => this._repo.delete(id);
}
