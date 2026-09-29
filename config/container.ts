import 'reflect-metadata';
import { Container } from 'inversify';
import { CosmosClient, Database } from '@azure/cosmos';
import {
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

const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
});

export const container = new Container({ defaultScope: 'Singleton' });
container.bind(Database).toConstantValue(client.database(env.COSMOS_DB_DATABASE_NAME));
container.bind(SchemaValidator).toSelf();
container.bind(CatRepository).toSelf();
container.bind(CatService).toSelf();
container.bind(CatController).toSelf();
container.bind(DogRepository).toSelf();
container.bind(DogService).toSelf();
container.bind(DogController).toSelf();

export const catController = container.get(CatController);
export const dogController = container.get(DogController);
