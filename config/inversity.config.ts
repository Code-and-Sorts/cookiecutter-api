import "reflect-metadata";
import { CosmosClient } from '@azure/cosmos';
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
const database = client.database(env.COSMOS_DB_DATABASE_NAME);

const validator = new SchemaValidator();
export const kittenClawsController = new KittenClawsController(new KittenClawsService(new KittenClawsRepository(database.container(env.COSMOS_CONTAINER_KITTIES))), validator);
