import 'reflect-metadata';
import { Container } from 'inversify';
{% if cloud_service == 'Azure Function App' -%}
import { CosmosClient, Database } from '@azure/cosmos';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import {
{%- if cloud_service == 'AWS Lambda' %}
  DocumentClient,
{%- endif %}
{%- for resource in resources %}
  {{ resource.name }}Repository,
{%- endfor %}
} from '@repositories';
import {
{%- for resource in resources %}
  {{ resource.name }}Controller,
{%- endfor %}
} from '@controllers';
import {
{%- for resource in resources %}
  {{ resource.name }}Service,
{%- endfor %}
  SchemaValidator,
} from '@services';
import { env } from '@models';

{% if cloud_service == 'Azure Function App' -%}
const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
});
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
const client = new Firestore({
  projectId: env.GCP_PROJECT_ID,
  databaseId: env.FIRESTORE_DATABASE,
});
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
const client = DynamoDBDocumentClient.from(new DynamoDBClient({ region: env.AWS_REGION }));
{%- endif %}

export const container = new Container({ defaultScope: 'Singleton' });
{% if cloud_service == 'Azure Function App' -%}
container.bind(Database).toConstantValue(client.database(env.COSMOS_DB_DATABASE_NAME));
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
container.bind(Firestore).toConstantValue(client);
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
container.bind(DocumentClient).toConstantValue(client);
{%- endif %}
container.bind(SchemaValidator).toSelf();
{%- for resource in resources %}
container.bind({{ resource.name }}Repository).toSelf();
container.bind({{ resource.name }}Service).toSelf();
container.bind({{ resource.name }}Controller).toSelf();
{%- endfor %}
{% for resource in resources %}
export const {{ resource.name | to_lower_camel }}Controller = container.get({{ resource.name }}Controller);
{%- endfor %}
