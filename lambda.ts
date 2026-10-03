import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import {
  errorResponse,
  healthRoutes,
  notFound,
  catRoutes,
  dogRoutes,
} from '@routes';

// template.yaml maps only the enabled operations; API Gateway answers other paths and methods itself.
export const handler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
  const segments = (event.resource || event.path || '').split('/').filter(Boolean);
  const endpoint = segments[0];
  const id = segments.length > 1 ? event.pathParameters?.id : undefined;

  try {
    if (segments.length > 2 || (segments.length === 2 && id === undefined)) {
      return notFound();
    }
    switch (endpoint) {
      case 'health':
        return id === undefined ? await healthRoutes(event) : notFound();
      case 'cats':
        return await catRoutes(event, id);
      case 'dogs':
        return await dogRoutes(event, id);
      default:
        return notFound();
    }
  } catch (error) {
    return errorResponse(error);
  }
};
