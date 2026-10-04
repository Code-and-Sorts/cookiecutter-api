import { createRequire } from 'node:module';
import * as ff from '@google-cloud/functions-framework';
import {
  errorResponse,
  healthRoutes,
  jsonResponse,
  notFound,
  catRoutes,
  dogRoutes,
} from '@routes';
import { INVALID_JSON_MESSAGE, runWithSignal } from '@utils';

type Next = (error?: unknown) => void;
type ExpressApplication = { handle(req: ff.Request, res: ff.Response, next?: Next): void };

// Express parses bodies before the function runs and answers parse failures with an HTML page;
// this final handler answers them in JSON instead.
export const frameworkFinalHandler = (req: ff.Request, res: ff.Response): Next => (error?: unknown) => {
  if (res.headersSent) {
    return;
  }
  if (error === undefined) {
    return notFound(res);
  }
  const status = (error as { status?: number }).status;
  if (status !== undefined && status >= 400 && status < 500) {
    return jsonResponse(res, 400, { errorMessage: INVALID_JSON_MESSAGE });
  }
  return errorResponse(res, error);
};

// Patch the framework's own Express copy; the framework builds its app after loading this module.
const frameworkRequire = createRequire(createRequire(import.meta.url).resolve('@google-cloud/functions-framework'));
const express = frameworkRequire('express') as { application: ExpressApplication };
const handle = express.application.handle;
express.application.handle = function (this: ExpressApplication, req, res, next) {
  return handle.call(this, req, res, next ?? frameworkFinalHandler(req, res));
};

const route = async (req: ff.Request, res: ff.Response): Promise<void> => {
  const segments = req.path.split('/').filter(Boolean);
  const [endpoint, id] = segments;

  try {
    if (segments.length > 2) {
      return notFound(res);
    }
    switch (endpoint) {
      case 'health':
        return id === undefined ? await healthRoutes(req, res) : notFound(res);
      case 'cats':
        return await catRoutes(req, res, id);
      case 'dogs':
        return await dogRoutes(req, res, id);
      default:
        return notFound(res);
    }
  } catch (error) {
    return errorResponse(res, error);
  }
};

// A client that disconnects early aborts the signal, cancelling pending database work.
export const api = async (req: ff.Request, res: ff.Response): Promise<void> => {
  const controller = new AbortController();
  const onClose = () => {
    if (!res.writableEnded) {
      controller.abort();
    }
  };
  res.on('close', onClose);
  try {
    await runWithSignal(controller.signal, () => route(req, res));
  } finally {
    res.off('close', onClose);
  }
};

ff.http('api', api);
