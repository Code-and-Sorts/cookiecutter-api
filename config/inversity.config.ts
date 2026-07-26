import "reflect-metadata";

import { Firestore } from '@google-cloud/firestore';
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


const client = new Firestore({
  projectId: env.GCP_PROJECT_ID,
  databaseId: env.FIRESTORE_DATABASE,
});

const validator = new SchemaValidator();
export const catController = new CatController(new CatService(new CatRepository(client.collection(env.FIRESTORE_COLLECTION_ANIMALS))), validator);
export const dogController = new DogController(new DogService(new DogRepository(client.collection(env.FIRESTORE_COLLECTION_ANIMALS))), validator);
