import { describe, it, expect, beforeAll } from '@jest/globals';
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';

let routes: (event: APIGatewayProxyEvent) => Promise<APIGatewayProxyResult>;

beforeAll(async () => {
    ({ healthRoutes: routes } = await import('../health.routes'));
});

const send = async (method: string) => {
    const event = { httpMethod: method, queryStringParameters: null, body: null } as unknown as APIGatewayProxyEvent;
    const result = await routes(event);
    return { status: result.statusCode, body: JSON.parse(result.body) as unknown };
};

describe('health routes', () => {
    it('should answer GET /health with ok', async () => {
        expect(await send('GET')).toEqual({ status: 200, body: { status: 'ok' } });
    });

    it('should return 405 for other methods', async () => {
        expect(await send('POST')).toEqual({ status: 405, body: { errorMessage: 'Method not allowed.' } });
    });
});
