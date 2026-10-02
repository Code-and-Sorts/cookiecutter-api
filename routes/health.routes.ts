import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { jsonResponse, notAllowed, notFound } from './response';

export const healthRoutes = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  if (id !== undefined) {
    return notFound();
  }
  if (event.httpMethod !== 'GET') {
    return notAllowed();
  }
  return jsonResponse(200, { status: 'ok' });
};
