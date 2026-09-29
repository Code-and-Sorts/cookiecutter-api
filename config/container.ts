import 'reflect-metadata';
import { Container } from 'inversify';
import { CosmosClient, Database } from '@azure/cosmos';
import {
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

const client = new CosmosClient({
  endpoint: env.COSMOS_DB_URL,
  key: env.COSMOS_DB_KEY,
});

export const container = new Container({ defaultScope: 'Singleton' });
container.bind(Database).toConstantValue(client.database(env.COSMOS_DB_DATABASE_NAME));
container.bind(SchemaValidator).toSelf();
container.bind(KittenClawsRepository).toSelf();
container.bind(KittenClawsService).toSelf();
container.bind(KittenClawsController).toSelf();

export const kittenClawsController = container.get(KittenClawsController);
