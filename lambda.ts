import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import {
  catController,
  dogController,
} from '@config/inversity.config';
import {
  Cat,
  Dog,
} from '@models';
import { detectError } from '@utils';

const jsonResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

const notAllowed = () => jsonResponse(405, { error: 'Method not allowed.' });

const handleCat = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  switch (event.httpMethod) {
    case 'GET': {
      if (id) {
        return jsonResponse(200, await catController.get(id));
      }
      return jsonResponse(200, await catController.list(event.queryStringParameters?.limit ?? undefined));
    }
    case 'POST': {
      const item: Cat = JSON.parse(event.body || '{}');
      return jsonResponse(201, await catController.post(item));
    }
    case 'PATCH': {
      const item: Cat = JSON.parse(event.body || '{}');
      return jsonResponse(200, await catController.update({ id, ...item }));
    }
    case 'PUT': {
      return notAllowed();
    }
    case 'DELETE': {
      if (!id) {
        return jsonResponse(400, { error: 'Missing item ID.' });
      }
      await catController.delete(id);
      return jsonResponse(200, { message: `cats with ID ${id} deleted.` });
    }
    default:
      return notAllowed();
  }
};

const handleDog = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  switch (event.httpMethod) {
    case 'GET': {
      if (id) {
        return jsonResponse(200, await dogController.get(id));
      }
      return jsonResponse(200, await dogController.list(event.queryStringParameters?.limit ?? undefined));
    }
    case 'POST': {
      const item: Dog = JSON.parse(event.body || '{}');
      return jsonResponse(201, await dogController.post(item));
    }
    case 'PATCH': {
      return notAllowed();
    }
    case 'PUT': {
      const item: Dog = JSON.parse(event.body || '{}');
      return jsonResponse(200, await dogController.replace({ id, ...item }));
    }
    case 'DELETE': {
      if (!id) {
        return jsonResponse(400, { error: 'Missing item ID.' });
      }
      await dogController.delete(id);
      return jsonResponse(200, { message: `dogs with ID ${id} deleted.` });
    }
    default:
      return notAllowed();
  }
};

export const handler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
  const routePath = (event.resource || event.path || '').replace(/\/+$/, '');
  if (event.httpMethod === 'GET' && routePath.endsWith('/health')) {
    return jsonResponse(200, { status: 'ok' });
  }

  const endpoint = routePath.split('/').filter(Boolean)[0];
  const id = event.pathParameters?.id;

  try {
    switch (endpoint) {
      case 'cats':
        return await handleCat(event, id);
      case 'dogs':
        return await handleDog(event, id);
      default:
        return jsonResponse(404, { error: 'Not found.' });
    }
  } catch (error) {
    const errorResponse = detectError(error);
    return jsonResponse(errorResponse.status, errorResponse.body);
  }
};
