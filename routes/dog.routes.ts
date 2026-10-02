import * as ff from '@google-cloud/functions-framework';
import { dogController } from '@config/container';
import { jsonResponse, notAllowed, readBody, userIdFrom } from './response';

// Writes read the user id before the body so an invalid header is rejected first.
export const dogRoutes = async (req: ff.Request, res: ff.Response, id?: string): Promise<void> => {
  if (id === undefined) {
    switch (req.method) {
      case 'GET':
        return jsonResponse(res, 200, await dogController.list(req.query.limit as string | undefined));
      case 'POST': {
        const userId = userIdFrom(req);
        return jsonResponse(res, 201, await dogController.post(readBody(req), userId));
      }
      default:
        return notAllowed(res);
    }
  }
  switch (req.method) {
    case 'GET':
      return jsonResponse(res, 200, await dogController.get(id));
    case 'PUT': {
      const userId = userIdFrom(req);
      return jsonResponse(res, 200, await dogController.replace(id, readBody(req), userId));
    }
    case 'DELETE':
      return jsonResponse(res, 200, await dogController.delete(id, userIdFrom(req)));
    default:
      return notAllowed(res);
  }
};
