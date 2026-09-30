import 'reflect-metadata';
import { Container } from 'inversify';
import { CosmosClient } from '@azure/cosmos';
import {
  cosmosStoreFactory,
  StoreFactory,
  CatRepository,
  DogRepository,
} from '@repositories';
import {
  CatController,
  DogController,
} from '@controllers';
import {
  CatService,
  DogService,
  SchemaValidator,
} from '@services';
import { env } from '@models';
import { DATABASE_ATTEMPT_TIMEOUT_MS } from '@utils';

// Capped so a failing database gives the 500 within 10 seconds.
export const cosmosConnectionPolicy = {
  requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS,
  retryOptions: { maxRetryAttemptCount: 2, fixedRetryIntervalInMilliseconds: 200, maxWaitTimeInSeconds: 5 },
};

const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
  connectionPolicy: cosmosConnectionPolicy,
});

export const container = new Container({ defaultScope: 'Singleton' });
container.bind(StoreFactory).toConstantValue(cosmosStoreFactory(client.database(env.COSMOS_DB_DATABASE_NAME)));
container.bind(SchemaValidator).toSelf();
container.bind(CatRepository).toSelf();
container.bind(CatService).toSelf();
container.bind(CatController).toSelf();
container.bind(DogRepository).toSelf();
container.bind(DogService).toSelf();
container.bind(DogController).toSelf();

export const catController = container.get(CatController);
export const dogController = container.get(DogController);
