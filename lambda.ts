import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import {
  kittenClawsController,
} from '@config/container';
import {
  KittenClaws,
} from '@models';
import { detectError } from '@utils';

const jsonResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

const notAllowed = () => jsonResponse(405, { error: 'Method not allowed.' });

const handleKittenClaws = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  switch (event.httpMethod) {
    case 'GET': {
      if (id) {
        return jsonResponse(200, await kittenClawsController.get(id));
      }
      return jsonResponse(200, await kittenClawsController.list(event.queryStringParameters?.limit ?? undefined));
    }
    case 'POST': {
      const item: KittenClaws = JSON.parse(event.body || '{}');
      return jsonResponse(201, await kittenClawsController.post(item));
    }
    case 'PATCH': {
      const item: KittenClaws = JSON.parse(event.body || '{}');
      return jsonResponse(200, await kittenClawsController.update({ ...item, id }));
    }
    case 'PUT': {
      return notAllowed();
    }
    case 'DELETE': {
      if (!id) {
        return jsonResponse(400, { error: 'Missing item ID.' });
      }
      await kittenClawsController.delete(id);
      return jsonResponse(200, { message: `kitties with ID ${id} deleted.` });
    }
    default:
      return notAllowed();
  }
};

export const handler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
  const routePath = (event.resource || event.path || '').replace(/\/+$/, '');
  const segments = routePath.split('/').filter(Boolean);
  const endpoint = segments[0];
  const id = event.pathParameters?.id;

  if (event.httpMethod === 'GET' && endpoint === 'health' && segments.length === 1) {
    return jsonResponse(200, { status: 'ok' });
  }

  try {
    switch (endpoint) {
      case 'kitties':
        return await handleKittenClaws(event, id);
      default:
        return jsonResponse(404, { error: 'Not found.' });
    }
  } catch (error) {
    const errorResponse = detectError(error);
    return jsonResponse(errorResponse.status, errorResponse.body);
  }
};
