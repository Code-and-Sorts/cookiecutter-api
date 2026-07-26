import "reflect-metadata";

import { Firestore } from '@google-cloud/firestore';
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


const client = new Firestore({
  projectId: env.GCP_PROJECT_ID,
  databaseId: env.FIRESTORE_DATABASE,
});

const validator = new SchemaValidator();
export const kittenClawsController = new KittenClawsController(new KittenClawsService(new KittenClawsRepository(client.collection(env.FIRESTORE_COLLECTION_KITTIES))), validator);
