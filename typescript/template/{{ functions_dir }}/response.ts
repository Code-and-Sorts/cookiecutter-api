import { HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { detectError, parseUserId, USER_ID_HEADER } from '@utils';

export const jsonResponse = (status: number, body: unknown): HttpResponseInit => ({
  status,
  body: JSON.stringify(body),
  headers: { 'Content-Type': 'application/json' },
});

export const errorResponse = (error: unknown, context: InvocationContext): HttpResponseInit => {
  const { status, body } = detectError(error, (...args: unknown[]) => context.error(...args));
  return jsonResponse(status, body);
};

export const handle =
  (status: number, fn: (request: HttpRequest) => Promise<unknown>) =>
  async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
    try {
      return jsonResponse(status, await fn(request));
    } catch (error) {
      return errorResponse(error, context);
    }
  };

export const userIdFrom = (request: HttpRequest): string | undefined => parseUserId(request.headers.get(USER_ID_HEADER));
