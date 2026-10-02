import 'reflect-metadata';
import { Container } from 'inversify';

import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import {
  dynamoStoreFactory,
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


// Capped so a failing database gives the 500 within 10 seconds; without throwOnRequestTimeout
// the SDK only logs a warning when requestTimeout passes.
export const dynamoDbClientConfig = {
  region: env.AWS_REGION,
  maxAttempts: 2,
  requestHandler: { connectionTimeout: 1000, requestTimeout: DATABASE_ATTEMPT_TIMEOUT_MS, throwOnRequestTimeout: true },
};

const client = DynamoDBDocumentClient.from(new DynamoDBClient(dynamoDbClientConfig));

export const container = new Container({ defaultScope: 'Singleton' });
container.bind(StoreFactory).toConstantValue(dynamoStoreFactory(client));
container.bind(SchemaValidator).toSelf();
container.bind(KittenClawsRepository).toSelf();
container.bind(KittenClawsService).toSelf();
container.bind(KittenClawsController).toSelf();

export const kittenClawsController = container.get(KittenClawsController);
