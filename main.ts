import * as ff from '@google-cloud/functions-framework';
import {
  kittenClawsController,
} from '@config/container';
import {
  KittenClaws,
} from '@models';
import { detectError } from '@utils';

const jsonResponse = (res: ff.Response, status: number, body: unknown): void => {
  res.status(status).json(body);
};

const notAllowed = (res: ff.Response) => jsonResponse(res, 405, { error: 'Method not allowed.' });

ff.http('api', async (req: ff.Request, res: ff.Response) => {
  const cleanPath = req.path.replace(/\/+$/, '');
  const segments = cleanPath.split('/').filter(Boolean);
  const endpoint = segments[0];
  const id = segments[1];

  if (endpoint === 'health' && !id) {
    return jsonResponse(res, 200, { status: 'ok' });
  }

  try {
    switch (endpoint) {
      case 'kitties': {
        switch (req.method) {
          case 'GET': {
            if (id) {
              return jsonResponse(res, 200, await kittenClawsController.get(id));
            }
            return jsonResponse(res, 200, await kittenClawsController.list(req.query.limit as string | undefined));
          }
          case 'POST': {
            return jsonResponse(res, 201, await kittenClawsController.post(req.body as KittenClaws));
          }
          case 'PATCH': {
            return jsonResponse(res, 200, await kittenClawsController.update({ ...(req.body as KittenClaws), id }));
          }
          case 'PUT': {
            return notAllowed(res);
          }
          case 'DELETE': {
            if (!id) return jsonResponse(res, 400, { error: 'Missing item ID.' });
            await kittenClawsController.delete(id);
            return jsonResponse(res, 200, { message: `kitties with ID ${id} deleted.` });
          }
          default:
            return notAllowed(res);
        }
      }
      default:
        return jsonResponse(res, 404, { error: 'Not found.' });
    }
  } catch (error) {
    const errorResponse = detectError(error);
    return res.status(errorResponse.status).send(errorResponse.body);
  }
});
