{%- if cloud_service == 'GCP Cloud Function' -%}
import * as ff from '@google-cloud/functions-framework';
import { detectError, METHOD_NOT_ALLOWED_MESSAGE, NOT_FOUND_MESSAGE, parseJsonBody } from '@utils';

export const jsonResponse = (res: ff.Response, status: number, body: unknown): void => {
  res.status(status).json(body);
};

export const notFound = (res: ff.Response): void => jsonResponse(res, 404, { errorMessage: NOT_FOUND_MESSAGE });

export const notAllowed = (res: ff.Response): void => jsonResponse(res, 405, { errorMessage: METHOD_NOT_ALLOWED_MESSAGE });

export const errorResponse = (res: ff.Response, error: unknown): void => {
  const { status, body } = detectError(error);
  jsonResponse(res, status, body);
};

// Reads rawBody whatever the Content-Type, so a form or text body is a 400, not accepted.
export const readBody = (req: ff.Request): unknown => parseJsonBody(req.rawBody?.toString('utf8'));
{%- else -%}
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { detectError, METHOD_NOT_ALLOWED_MESSAGE, NOT_FOUND_MESSAGE, parseJsonBody } from '@utils';

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
{%- endif %}
