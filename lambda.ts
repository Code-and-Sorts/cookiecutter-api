import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import {
  errorResponse,
  healthRoutes,
  notFound,
  kittenClawsRoutes,
} from '@routes';

type Route = (event: APIGatewayProxyEvent, id?: string) => Promise<APIGatewayProxyResult>;

export const routes = new Map<string, Route>([
  ['health', healthRoutes],
  ['kittenclaws', kittenClawsRoutes],
]);

// template.yaml maps only the enabled operations; API Gateway answers other paths and methods itself.
export const handler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
  const segments = (event.resource || event.path || '').split('/').filter(Boolean);
  const id = segments.length > 1 ? event.pathParameters?.id : undefined;
  const route = routes.get(segments[0]);

  try {
    if (route === undefined || segments.length > 2 || (segments.length === 2 && id === undefined)) {
      return notFound();
    }
    return await route(event, id);
  } catch (error) {
    return errorResponse(error);
  }
};
