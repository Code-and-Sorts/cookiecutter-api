import * as ff from '@google-cloud/functions-framework';
import { visitController } from '@config/container';
import { jsonResponse, notAllowed, readBody, userIdFrom } from './response';

// Writes read the user id before the body so an invalid header is rejected first.
export const visitRoutes = async (req: ff.Request, res: ff.Response, id?: string): Promise<void> => {
  if (id === undefined) {
    switch (req.method) {
      case 'POST': {
        const userId = userIdFrom(req);
        return jsonResponse(res, 201, await visitController.post(readBody(req), userId));
      }
      default:
        return notAllowed(res);
    }
  }
  switch (req.method) {
    case 'GET':
      return jsonResponse(res, 200, await visitController.get(id));
    case 'PATCH': {
      const userId = userIdFrom(req);
      return jsonResponse(res, 200, await visitController.update(id, readBody(req), userId));
    }
    default:
      return notAllowed(res);
  }
};
