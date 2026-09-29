import 'reflect-metadata';
import { Container } from 'inversify';

import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import {
  DocumentClient,
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


const client = DynamoDBDocumentClient.from(new DynamoDBClient({ region: env.AWS_REGION }));

export const container = new Container({ defaultScope: 'Singleton' });

container.bind(DocumentClient).toConstantValue(client);
container.bind(SchemaValidator).toSelf();
container.bind(CatRepository).toSelf();
container.bind(CatService).toSelf();
container.bind(CatController).toSelf();
container.bind(DogRepository).toSelf();
container.bind(DogService).toSelf();
container.bind(DogController).toSelf();

export const catController = container.get(CatController);
export const dogController = container.get(DogController);
