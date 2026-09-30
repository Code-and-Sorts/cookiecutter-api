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
import { DATABASE_ATTEMPT_TIMEOUT_MS{% if cloud_service == 'GCP Cloud Function' %}, DATABASE_DEADLINE_MS{% endif %} } from '@utils';

{% if cloud_service == 'Azure Function App' -%}
// Short per-request timeouts and few retries, so a failing database surfaces quickly
// (repositories also bound each operation with withDeadline).
export const cosmosConnectionPolicy = {
  requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS,
  retryOptions: { maxRetryAttemptCount: 2, fixedRetryIntervalInMilliseconds: 200, maxWaitTimeInSeconds: 5 },
};

const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
  connectionPolicy: cosmosConnectionPolicy,
});
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
// Short per-call timeouts and a capped retry budget, so a failing database surfaces quickly
// (repositories also bound each operation with withDeadline).
export const firestoreRetryParams = {
  initial_retry_delay_millis: 100,
  retry_delay_multiplier: 1.3,
  max_retry_delay_millis: 1000,
  initial_rpc_timeout_millis: DATABASE_ATTEMPT_TIMEOUT_MS,
  rpc_timeout_multiplier: 1,
  max_rpc_timeout_millis: DATABASE_ATTEMPT_TIMEOUT_MS,
  total_timeout_millis: DATABASE_DEADLINE_MS,
};

const client = new Firestore({
  projectId: env.GCP_PROJECT_ID,
  databaseId: env.FIRESTORE_DATABASE,
  clientConfig: {
    interfaces: { 'google.firestore.v1.Firestore': { retry_params: { default: firestoreRetryParams } } },
  },
});
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
// Short per-attempt timeouts and two attempts at most, so a failing database surfaces quickly
// (repositories also bound each operation with withDeadline).
export const dynamoDbClientConfig = {
  region: env.AWS_REGION,
  maxAttempts: 2,
  requestHandler: { connectionTimeout: 1000, requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS, throwOnRequestTimeout: true },
};

const client = DynamoDBDocumentClient.from(new DynamoDBClient(dynamoDbClientConfig));
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
