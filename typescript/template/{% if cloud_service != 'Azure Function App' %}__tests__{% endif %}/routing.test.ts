{#- For each operation: HTTP method, whether the path carries an id, success status, controller method. -#}
{%- set probes = {
    'list': ['GET', false, 200, 'list'],
    'get_by_id': ['GET', true, 200, 'get'],
    'create': ['POST', false, 201, 'post'],
    'update': ['PATCH', true, 200, 'update'],
    'replace': ['PUT', true, 200, 'replace'],
    'delete': ['DELETE', true, 200, 'delete'],
} -%}
{%- macro path(resource, with_id) -%}
{%- if with_id %}`/{{ resource.endpoint }}/${mockId}`{% else %}'/{{ resource.endpoint }}'{% endif -%}
{%- endmacro -%}
import { describe, it, expect, beforeEach, beforeAll, jest } from '@jest/globals';
{%- if cloud_service == 'AWS Lambda' %}
import { APIGatewayProxyEvent } from 'aws-lambda';
{%- endif %}
import { NotFoundError } from '@errors';

type MockFn = (...args: any[]) => any;

{% if cloud_service == 'GCP Cloud Function' -%}
jest.unstable_mockModule('@google-cloud/functions-framework', () => ({ http: jest.fn<MockFn>() }));
{% endif -%}
jest.unstable_mockModule('@config/container', () => {
    const mockController = () => ({
        list: jest.fn<MockFn>().mockResolvedValue([]),
        get: jest.fn<MockFn>().mockResolvedValue({}),
        post: jest.fn<MockFn>().mockResolvedValue({}),
        update: jest.fn<MockFn>().mockResolvedValue({}),
        replace: jest.fn<MockFn>().mockResolvedValue({}),
        delete: jest.fn<MockFn>().mockResolvedValue(undefined),
    });
    return {
{%- for resource in resources %}
        {{ resource.name | to_lower_camel }}Controller: mockController(),
{%- endfor %}
    };
});

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
let controllers: Record<string, Record<string, jest.Mock<MockFn>>>;

beforeAll(async () => {
    controllers = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>;
});
{%- if cloud_service == 'GCP Cloud Function' %}

let api: (req: unknown, res: unknown) => Promise<void>;

beforeAll(async () => {
    const ff = await import('@google-cloud/functions-framework');
    await import('../main');
    api = (ff.http as jest.Mock<MockFn>).mock.calls[0][1];
});

const send = async (method: string, path: string, body?: unknown) => {
    const res = {
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
        send: jest.fn<MockFn>().mockReturnThis(),
    };
    await api({ method, path, body, query: {} }, res);
    return res.status.mock.calls[0][0];
};
{%- else %}

let handler: (event: APIGatewayProxyEvent) => Promise<{ statusCode: number }>;

beforeAll(async () => {
    ({ handler } = await import('../lambda'));
});

const send = async (method: string, path: string, body?: unknown) => {
    const [endpoint, id] = path.split('/').filter(Boolean);
    const event = {
        httpMethod: method,
        resource: id ? `/${endpoint}/{id}` : `/${endpoint}`,
        path,
        pathParameters: id ? { id } : null,
        queryStringParameters: null,
        body: body === undefined ? null : JSON.stringify(body),
    } as unknown as APIGatewayProxyEvent;
    return (await handler(event)).statusCode;
};
{%- endif %}

describe('routing', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should answer the health check', async () => {
        expect(await send('GET', '/health')).toEqual(200);
    });

    it('should return 404 for an unknown endpoint', async () => {
        expect(await send('GET', '/unknown')).toEqual(404);
    });
{%- for resource in resources %}
{%- set p = probes[resource.operations[0]] %}

    it('should dispatch {{ p[0] }} /{{ resource.endpoint }}{% if p[1] %}/{id}{% endif %} to the {{ resource.name }} routes', async () => {
        expect(await send('{{ p[0] }}', {{ path(resource, p[1]) }}{% if p[0] in ['POST', 'PATCH', 'PUT'] %}, { name: 'mockName' }{% endif %})).toEqual({{ p[2] }});
        expect(controllers['{{ resource.name | to_lower_camel }}Controller'].{{ p[3] }}).toHaveBeenCalledTimes(1);
    });
{%- endfor %}
{%- set first = resources[0] %}
{%- set p = probes[first.operations[0]] %}

    it('should map route errors to responses', async () => {
        const method = controllers['{{ first.name | to_lower_camel }}Controller'].{{ p[3] }};
        method.mockRejectedValueOnce(new NotFoundError('missing'));
        expect(await send('{{ p[0] }}', {{ path(first, p[1]) }}{% if p[0] in ['POST', 'PATCH', 'PUT'] %}, { name: 'mockName' }{% endif %})).toEqual(404);
        method.mockRejectedValueOnce(new Error('boom'));
        expect(await send('{{ p[0] }}', {{ path(first, p[1]) }}{% if p[0] in ['POST', 'PATCH', 'PUT'] %}, { name: 'mockName' }{% endif %})).toEqual(500);
    });
});
