{#- [method, has id, status, controller method] -#}
{%- set probes = {
    'list': ['GET', false, 200, 'list'],
    'get_by_id': ['GET', true, 200, 'get'],
    'create': ['POST', false, 201, 'post'],
    'update': ['PATCH', true, 200, 'update'],
    'replace': ['PUT', true, 200, 'replace'],
    'delete': ['DELETE', true, 200, 'delete'],
} -%}
{%- set gcp = cloud_service == 'GCP Cloud Function' -%}
{%- macro path(resource, with_id) -%}
{%- if with_id %}`/{{ resource.endpoint }}/${mockId}`{% else %}'/{{ resource.endpoint }}'{% endif -%}
{%- endmacro -%}
{%- set ns = namespace(body_resource=none, body_op=none) -%}
{%- for resource in resources -%}
{%- for op in ['create', 'update', 'replace'] -%}
{%- if ns.body_resource is none and op in resource.operations -%}
{%- set ns.body_resource = resource -%}
{%- set ns.body_op = op -%}
{%- endif -%}
{%- endfor -%}
{%- endfor -%}
import { describe, it, expect, beforeEach, beforeAll, afterEach, jest } from '@jest/globals';
{%- if not gcp %}
import { APIGatewayProxyEvent } from 'aws-lambda';
{%- endif %}
import { NotFoundError, ValidationError } from '@errors';
{%- if gcp %}
import { EventEmitter } from 'node:events';
import { currentSignal } from '@utils';
{%- endif %}
import { MockFn, mockController } from '../test/mocks';

{% if gcp -%}
jest.unstable_mockModule('@google-cloud/functions-framework', () => ({ http: jest.fn<MockFn>() }));
{% endif -%}
jest.unstable_mockModule('@config/container', () => ({
{%- for resource in resources %}
    {{ resource.name | to_lower_camel }}Controller: mockController(),
{%- endfor %}
}));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
let controllers: Record<string, Record<string, jest.Mock<MockFn>>>;

beforeAll(async () => {
    controllers = (await import('@config/container')) as unknown as Record<string, Record<string, jest.Mock<MockFn>>>;
});
{%- if gcp %}

type Handler = (req: unknown, res: unknown) => Promise<void>;
let api: Handler;
let finalHandler: (req: unknown, res: unknown) => (error?: unknown) => void;

beforeAll(async () => {
    const ff = await import('@google-cloud/functions-framework');
    const main = await import('../main');
    api = main.api as unknown as Handler;
    finalHandler = main.frameworkFinalHandler as unknown as typeof finalHandler;
    expect(ff.http).toHaveBeenCalledWith('api', main.api);
});

const mockResponse = () =>
    Object.assign(new EventEmitter(), {
        headersSent: false,
        writableEnded: false,
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    });

const send = async (method: string, path: string, body?: unknown) => {
    const res = mockResponse();
    const rawBody = body === undefined ? undefined : Buffer.from(typeof body === 'string' ? body : JSON.stringify(body));
    await api({ method, path, rawBody, query: {} }, res);
    return { status: res.status.mock.calls[0][0], body: res.json.mock.calls[0][0] };
};
{%- else %}

let handler: (event: APIGatewayProxyEvent) => Promise<{ statusCode: number; headers?: unknown; body: string }>;

beforeAll(async () => {
    ({ handler } = await import('../lambda'));
});

const send = async (method: string, path: string, body?: unknown) => {
    const segments = path.split('/').filter(Boolean);
    const event = {
        httpMethod: method,
        resource: segments.length > 1 ? `/${segments[0]}/{id}${segments.slice(2).map((s) => `/${s}`).join('')}` : path,
        path,
        pathParameters: segments.length > 1 ? { id: segments[1] } : null,
        queryStringParameters: null,
        body: body === undefined ? null : typeof body === 'string' ? body : JSON.stringify(body),
        isBase64Encoded: false,
    } as unknown as APIGatewayProxyEvent;
    const result = await handler(event);
    expect(result.headers).toEqual({ 'Content-Type': 'application/json' });
    return { status: result.statusCode, body: JSON.parse(result.body) as unknown };
};
{%- endif %}

const notFound = { status: 404, body: { errorMessage: 'Not found.' } };

describe('routing', () => {
    let consoleError: jest.SpiedFunction<typeof console.error>;

    beforeEach(() => {
        jest.clearAllMocks();
        consoleError = jest.spyOn(console, 'error').mockImplementation(() => undefined);
    });

    afterEach(() => consoleError.mockRestore());
{%- if health_endpoint %}

    it('should dispatch GET /{{ health_endpoint }} to the health check', async () => {
        expect(await send('GET', '/{{ health_endpoint }}')).toEqual({ status: 200, body: { status: 'ok' } });
    });

    it('should only match the health path exactly', async () => {
        expect(await send('GET', '/{{ health_endpoint }}/extra')).toEqual(notFound);
    });
{%- endif %}

    it('should return a JSON 404 for an unknown endpoint', async () => {
        expect(await send('GET', '/unknown')).toEqual(notFound);
        expect(await send('GET', '/')).toEqual(notFound);
    });

    it('should return a JSON 404 below an item path', async () => {
        expect(await send('GET', `/{{ resources[0].endpoint }}/${mockId}/extra`)).toEqual(notFound);
    });
{%- for resource in resources %}
{%- set p = probes[resource.operations[0]] %}

    it('should dispatch {{ p[0] }} /{{ resource.endpoint }}{% if p[1] %}/{id}{% endif %} to the {{ resource.name }} routes', async () => {
        expect((await send('{{ p[0] }}', {{ path(resource, p[1]) }}{% if p[0] in ['POST', 'PATCH', 'PUT'] %}, { name: 'mockName' }{% endif %})).status).toEqual({{ p[2] }});
        expect(controllers['{{ resource.name | to_lower_camel }}Controller'].{{ p[3] }}).toHaveBeenCalledTimes(1);
    });
{%- endfor %}
{%- set first = resources[0] %}
{%- set p = probes[first.operations[0]] %}
{%- set call = "send('" ~ p[0] ~ "', " ~ path(first, p[1]) ~ (", { name: 'mockName' }" if p[0] in ['POST', 'PATCH', 'PUT'] else "") ~ ")" %}

    it('should map route errors to JSON error responses', async () => {
        const method = controllers['{{ first.name | to_lower_camel }}Controller'].{{ p[3] }};
        method.mockRejectedValueOnce(new NotFoundError('{{ first.name }} with id x was not found.'));
        expect(await {{ call }}).toEqual({ status: 404, body: { errorMessage: '{{ first.name }} with id x was not found.' } });
        method.mockRejectedValueOnce(new ValidationError('name is required.'));
        expect(await {{ call }}).toEqual({ status: 400, body: { errorMessage: 'name is required.' } });
        expect(consoleError).not.toHaveBeenCalled();
    });

    it('should log unexpected errors and answer with a generic 500', async () => {
        const method = controllers['{{ first.name | to_lower_camel }}Controller'].{{ p[3] }};
        const error = new Error('secret details');
        method.mockRejectedValueOnce(error);
        expect(await {{ call }}).toEqual({ status: 500, body: { errorMessage: 'An unexpected error occurred.' } });
        expect(consoleError).toHaveBeenCalledWith(expect.any(String), error);
    });
{%- if ns.body_resource is not none %}
{%- set bp = probes[ns.body_op] %}

    it('should answer a malformed JSON body with a JSON 400', async () => {
        expect(await send('{{ bp[0] }}', {{ path(ns.body_resource, bp[1]) }}, '{bad')).toEqual({
            status: 400,
            body: { errorMessage: 'Request body must be valid JSON.' },
        });
        expect(controllers['{{ ns.body_resource.name | to_lower_camel }}Controller'].{{ bp[3] }}).not.toHaveBeenCalled();
    });
{%- endif %}
});
{%- if gcp %}

describe('client disconnects', () => {
    it('should abort the request signal when the response closes before it is sent', async () => {
        const method = controllers['{{ first.name | to_lower_camel }}Controller'].{{ p[3] }};
        let signal: AbortSignal | undefined;
        let release: () => void = () => undefined;
        method.mockImplementationOnce(() => {
            signal = currentSignal();
            return new Promise((resolve) => {
                release = () => resolve({});
            });
        });
        const res = mockResponse();
        const rawBody = Buffer.from(JSON.stringify({ name: 'mockName' }));
        const pending = api({ method: '{{ p[0] }}', path: {{ path(first, p[1]) }}, rawBody, query: {} }, res);
        await new Promise((resolve) => setImmediate(resolve));
        expect(signal?.aborted).toBe(false);
        res.emit('close');
        expect(signal?.aborted).toBe(true);
        release();
        await pending;
        expect(res.listenerCount('close')).toEqual(0);
    });

    it('should not abort once the response has been sent', async () => {
        const method = controllers['{{ first.name | to_lower_camel }}Controller'].{{ p[3] }};
        let signal: AbortSignal | undefined;
        method.mockImplementationOnce(async () => {
            signal = currentSignal();
            return {};
        });
        const res = mockResponse();
        const rawBody = Buffer.from(JSON.stringify({ name: 'mockName' }));
        const pending = api({ method: '{{ p[0] }}', path: {{ path(first, p[1]) }}, rawBody, query: {} }, res);
        await pending;
        res.writableEnded = true;
        res.emit('close');
        expect(signal?.aborted).toBe(false);
    });
});

describe('frameworkFinalHandler', () => {
    const run = (error?: unknown, headersSent = false) => {
        const res = { ...mockResponse(), headersSent };
        finalHandler({}, res)(error);
        return res;
    };

    it('should answer a request no route handled with a JSON 404', () => {
        const res = run();
        expect(res.status).toHaveBeenCalledWith(404);
        expect(res.json).toHaveBeenCalledWith({ errorMessage: 'Not found.' });
    });

    it('should answer a body-parser failure with a JSON 400', () => {
        const res = run(Object.assign(new SyntaxError('Unexpected token'), { status: 400, type: 'entity.parse.failed' }));
        expect(res.status).toHaveBeenCalledWith(400);
        expect(res.json).toHaveBeenCalledWith({ errorMessage: 'Request body must be valid JSON.' });
    });

    it('should log other errors and answer with a generic 500', () => {
        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => undefined);
        const res = run(new Error('boom'));
        expect(res.status).toHaveBeenCalledWith(500);
        expect(res.json).toHaveBeenCalledWith({ errorMessage: 'An unexpected error occurred.' });
        expect(consoleError).toHaveBeenCalledTimes(1);
        consoleError.mockRestore();
    });

    it('should leave a response that was already sent alone', () => {
        const res = run(new Error('late'), true);
        expect(res.status).not.toHaveBeenCalled();
    });
});
{%- endif %}
