import 'reflect-metadata';
import { Container } from 'inversify';

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

export const container = new Container({ defaultScope: 'Singleton' });

container.bind(Firestore).toConstantValue(client);
container.bind(SchemaValidator).toSelf();
container.bind(CatRepository).toSelf();
container.bind(CatService).toSelf();
container.bind(CatController).toSelf();
container.bind(DogRepository).toSelf();
container.bind(DogService).toSelf();
container.bind(DogController).toSelf();

export const catController = container.get(CatController);
export const dogController = container.get(DogController);
