import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { kittenClawsController } from '@config/container';
import { jsonResponse, notAllowed, readBody, userIdFrom } from './response';

// Writes read the user id before the body so an invalid header is rejected first.
export const kittenClawsRoutes = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  if (id === undefined) {
    switch (event.httpMethod) {
      case 'GET':
        return jsonResponse(200, await kittenClawsController.list(event.queryStringParameters?.limit));
      case 'POST': {
        const userId = userIdFrom(event);
        return jsonResponse(201, await kittenClawsController.post(readBody(event), userId));
      }
      default:
        return notAllowed();
    }
  }
  switch (event.httpMethod) {
    case 'GET':
      return jsonResponse(200, await kittenClawsController.get(id));
    case 'PATCH': {
      const userId = userIdFrom(event);
      return jsonResponse(200, await kittenClawsController.update(id, readBody(event), userId));
    }
    case 'DELETE':
      return jsonResponse(200, await kittenClawsController.delete(id, userIdFrom(event)));
    default:
      return notAllowed();
  }
};
