import 'reflect-metadata';
import { Container } from 'inversify';

import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import {
  DocumentClient,
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


const client = DynamoDBDocumentClient.from(new DynamoDBClient({ region: env.AWS_REGION }));

export const container = new Container({ defaultScope: 'Singleton' });

container.bind(DocumentClient).toConstantValue(client);
container.bind(SchemaValidator).toSelf();
container.bind(KittenClawsRepository).toSelf();
container.bind(KittenClawsService).toSelf();
container.bind(KittenClawsController).toSelf();

export const kittenClawsController = container.get(KittenClawsController);
