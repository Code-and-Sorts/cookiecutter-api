
import { CosmosClient, PartitionKeyKind } from '@azure/cosmos';
import { cosmosClientOptions } from '@config/container';
import { env } from '@models';

const TIMEOUT_MS = 120_000;
const MAX_DELAY_MS = 8_000;

class NotEmulatorError extends Error {}

const storeNames = (): string[] =>
  [
    env.COSMOS_CONTAINER_ANIMALS,
  ]
    .filter((name, index, names) => names.indexOf(name) === index)
    .sort();

const bootstrap = async (): Promise<void> => {
  if (!env.COSMOS_DB_EMULATOR) {
    throw new NotEmulatorError('COSMOS_DB_EMULATOR is not true; refusing to create containers outside the emulator.');
  }
  const client = new CosmosClient(cosmosClientOptions(env));
  try {
    const { database } = await client.databases.createIfNotExists({ id: env.COSMOS_DB_DATABASE_NAME });
    for (const name of storeNames()) {
      // The emulator stores the definition as sent, and SDKs in other languages require its kind.
      await database.containers.createIfNotExists({
        id: name,
        partitionKey: { paths: ['/id'], kind: PartitionKeyKind.Hash },
      });
      console.log(`Container ${env.COSMOS_DB_DATABASE_NAME}/${name} is ready.`);
    }
  } finally {
    client.dispose();
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
