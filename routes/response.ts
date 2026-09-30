import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { detectError, METHOD_NOT_ALLOWED_MESSAGE, NOT_FOUND_MESSAGE, parseJsonBody, parseUserId, USER_ID_HEADER } from '@utils';

export const jsonResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

export const notFound = (): APIGatewayProxyResult => jsonResponse(404, { errorMessage: NOT_FOUND_MESSAGE });

export const notAllowed = (): APIGatewayProxyResult => jsonResponse(405, { errorMessage: METHOD_NOT_ALLOWED_MESSAGE });

export const errorResponse = (error: unknown): APIGatewayProxyResult => {
  const { status, body } = detectError(error);
  return jsonResponse(status, body);
};

export const readBody = (event: APIGatewayProxyEvent): unknown =>
  parseJsonBody(event.isBase64Encoded && event.body ? Buffer.from(event.body, 'base64').toString('utf8') : event.body);

// API Gateway passes header names in the client's casing.
export const userIdFrom = (event: APIGatewayProxyEvent): string | undefined =>
  parseUserId(Object.entries(event.headers ?? {}).find(([name]) => name.toLowerCase() === USER_ID_HEADER)?.[1]);
