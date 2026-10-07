import 'reflect-metadata';
import { Container } from 'inversify';
{% if cloud_service == 'Azure Function App' -%}
import { Agent } from 'node:https';
import { CosmosClient, CosmosClientOptions } from '@azure/cosmos';
import { DefaultAzureCredential } from '@azure/identity';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import {
{%- if cloud_service == 'Azure Function App' %}
  cosmosStoreFactory,
{%- elif cloud_service == 'GCP Cloud Function' %}
  firestoreStoreFactory,
{%- else %}
  dynamoStoreFactory,
{%- endif %}
  StoreFactory,
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
// Capped so a failing database gives the 500 within 10 seconds.
export const cosmosConnectionPolicy = {
  requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS,
  retryOptions: { maxRetryAttemptCount: 2, fixedRetryIntervalInMilliseconds: 200, maxWaitTimeInSeconds: 5 },
};

type CosmosSettings = Pick<typeof env, 'COSMOS_DB_URL' | 'COSMOS_DB_KEY' | 'COSMOS_DB_EMULATOR'>;

export const cosmosClientOptions = (settings: CosmosSettings): CosmosClientOptions => {
  const options: CosmosClientOptions = {
    endpoint: settings.COSMOS_DB_URL,
    ...(settings.COSMOS_DB_KEY ? { key: settings.COSMOS_DB_KEY } : { aadCredentials: new DefaultAzureCredential() }),
    connectionPolicy: cosmosConnectionPolicy,
  };
  if (!settings.COSMOS_DB_EMULATOR) {
    return options;
  }
  return {
    ...options,
    connectionPolicy: { ...cosmosConnectionPolicy, enableEndpointDiscovery: false },
    // Only an emulator serving HTTPS gets here; its certificate is self-signed.
    ...(settings.COSMOS_DB_URL.toLowerCase().startsWith('https:') && { agent: new Agent({ rejectUnauthorized: false }) }),
  };
};

const client = new CosmosClient(cosmosClientOptions(env));
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
// Capped so a failing database gives the 500 within 10 seconds.
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
// Capped so a failing database gives the 500 within 10 seconds; without throwOnRequestTimeout
// the SDK only logs a warning when requestTimeout passes.
export const dynamoDbClientConfig = {
  region: env.AWS_REGION,
  maxAttempts: 2,
  requestHandler: { connectionTimeout: 1000, requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS, throwOnRequestTimeout: true },
};

const client = DynamoDBDocumentClient.from(new DynamoDBClient(dynamoDbClientConfig));
{%- endif %}

export const container = new Container({ defaultScope: 'Singleton' });
{% if cloud_service == 'Azure Function App' -%}
container.bind(StoreFactory).toConstantValue(cosmosStoreFactory(client.database(env.COSMOS_DB_DATABASE_NAME)));
{%- elif cloud_service == 'GCP Cloud Function' -%}
container.bind(StoreFactory).toConstantValue(firestoreStoreFactory(client));
{%- else -%}
container.bind(StoreFactory).toConstantValue(dynamoStoreFactory(client));
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
