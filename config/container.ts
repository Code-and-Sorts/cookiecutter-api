import 'reflect-metadata';
import { Container } from 'inversify';

import { Firestore } from '@google-cloud/firestore';
import {
  firestoreStoreFactory,
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
container.bind(KittenClawsRepository).toSelf();
container.bind(KittenClawsService).toSelf();
container.bind(KittenClawsController).toSelf();

export const kittenClawsController = container.get(KittenClawsController);
