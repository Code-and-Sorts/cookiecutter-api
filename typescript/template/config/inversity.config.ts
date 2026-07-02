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
import { ItemRepository } from '@repositories';
import { ItemController } from '@controllers';
import { ItemService, SchemaValidator } from '@services';
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

// One controller is wired per distinct container; resources that share a
// container id share the same storage handle.
export const controllers: Record<string, ItemController> = {
{%- for container in resources | map(attribute='container') | unique %}
{%- if cloud_service == 'Azure Function App' %}
  "{{ container }}": new ItemController(new ItemService(new ItemRepository(database.container(env.COSMOS_CONTAINER_{{ container | upper | replace('-', '_') }}))), validator),
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
  "{{ container }}": new ItemController(new ItemService(new ItemRepository(client.collection(env.FIRESTORE_COLLECTION_{{ container | upper | replace('-', '_') }}))), validator),
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
  "{{ container }}": new ItemController(new ItemService(new ItemRepository(client, env.DYNAMODB_TABLE_NAME_{{ container | upper | replace('-', '_') }})), validator),
{%- endif %}
{%- endfor %}
};

export default controllers;
