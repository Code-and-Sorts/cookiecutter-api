{%- if cloud_service == 'GCP Cloud Function' -%}
import * as ff from '@google-cloud/functions-framework';
import spec from '../openapi.json' with { type: 'json' };
import { jsonResponse, notAllowed, notFound } from './response';

export const openApiRoutes = async (req: ff.Request, res: ff.Response, id?: string): Promise<void> => {
  if (id !== undefined) return notFound(res);
  if (req.method !== 'GET') return notAllowed(res);
  return jsonResponse(res, 200, spec);
};
{%- else -%}
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import spec from '../openapi.json' with { type: 'json' };
import { jsonResponse, notAllowed, notFound } from './response';

export const openApiRoutes = async (event: APIGatewayProxyEvent, id?: string): Promise<APIGatewayProxyResult> => {
  if (id !== undefined) {
    return notFound();
  }
  if (event.httpMethod !== 'GET') {
    return notAllowed();
  }
  return jsonResponse(200, spec);
};
{%- endif %}
