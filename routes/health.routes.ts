import * as ff from '@google-cloud/functions-framework';
import { jsonResponse, notAllowed } from './response';

export const healthRoutes = async (req: ff.Request, res: ff.Response): Promise<void> => {
  if (req.method !== 'GET') return notAllowed(res);
  return jsonResponse(res, 200, { status: 'ok' });
};
