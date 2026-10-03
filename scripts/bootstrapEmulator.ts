
import { CreateTableCommand, DynamoDBClient, waitUntilTableExists } from '@aws-sdk/client-dynamodb';
import { dynamoDbClientConfig } from '@config/container';
import { env } from '@models';

const TIMEOUT_MS = 120_000;
const MAX_DELAY_MS = 8_000;

class NotEmulatorError extends Error {}

const storeNames = (): string[] =>
  [
    env.DYNAMODB_TABLE_NAME_ANIMALS,
  ]
    .filter((name, index, names) => names.indexOf(name) === index)
    .sort();

const bootstrap = async (): Promise<void> => {
  if (!env.AWS_ENDPOINT_URL_DYNAMODB) {
    throw new NotEmulatorError('AWS_ENDPOINT_URL_DYNAMODB is not set; refusing to create tables outside the emulator.');
  }
  const client = new DynamoDBClient(dynamoDbClientConfig);
  try {
    for (const name of storeNames()) {
      try {
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
