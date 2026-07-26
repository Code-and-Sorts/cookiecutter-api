import "reflect-metadata";

import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
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


const dynamoClient = new DynamoDBClient({ region: env.AWS_REGION });
const client = DynamoDBDocumentClient.from(dynamoClient);

const validator = new SchemaValidator();
export const kittenClawsController = new KittenClawsController(new KittenClawsService(new KittenClawsRepository(client, env.DYNAMODB_TABLE_NAME_KITTIES)), validator);
