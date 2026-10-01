{%- if cloud_service == 'GCP Cloud Function' -%}
import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import spec from '../../openapi.json' with { type: 'json' };
import { MockFn } from '../../test/mocks';

let routes: (req: unknown, res: unknown, id?: string) => Promise<void>;

beforeAll(async () => {
    ({ openApiRoutes: routes } = (await import('../openapi.routes')) as unknown as { openApiRoutes: typeof routes });
});

const send = async (method: string, id?: string) => {
    const res = {
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    };
    await routes({ method, query: {} }, res, id);
    return { status: res.status.mock.calls[0][0], body: res.json.mock.calls[0][0] };
};
{%- else -%}
import { describe, it, expect, beforeAll } from '@jest/globals';
import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import spec from '../../openapi.json' with { type: 'json' };

let routes: (event: APIGatewayProxyEvent, id?: string) => Promise<APIGatewayProxyResult>;

beforeAll(async () => {
    ({ openApiRoutes: routes } = await import('../openapi.routes'));
});

const send = async (method: string, id?: string) => {
    const event = { httpMethod: method, queryStringParameters: null, body: null } as unknown as APIGatewayProxyEvent;
    const result = await routes(event, id);
    return { status: result.statusCode, body: JSON.parse(result.body) as unknown };
};
{%- endif %}

describe('openapi routes', () => {
    it('should answer GET /openapi.json with the spec', async () => {
        expect(await send('GET')).toEqual({ status: 200, body: spec });
    });

    it('should return 405 for other methods', async () => {
        expect(await send('POST')).toEqual({ status: 405, body: { errorMessage: 'Method not allowed.' } });
    });

    it('should return 404 below the spec path', async () => {
        expect(await send('GET', 'extra')).toEqual({ status: 404, body: { errorMessage: 'Not found.' } });
    });
});
