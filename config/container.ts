import 'reflect-metadata';
import { Container } from 'inversify';

import { Firestore } from '@google-cloud/firestore';
import {
  firestoreStoreFactory,
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
import { DATABASE_ATTEMPT_TIMEOUT_MS, DATABASE_DEADLINE_MS } from '@utils';


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

export const container = new Container({ defaultScope: 'Singleton' });
container.bind(StoreFactory).toConstantValue(firestoreStoreFactory(client));
container.bind(SchemaValidator).toSelf();
container.bind(CatRepository).toSelf();
container.bind(CatService).toSelf();
container.bind(CatController).toSelf();
container.bind(DogRepository).toSelf();
container.bind(DogService).toSelf();
container.bind(DogController).toSelf();

export const catController = container.get(CatController);
export const dogController = container.get(DogController);
