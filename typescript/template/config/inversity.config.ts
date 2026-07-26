import "reflect-metadata";
{% if cloud_service == 'Azure Function App' -%}
import { CosmosClient } from '@azure/cosmos';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import {
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
const database = client.database(env.COSMOS_DB_DATABASE_NAME);
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
const client = new Firestore({
  projectId: env.GCP_PROJECT_ID,
  databaseId: env.FIRESTORE_DATABASE,
});
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
const dynamoClient = new DynamoDBClient({ region: env.AWS_REGION });
const client = DynamoDBDocumentClient.from(dynamoClient);
{%- endif %}

const validator = new SchemaValidator();

{%- for resource in resources %}
{%- set r = resource.name %}
{%- if cloud_service == 'Azure Function App' %}
export const {{ r | to_lower_camel }}Controller = new {{ r }}Controller(new {{ r }}Service(new {{ r }}Repository(database.container(env.COSMOS_CONTAINER_{{ resource.container | upper | replace('-', '_') }}))), validator);
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
export const {{ r | to_lower_camel }}Controller = new {{ r }}Controller(new {{ r }}Service(new {{ r }}Repository(client.collection(env.FIRESTORE_COLLECTION_{{ resource.container | upper | replace('-', '_') }}))), validator);
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
export const {{ r | to_lower_camel }}Controller = new {{ r }}Controller(new {{ r }}Service(new {{ r }}Repository(client, env.DYNAMODB_TABLE_NAME_{{ resource.container | upper | replace('-', '_') }})), validator);
{%- endif %}
{%- endfor %}
