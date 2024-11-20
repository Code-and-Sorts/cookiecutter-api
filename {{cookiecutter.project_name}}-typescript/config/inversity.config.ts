import "reflect-metadata";
import { Container } from 'inversify';
import { CosmosClient } from '@azure/cosmos';
import { {{cookiecutter.project_class_name}}Repository } from '@repository';
import { {{cookiecutter.project_class_name}}Controller } from '@controller';
import { {{cookiecutter.project_class_name}}Service, SchemaValidator } from '@service';
import { env } from '@models';

const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
});

export const container = new Container({ skipBaseClassChecks: true });
container.bind<SchemaValidator>(SchemaValidator).to(SchemaValidator);
container.bind<{{cookiecutter.project_class_name}}Controller>({{cookiecutter.project_class_name}}Controller).to({{cookiecutter.project_class_name}}Controller);
container.bind<{{cookiecutter.project_class_name}}Service>({{cookiecutter.project_class_name}}Service).to({{cookiecutter.project_class_name}}Service);
container.bind<{{cookiecutter.project_class_name}}Repository>({{cookiecutter.project_class_name}}Repository).to({{cookiecutter.project_class_name}}Repository);
container.bind<CosmosClient>(CosmosClient).toConstantValue(client);

export default container;
