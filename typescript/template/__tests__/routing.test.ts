{%- set all_ops = ["list", "get_by_id", "create", "update", "replace", "delete"] -%}
{%- if cloud_service == 'Azure Function App' -%}
import { HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { APIGatewayProxyEvent } from 'aws-lambda';
{%- endif %}
import { NotFoundError } from '@errors';
import * as container from '@config/container';

{% if cloud_service == 'Azure Function App' -%}
jest.mock('@azure/functions', () => ({ app: { http: jest.fn() } }));
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
jest.mock('@google-cloud/functions-framework', () => ({ http: jest.fn() }));
{%- endif %}
jest.mock('@config/container', () => {
    const mockController = () => ({
        list: jest.fn().mockResolvedValue([]),
        get: jest.fn().mockResolvedValue({}),
        post: jest.fn().mockResolvedValue({}),
        update: jest.fn().mockResolvedValue({}),
        replace: jest.fn().mockResolvedValue({}),
        delete: jest.fn().mockResolvedValue(undefined),
    });
    return {
{%- for resource in resources %}
        {{ resource.name | to_lower_camel }}Controller: mockController(),
{%- endfor %}
    };
});

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
const mockBody = { id: 'body-id', name: 'mockName' };
const controllers = container as unknown as Record<string, Record<string, jest.Mock>>;
{%- if cloud_service == 'Azure Function App' %}

type Registration = { methods: string[]; route: string; handler: (request: HttpRequest, context: InvocationContext) => Promise<HttpResponseInit> };
let registrations: Record<string, Registration>;

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    await import('../functions/index');
    registrations = Object.fromEntries((app.http as jest.Mock).mock.calls.map(([name, options]) => [name, options]));
});

const call = async (name: string, id?: string, body?: unknown) => {
    const request = {
        url: `http://localhost/api/${name}`,
        params: id ? { id } : {},
        query: new URLSearchParams(),
        json: async () => body,
    } as unknown as HttpRequest;
    return registrations[name].handler(request, { log: jest.fn() } as unknown as InvocationContext);
};

describe('routing', () => {
    beforeEach(() => jest.clearAllMocks());

    it('should register an anonymous health check', async () => {
        expect(registrations.health.route).toEqual('health');
        expect((await registrations.health.handler(undefined, undefined)).status).toEqual(200);
    });
{%- for resource in resources %}
{%- set c = resource.name | to_lower_camel %}
{%- set ops = resource.operations %}

    describe('{{ resource.endpoint }}', () => {
        const controller = () => controllers['{{ c }}Controller'];
{%- if "list" in ops %}

        it('should register list', async () => {
            expect(registrations['list{{ resource.name }}']).toMatchObject({ methods: ['GET'], route: '{{ resource.endpoint }}' });
            expect((await call('list{{ resource.name }}')).status).toEqual(200);
            expect(controller().list).toHaveBeenCalledTimes(1);
        });
{%- endif %}
{%- if "get_by_id" in ops %}

        it('should register get by id', async () => {
            expect(registrations['getById{{ resource.name }}']).toMatchObject({ methods: ['GET'], route: '{{ resource.endpoint }}/{id}' });
            expect((await call('getById{{ resource.name }}', mockId)).status).toEqual(200);
            expect(controller().get).toHaveBeenCalledWith(mockId);
        });

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect((await call('getById{{ resource.name }}', mockId)).status).toEqual(404);
        });
{%- endif %}
{%- if "create" in ops %}

        it('should register create', async () => {
            expect(registrations['create{{ resource.name }}']).toMatchObject({ methods: ['POST'], route: '{{ resource.endpoint }}' });
            expect((await call('create{{ resource.name }}', undefined, { name: 'mockName' })).status).toEqual(201);
            expect(controller().post).toHaveBeenCalledWith({ name: 'mockName' });
        });
{%- endif %}
{%- if "update" in ops %}

        it('should register update with the path id taking precedence', async () => {
            expect(registrations['update{{ resource.name }}']).toMatchObject({ methods: ['PATCH'], route: '{{ resource.endpoint }}/{id}' });
            expect((await call('update{{ resource.name }}', mockId, mockBody)).status).toEqual(200);
            expect(controller().update).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });
{%- endif %}
{%- if "replace" in ops %}

        it('should register replace with the path id taking precedence', async () => {
            expect(registrations['replace{{ resource.name }}']).toMatchObject({ methods: ['PUT'], route: '{{ resource.endpoint }}/{id}' });
            expect((await call('replace{{ resource.name }}', mockId, mockBody)).status).toEqual(200);
            expect(controller().replace).toHaveBeenCalledWith({ ...mockBody, id: mockId });
        });
{%- endif %}
{%- if "delete" in ops %}

        it('should register delete', async () => {
            expect(registrations['delete{{ resource.name }}']).toMatchObject({ methods: ['DELETE'], route: '{{ resource.endpoint }}/{id}' });
            expect((await call('delete{{ resource.name }}', mockId)).status).toEqual(200);
            expect(controller().delete).toHaveBeenCalledWith(mockId);
        });
{%- endif %}
{%- set disabled = all_ops | reject('in', ops) | list %}
{%- if disabled %}

        it('should not register disabled operations', () => {
{%- if "list" in disabled %}
            expect(registrations['list{{ resource.name }}']).toBeUndefined();
{%- endif %}
{%- if "get_by_id" in disabled %}
            expect(registrations['getById{{ resource.name }}']).toBeUndefined();
{%- endif %}
{%- if "create" in disabled %}
            expect(registrations['create{{ resource.name }}']).toBeUndefined();
{%- endif %}
{%- if "update" in disabled %}
            expect(registrations['update{{ resource.name }}']).toBeUndefined();
{%- endif %}
{%- if "replace" in disabled %}
            expect(registrations['replace{{ resource.name }}']).toBeUndefined();
{%- endif %}
{%- if "delete" in disabled %}
            expect(registrations['delete{{ resource.name }}']).toBeUndefined();
{%- endif %}
        });
{%- endif %}
    });
{%- endfor %}
});
{%- else %}
{%- if cloud_service == 'GCP Cloud Function' %}

