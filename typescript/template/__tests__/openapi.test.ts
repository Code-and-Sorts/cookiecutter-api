{%- set gcp = cloud_service == 'GCP Cloud Function' -%}
{%- set request_ops = [('create', 'CreateRequest', 'RequestSchema'), ('update', 'UpdateRequest', 'UpdateSchema'), ('replace', 'ReplaceRequest', 'RequestSchema')] -%}
{%- set ns = namespace(requests=[]) -%}
{%- for resource in resources -%}
{%- for op, suffix, schema in request_ops if op in resource.operations -%}
{%- set ns.requests = ns.requests + [(resource.name ~ suffix, resource.name ~ schema)] -%}
{%- endfor -%}
{%- endfor -%}
import { describe, it, expect, beforeAll, jest } from '@jest/globals';
{%- if cloud_service == 'Azure Function App' %}
import { readdirSync } from 'node:fs';
{%- elif not gcp %}
import { APIGatewayProxyEvent } from 'aws-lambda';
{%- else %}
import { EventEmitter } from 'node:events';
{%- endif %}
{%- if ns.requests %}
import { z } from 'zod';
import { {{ ns.requests | map(attribute=1) | unique | join(', ') }} } from '@models';
{%- endif %}
import spec from '../openapi.json' with { type: 'json' };
import { MockFn{% if cloud_service != 'Azure Function App' %}, mockController{% endif %} } from '../test/mocks';
{%- if cloud_service == 'Azure Function App' %}

jest.unstable_mockModule('@azure/functions', () => ({ app: { http: jest.fn<MockFn>() } }));

type Registration = { methods: string[]; route: string };
let registrations: Record<string, Registration>;

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    for (const file of readdirSync(new URL('../functions', import.meta.url))) {
        if (file.endsWith('.ts')) {
            await import(`../functions/${file.slice(0, -'.ts'.length)}`);
        }
    }
    registrations = Object.fromEntries((app.http as jest.Mock<MockFn>).mock.calls.map(([name, options]) => [name, options]));
});

const registeredRoutes = (): string[] =>
    Object.values(registrations)
        .flatMap(({ methods, route }) => methods.map((method) => `${method} /${route}`))
        .sort();
{%- else %}
{% if gcp %}
jest.unstable_mockModule('@google-cloud/functions-framework', () => ({ http: jest.fn<MockFn>() }));
{%- endif %}
jest.unstable_mockModule('@config/container', () => ({
{%- for resource in resources %}
    {{ resource.name | to_lower_camel }}Controller: mockController(),
{%- endfor %}
}));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
{%- if gcp %}
let api: (req: unknown, res: unknown) => Promise<void>;
{%- else %}
let handler: (event: APIGatewayProxyEvent) => Promise<{ statusCode: number; body: string }>;
{%- endif %}
let routes: Map<string, unknown>;

beforeAll(async () => {
{%- if gcp %}
    ({ api, routes } = (await import('../main')) as unknown as { api: typeof api; routes: typeof routes });
{%- else %}
    ({ handler, routes } = (await import('../lambda')) as unknown as { handler: typeof handler; routes: typeof routes });
{%- endif %}
});

const send = async (method: string, endpoint: string, id?: string) => {
    const body = JSON.stringify({ name: 'mockName' });
{%- if gcp %}
    const res = Object.assign(new EventEmitter(), {
        headersSent: false,
        writableEnded: false,
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    });
    const path = id === undefined ? `/${endpoint}` : `/${endpoint}/${id}`;
    await api({ method, path, rawBody: Buffer.from(body), query: {}, headers: {} }, res);
    return { status: res.status.mock.calls[0][0] as number, body: res.json.mock.calls[0][0] as { errorMessage?: string } };
{%- else %}
    const event = {
        httpMethod: method,
        resource: id === undefined ? `/${endpoint}` : `/${endpoint}/{id}`,
        path: id === undefined ? `/${endpoint}` : `/${endpoint}/${id}`,
        pathParameters: id === undefined ? null : { id },
        queryStringParameters: null,
        body,
        isBase64Encoded: false,
    } as unknown as APIGatewayProxyEvent;
    const result = await handler(event);
    return { status: result.statusCode, body: JSON.parse(result.body) as { errorMessage?: string } };
{%- endif %}
};

const registeredRoutes = async (): Promise<string[]> => {
    const found: string[] = [];
    for (const endpoint of routes.keys()) {
        for (const id of [undefined, mockId]) {
            for (const method of ['GET', 'POST', 'PUT', 'PATCH', 'DELETE']) {
                const { status, body } = await send(method, endpoint, id);
                if (status !== 405 && !(status === 404 && body.errorMessage === 'Not found.')) {
                    found.push(`${method} /${endpoint}${id === undefined ? '' : '/{id}'}`);
                }
            }
        }
    }
    return found.sort();
};
{%- endif %}

const specRoutes = (): string[] =>
    Object.entries(spec.paths as Record<string, Record<string, unknown>>)
        .flatMap(([path, item]) =>
            Object.keys(item)
                .filter((key) => key !== 'parameters')
                .map((method) => `${method.toUpperCase()} ${path}`),
        )
        .sort();
{%- if ns.requests %}

const specSchemas = spec.components.schemas as Record<string, unknown>;

const requestSchemas: Record<string, z.ZodType> = {
{%- for name, schema in ns.requests %}
    {{ name }}: {{ schema }},
{%- endfor %}
};

const jsonSchema = (schema: z.ZodType): Record<string, unknown> => {
    const { $schema, ...rest } = z.toJSONSchema(schema) as Record<string, unknown>;
    return rest;
};
{%- endif %}

describe('openapi.json', () => {
{%- if cloud_service == 'Azure Function App' %}
    it('should list exactly the registered routes', () => {
        expect(registeredRoutes()).toEqual(specRoutes());
    });
{%- else %}
    it('should list exactly the registered routes', async () => {
        expect(await registeredRoutes()).toEqual(specRoutes());
    });
{%- endif %}
{%- if ns.requests %}

    it('should have a request schema per request validator', () => {
        expect(Object.keys(specSchemas).filter((name) => name.endsWith('Request')).sort()).toEqual(Object.keys(requestSchemas).sort());
    });

    it.each(Object.entries(requestSchemas))('should describe %s as its validator does', (name, schema) => {
        expect(jsonSchema(schema)).toEqual(specSchemas[name]);
    });
{%- endif %}
});
