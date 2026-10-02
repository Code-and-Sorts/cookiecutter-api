import 'reflect-metadata';
import { Container } from 'inversify';
import { Agent } from 'node:https';
import { CosmosClient, CosmosClientOptions } from '@azure/cosmos';
import {
  cosmosStoreFactory,
  StoreFactory,
  KittenClawsRepository,
} from '@repositories';
import {
  KittenClawsController,
} from '@controllers';
import {
  KittenClawsService,
  SchemaValidator,
} from '@services';
import { env } from '@models';
import { DATABASE_ATTEMPT_TIMEOUT_MS } from '@utils';

// Capped so a failing database gives the 500 within 10 seconds.
export const cosmosConnectionPolicy = {
  requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS,
  retryOptions: { maxRetryAttemptCount: 2, fixedRetryIntervalInMilliseconds: 200, maxWaitTimeInSeconds: 5 },
};

type CosmosSettings = Pick<typeof env, 'COSMOS_DB_URL' | 'COSMOS_DB_KEY' | 'COSMOS_DB_EMULATOR'>;

export const cosmosClientOptions = (settings: CosmosSettings): CosmosClientOptions => {
  const options = { endpoint: settings.COSMOS_DB_URL, key: settings.COSMOS_DB_KEY, connectionPolicy: cosmosConnectionPolicy };
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

export const container = new Container({ defaultScope: 'Singleton' });
container.bind(StoreFactory).toConstantValue(cosmosStoreFactory(client.database(env.COSMOS_DB_DATABASE_NAME)));
container.bind(SchemaValidator).toSelf();
container.bind(KittenClawsRepository).toSelf();
container.bind(KittenClawsService).toSelf();
container.bind(KittenClawsController).toSelf();

export const kittenClawsController = container.get(KittenClawsController);
