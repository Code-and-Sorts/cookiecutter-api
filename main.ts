import * as ff from '@google-cloud/functions-framework';
import {
  catController,
  dogController,
} from '@config/inversity.config';
import {
  Cat,
  Dog,
} from '@models';
import { detectError } from '@utils';

const jsonResponse = (res: ff.Response, status: number, body: unknown): void => {
  res.status(status).json(body);
};

const notAllowed = (res: ff.Response) => jsonResponse(res, 405, { error: 'Method not allowed.' });

ff.http('api', async (req: ff.Request, res: ff.Response) => {
  const cleanPath = req.path.replace(/\/+$/, '');
  if (cleanPath.endsWith('/health')) {
    return jsonResponse(res, 200, { status: 'ok' });
  }

  const segments = cleanPath.split('/').filter(Boolean);
  const endpoint = segments[0];
  const id = segments[1];

  try {
    switch (endpoint) {
      case 'cats': {
        switch (req.method) {
          case 'GET': {
            if (id) {
              return jsonResponse(res, 200, await catController.get(id));
            }
            return jsonResponse(res, 200, await catController.list(req.query.limit as string | undefined));
          }
          case 'POST': {
            return jsonResponse(res, 201, await catController.post(req.body as Cat));
          }
          case 'PATCH': {
            return jsonResponse(res, 200, await catController.update({ id, ...(req.body as Cat) }));
          }
          case 'PUT': {
            return notAllowed(res);
          }
          case 'DELETE': {
            if (!id) return jsonResponse(res, 400, { error: 'Missing item ID.' });
            await catController.delete(id);
            return jsonResponse(res, 200, { message: `cats with ID ${id} deleted.` });
          }
          default:
            return notAllowed(res);
        }
      }
      case 'dogs': {
        switch (req.method) {
          case 'GET': {
            if (id) {
              return jsonResponse(res, 200, await dogController.get(id));
            }
            return jsonResponse(res, 200, await dogController.list(req.query.limit as string | undefined));
          }
          case 'POST': {
            return jsonResponse(res, 201, await dogController.post(req.body as Dog));
          }
          case 'PATCH': {
            return notAllowed(res);
          }
          case 'PUT': {
            return jsonResponse(res, 200, await dogController.replace({ id, ...(req.body as Dog) }));
          }
          case 'DELETE': {
            if (!id) return jsonResponse(res, 400, { error: 'Missing item ID.' });
            await dogController.delete(id);
            return jsonResponse(res, 200, { message: `dogs with ID ${id} deleted.` });
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
