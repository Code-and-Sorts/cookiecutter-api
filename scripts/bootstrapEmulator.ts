
import { env } from '@models';

const TIMEOUT_MS = 120_000;
const MAX_DELAY_MS = 8_000;

class NotEmulatorError extends Error {}

const storeNames = (): string[] =>
  [
    env.FIRESTORE_COLLECTION_ANIMALS,
  ]
    .filter((name, index, names) => names.indexOf(name) === index)
    .sort();

const bootstrap = async (): Promise<void> => {
  const host = env.FIRESTORE_EMULATOR_HOST;
  if (!host) {
    throw new NotEmulatorError('FIRESTORE_EMULATOR_HOST is not set; refusing to run outside the emulator.');
  }
  const response = await fetch(`http://${host}/`, { signal: AbortSignal.timeout(5_000) });
  if ((await response.text()).trim() !== 'Ok') {
    throw new Error(`${host} is not a Firestore emulator.`);
  }
  console.log(`Firestore emulator at ${host} is ready for project ${env.GCP_PROJECT_ID} (collections: ${storeNames().join(', ')}).`);
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
