import * as ff from '@google-cloud/functions-framework';
import { detectError, METHOD_NOT_ALLOWED_MESSAGE, NOT_FOUND_MESSAGE, parseJsonBody, parseUserId, USER_ID_HEADER } from '@utils';

export const jsonResponse = (res: ff.Response, status: number, body: unknown): void => {
  res.status(status).json(body);
};

export const notFound = (res: ff.Response): void => jsonResponse(res, 404, { errorMessage: NOT_FOUND_MESSAGE });

export const notAllowed = (res: ff.Response): void => jsonResponse(res, 405, { errorMessage: METHOD_NOT_ALLOWED_MESSAGE });

export const errorResponse = (res: ff.Response, error: unknown): void => {
  const { status, body } = detectError(error);
  jsonResponse(res, status, body);
};

// Reads rawBody whatever the Content-Type, so a form or text body is a 400, not accepted.
export const readBody = (req: ff.Request): unknown => parseJsonBody(req.rawBody?.toString('utf8'));

// Node lower-cases incoming header names.
export const userIdFrom = (req: ff.Request): string | undefined => {
  const value = req.headers[USER_ID_HEADER];
  return parseUserId(Array.isArray(value) ? value[0] : value);
};
