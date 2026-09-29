import 'reflect-metadata';
import { Container } from 'inversify';

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

export const container = new Container({ defaultScope: 'Singleton' });

container.bind(Firestore).toConstantValue(client);
container.bind(SchemaValidator).toSelf();
container.bind(KittenClawsRepository).toSelf();
container.bind(KittenClawsService).toSelf();
container.bind(KittenClawsController).toSelf();

export const kittenClawsController = container.get(KittenClawsController);
