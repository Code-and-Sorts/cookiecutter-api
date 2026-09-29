{%- if cloud_service == 'GCP Cloud Function' -%}
import * as ff from '@google-cloud/functions-framework';

export const jsonResponse = (res: ff.Response, status: number, body: unknown): void => {
  res.status(status).json(body);
};

export const notAllowed = (res: ff.Response): void => jsonResponse(res, 405, { error: 'Method not allowed.' });
{%- else -%}
import { APIGatewayProxyResult } from 'aws-lambda';

export const jsonResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

export const notAllowed = (): APIGatewayProxyResult => jsonResponse(405, { error: 'Method not allowed.' });
{%- endif %}
