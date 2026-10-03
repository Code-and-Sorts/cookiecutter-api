import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { visitController } from '@config/container';
import { jsonResponse, notAllowed, readBody, userIdFrom } from './response';

// Writes read the user id before the body so an invalid header is rejected first.
export const visitRoutes = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  if (id === undefined) {
    switch (event.httpMethod) {
      case 'POST': {
        const userId = userIdFrom(event);
        return jsonResponse(201, await visitController.post(readBody(event), userId));
      }
      default:
        return notAllowed();
    }
  }
  switch (event.httpMethod) {
    case 'GET':
      return jsonResponse(200, await visitController.get(id));
    case 'PATCH': {
      const userId = userIdFrom(event);
      return jsonResponse(200, await visitController.update(id, readBody(event), userId));
    }
    default:
      return notAllowed();
  }
};
