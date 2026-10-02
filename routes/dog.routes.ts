import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { dogController } from '@config/container';
import { jsonResponse, notAllowed, readBody, userIdFrom } from './response';

// Writes read the user id before the body so an invalid header is rejected first.
export const dogRoutes = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  if (id === undefined) {
    switch (event.httpMethod) {
      case 'GET':
        return jsonResponse(200, await dogController.list(event.queryStringParameters?.limit));
      case 'POST': {
        const userId = userIdFrom(event);
        return jsonResponse(201, await dogController.post(readBody(event), userId));
      }
      default:
        return notAllowed();
    }
  }
  switch (event.httpMethod) {
    case 'GET':
      return jsonResponse(200, await dogController.get(id));
    case 'PUT': {
      const userId = userIdFrom(event);
      return jsonResponse(200, await dogController.replace(id, readBody(event), userId));
    }
    case 'DELETE':
      return jsonResponse(200, await dogController.delete(id, userIdFrom(event)));
    default:
      return notAllowed();
  }
};
