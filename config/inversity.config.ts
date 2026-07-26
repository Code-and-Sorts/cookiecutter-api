import "reflect-metadata";

import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
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


const dynamoClient = new DynamoDBClient({ region: env.AWS_REGION });
const client = DynamoDBDocumentClient.from(dynamoClient);

const validator = new SchemaValidator();
export const catController = new CatController(new CatService(new CatRepository(client, env.DYNAMODB_TABLE_NAME_ANIMALS)), validator);
export const dogController = new DogController(new DogService(new DogRepository(client, env.DYNAMODB_TABLE_NAME_ANIMALS)), validator);
