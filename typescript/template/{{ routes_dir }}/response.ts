{%- if cloud_service == 'GCP Cloud Function' -%}
import * as ff from '@google-cloud/functions-framework';
import { detectError, parseJsonBody } from '@utils';

// Every response is JSON, errors included.
export const jsonResponse = (res: ff.Response, status: number, body: unknown): void => {
  res.status(status).json(body);
};

export const notFound = (res: ff.Response): void => jsonResponse(res, 404, { errorMessage: 'Not found.' });

export const notAllowed = (res: ff.Response): void => jsonResponse(res, 405, { errorMessage: 'Method not allowed.' });

export const errorResponse = (res: ff.Response, error: unknown): void => {
  const { status, body } = detectError(error);
  jsonResponse(res, status, body);
};

// Parses the raw request body as JSON whatever its Content-Type, so a form or text body is
// rejected (400) rather than accepted in a parsed form.
export const readBody = (req: ff.Request): unknown => parseJsonBody(req.rawBody?.toString('utf8'));
{%- else -%}
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { detectError, parseJsonBody } from '@utils';

// Every response is JSON, errors included.
export const jsonResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

export const notFound = (): APIGatewayProxyResult => jsonResponse(404, { errorMessage: 'Not found.' });

export const notAllowed = (): APIGatewayProxyResult => jsonResponse(405, { errorMessage: 'Method not allowed.' });

export const errorResponse = (error: unknown): APIGatewayProxyResult => {
  const { status, body } = detectError(error);
  return jsonResponse(status, body);
};

// Parses the request body as JSON; a missing or malformed body is a 400.
export const readBody = (event: APIGatewayProxyEvent): unknown =>
  parseJsonBody(event.isBase64Encoded && event.body ? Buffer.from(event.body, 'base64').toString('utf8') : event.body);
{%- endif %}
