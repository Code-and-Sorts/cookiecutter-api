import "reflect-metadata";
import { CosmosClient } from '@azure/cosmos';
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
const database = client.database(env.COSMOS_DB_DATABASE_NAME);

const validator = new SchemaValidator();
export const catController = new CatController(new CatService(new CatRepository(database.container(env.COSMOS_CONTAINER_ANIMALS))), validator);
export const dogController = new DogController(new DogService(new DogRepository(database.container(env.COSMOS_CONTAINER_ANIMALS))), validator);
