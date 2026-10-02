import * as ff from '@google-cloud/functions-framework';
import { jsonResponse, notAllowed, notFound } from './response';

export const healthRoutes = async (req: ff.Request, res: ff.Response, id?: string): Promise<void> => {
  if (id !== undefined) return notFound(res);
  if (req.method !== 'GET') return notAllowed(res);
  return jsonResponse(res, 200, { status: 'ok' });
};
