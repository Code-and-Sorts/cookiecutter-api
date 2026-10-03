import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { catController } from '@config/container';
import { jsonResponse, notAllowed, readBody, userIdFrom } from './response';

// Writes read the user id before the body so an invalid header is rejected first.
export const catRoutes = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  if (id === undefined) {
    switch (event.httpMethod) {
      case 'GET':
        return jsonResponse(200, await catController.list(event.queryStringParameters?.limit));
      case 'POST': {
        const userId = userIdFrom(event);
        return jsonResponse(201, await catController.post(readBody(event), userId));
      }
      default:
        return notAllowed();
    }
  }
  switch (event.httpMethod) {
    case 'GET':
      return jsonResponse(200, await catController.get(id));
    case 'PATCH': {
      const userId = userIdFrom(event);
      return jsonResponse(200, await catController.update(id, readBody(event), userId));
    }
    case 'DELETE':
      return jsonResponse(200, await catController.delete(id, userIdFrom(event)));
    default:
      return notAllowed();
  }
};