let api: (req: unknown, res: unknown) => Promise<void>;

beforeAll(async () => {
    const ff = await import('@google-cloud/functions-framework');
    await import('../main');
    api = (ff.http as jest.Mock).mock.calls[0][1];
});

const send = async (method: string, path: string, body?: unknown) => {
    const res = {
        status: jest.fn().mockReturnThis(),
        json: jest.fn().mockReturnThis(),
        send: jest.fn().mockReturnThis(),
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
{%- set c = resource.name | to_lower_camel %}
{%- set ops = resource.operations %}
{%- set base = '/' ~ resource.endpoint %}
{%- set item = '/' ~ resource.endpoint ~ '/' %}

    describe('{{ resource.endpoint }}', () => {
        const controller = () => controllers['{{ c }}Controller'];

        it('{% if "list" in ops %}should route GET {{ base }} to list{% else %}should return 405 for GET {{ base }} (list disabled){% endif %}', async () => {
            expect(await send('GET', '{{ base }}')).toEqual({{ 200 if "list" in ops else 405 }});
            expect(controller().list).toHaveBeenCalledTimes({{ 1 if "list" in ops else 0 }});
        });

        it('{% if "get_by_id" in ops %}should route GET {{ item }}{id} to get{% else %}should return 405 for GET {{ item }}{id} (get_by_id disabled){% endif %}', async () => {
            expect(await send('GET', `{{ item }}${mockId}`)).toEqual({{ 200 if "get_by_id" in ops else 405 }});
            expect(controller().get).toHaveBeenCalledTimes({{ 1 if "get_by_id" in ops else 0 }});
        });

        it('{% if "create" in ops %}should route POST {{ base }} to create{% else %}should return 405 for POST {{ base }} (create disabled){% endif %}', async () => {
            expect(await send('POST', '{{ base }}', { name: 'mockName' })).toEqual({{ 201 if "create" in ops else 405 }});
            expect(controller().post).toHaveBeenCalledTimes({{ 1 if "create" in ops else 0 }});
        });

        it('{% if "update" in ops %}should route PATCH {{ item }}{id} to update{% else %}should return 405 for PATCH {{ item }}{id} (update disabled){% endif %}', async () => {
            expect(await send('PATCH', `{{ item }}${mockId}`, mockBody)).toEqual({{ 200 if "update" in ops else 405 }});
{%- if "update" in ops %}
            expect(controller().update).toHaveBeenCalledWith({ ...mockBody, id: mockId });
{%- else %}
            expect(controller().update).not.toHaveBeenCalled();
{%- endif %}
        });

        it('{% if "replace" in ops %}should route PUT {{ item }}{id} to replace{% else %}should return 405 for PUT {{ item }}{id} (replace disabled){% endif %}', async () => {
            expect(await send('PUT', `{{ item }}${mockId}`, mockBody)).toEqual({{ 200 if "replace" in ops else 405 }});
{%- if "replace" in ops %}
            expect(controller().replace).toHaveBeenCalledWith({ ...mockBody, id: mockId });
{%- else %}
            expect(controller().replace).not.toHaveBeenCalled();
{%- endif %}
        });

        it('{% if "delete" in ops %}should route DELETE {{ item }}{id} to delete{% else %}should return 405 for DELETE {{ item }}{id} (delete disabled){% endif %}', async () => {
            expect(await send('DELETE', `{{ item }}${mockId}`)).toEqual({{ 200 if "delete" in ops else 405 }});
            expect(controller().delete).toHaveBeenCalledTimes({{ 1 if "delete" in ops else 0 }});
        });
{%- if "get_by_id" in ops %}

        it('should map controller errors to responses', async () => {
            controller().get.mockRejectedValueOnce(new NotFoundError('missing'));
            expect(await send('GET', `{{ item }}${mockId}`)).toEqual(404);
        });
{%- endif %}
    });
{%- endfor %}
});
{%- endif %}
