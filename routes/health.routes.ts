import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { jsonResponse, notAllowed } from './response';

export const healthRoutes = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
  if (event.httpMethod !== 'GET') {
    return notAllowed();
  }
  return jsonResponse(200, { status: 'ok' });
};
