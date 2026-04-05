import "reflect-metadata";
import { Container } from 'inversify';
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { CosmosClient } from '@azure/cosmos';
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import { {{cookiecutter.project_class_name}}Repository } from '@repositories';
import { {{cookiecutter.project_class_name}}Controller } from '@controllers';
import { {{cookiecutter.project_class_name}}Service, SchemaValidator } from '@services';
import { env } from '@models';
{% if cookiecutter.cloud_service == 'Azure Function App' %}
const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
});
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
const client = new Firestore({
  projectId: env.GCP_PROJECT_ID,
  databaseId: env.FIRESTORE_DATABASE,
});
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
const dynamoClient = new DynamoDBClient({ region: env.AWS_REGION });
const client = DynamoDBDocumentClient.from(dynamoClient);
{%- endif %}

export const container = new Container({ skipBaseClassChecks: true });
container.bind<SchemaValidator>(SchemaValidator).to(SchemaValidator);
container.bind<{{cookiecutter.project_class_name}}Controller>({{cookiecutter.project_class_name}}Controller).to({{cookiecutter.project_class_name}}Controller);
container.bind<{{cookiecutter.project_class_name}}Service>({{cookiecutter.project_class_name}}Service).to({{cookiecutter.project_class_name}}Service);
container.bind<{{cookiecutter.project_class_name}}Repository>({{cookiecutter.project_class_name}}Repository).to({{cookiecutter.project_class_name}}Repository);
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
container.bind<CosmosClient>(CosmosClient).toConstantValue(client);
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
container.bind<Firestore>(Firestore).toConstantValue(client);
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
container.bind<DynamoDBDocumentClient>(DynamoDBDocumentClient).toConstantValue(client);
{%- endif %}

export default container;
