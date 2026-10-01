{%- set prefix = {'Azure Function App': 'COSMOS_CONTAINER_', 'GCP Cloud Function': 'FIRESTORE_COLLECTION_', 'AWS Lambda': 'DYNAMODB_TABLE_NAME_'}[cloud_service] -%}
// Prepares the local emulator for the app (yarn emulator:seed); it is safe to run again.
{%- if cloud_service == 'Azure Function App' %}
import { CosmosClient } from '@azure/cosmos';
import { cosmosClientOptions } from '@config/container';
{%- elif cloud_service == 'AWS Lambda' %}
import { CreateTableCommand, DynamoDBClient, waitUntilTableExists } from '@aws-sdk/client-dynamodb';
import { dynamoDbClientConfig } from '@config/container';
{%- endif %}
import { env } from '@models';

const TIMEOUT_MS = 120_000;
const MAX_DELAY_MS = 8_000;

class NotEmulatorError extends Error {}

// Same settings the repositories read, so the app and the bootstrap agree on store names.
const storeNames = (): string[] =>
  [
{%- for container in resources | map(attribute='container') | unique %}
    env.{{ prefix }}{{ container | upper | replace('-', '_') }},
{%- endfor %}
  ]
    .filter((name, index, names) => names.indexOf(name) === index)
    .sort();
{%- if cloud_service == 'Azure Function App' %}

const bootstrap = async (): Promise<void> => {
  if (!env.COSMOS_DB_EMULATOR) {
    throw new NotEmulatorError('COSMOS_DB_EMULATOR is not true; refusing to create containers outside the emulator.');
  }
  const client = new CosmosClient(cosmosClientOptions(env));
  try {
    const { database } = await client.databases.createIfNotExists({ id: env.COSMOS_DB_DATABASE_NAME });
    for (const name of storeNames()) {
      // Every repository reads and writes items by id, so id is the partition key.
      await database.containers.createIfNotExists({ id: name, partitionKey: { paths: ['/id'] } });
      console.log(`Container ${env.COSMOS_DB_DATABASE_NAME}/${name} is ready.`);
    }
  } finally {
    client.dispose();
  }
};
{%- elif cloud_service == 'GCP Cloud Function' %}

const bootstrap = async (): Promise<void> => {
  const host = env.FIRESTORE_EMULATOR_HOST;
  if (!host) {
    throw new NotEmulatorError('FIRESTORE_EMULATOR_HOST is not set; refusing to run outside the emulator.');
  }
  // Firestore creates collections on first write, so a reachable emulator is all the app needs.
  const response = await fetch(`http://${host}/`, { signal: AbortSignal.timeout(5_000) });
  if ((await response.text()).trim() !== 'Ok') {
    throw new Error(`${host} is not a Firestore emulator.`);
  }
  console.log(`Firestore emulator at ${host} is ready for project ${env.GCP_PROJECT_ID} (collections: ${storeNames().join(', ')}).`);
};
{%- else %}

const bootstrap = async (): Promise<void> => {
  if (!env.AWS_ENDPOINT_URL_DYNAMODB) {
    throw new NotEmulatorError('AWS_ENDPOINT_URL_DYNAMODB is not set; refusing to create tables outside the emulator.');
  }
  const client = new DynamoDBClient(dynamoDbClientConfig);
  try {
    for (const name of storeNames()) {
      try {
        // Matches the tables in template.yaml.
        await client.send(
          new CreateTableCommand({
            TableName: name,
            AttributeDefinitions: [{ AttributeName: 'id', AttributeType: 'S' }],
            KeySchema: [{ AttributeName: 'id', KeyType: 'HASH' }],
            BillingMode: 'PAY_PER_REQUEST',
          }),
        );
      } catch (error) {
        if (error?.name !== 'ResourceInUseException') {
          throw error;
        }
      }
      await waitUntilTableExists({ client, maxWaitTime: 30 }, { TableName: name });
      console.log(`Table ${name} is ready.`);
    }
  } finally {
    client.destroy();
  }
};
{%- endif %}

const main = async (): Promise<void> => {
  const deadline = Date.now() + TIMEOUT_MS;
  for (let delay = 1_000; ; delay = Math.min(delay * 2, MAX_DELAY_MS)) {
    try {
      await bootstrap();
      return;
    } catch (error) {
      if (error instanceof NotEmulatorError) {
        throw error;
      }
      if (Date.now() + delay > deadline) {
        throw new Error(`The emulator was not ready within ${TIMEOUT_MS / 1000} s.`, { cause: error });
      }
      console.error(`Waiting for the emulator (${error?.name ?? error}); retrying in ${delay / 1000} s.`);
      await new Promise((resolve) => setTimeout(resolve, delay));
    }
  }
};

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
