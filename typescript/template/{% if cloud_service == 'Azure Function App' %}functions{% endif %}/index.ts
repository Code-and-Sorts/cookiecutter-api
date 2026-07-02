import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import {
{%- for resource in resources %}
    {{ resource.name | to_lower_camel }}Controller,
{%- endfor %}
} from '@config/inversity.config';
import {
{%- for resource in resources %}
    {{ resource.name }},
{%- endfor %}
} from '@models';
import { detectError } from '@utils';

const jsonResponse = (status: number, body: unknown): HttpResponseInit => ({
    status,
    body: JSON.stringify(body, null, 2),
    headers: { 'Content-Type': 'application/json' },
});

app.http('health', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'health',
    handler: async (): Promise<HttpResponseInit> => jsonResponse(200, { status: 'ok' }),
});
{% for resource in resources %}
{%- set c = resource.name | to_lower_camel %}
{%- if "get_by_id" in resource.operations %}

app.http('getById{{ resource.name }}', {
    methods: ['GET'],
    authLevel: 'function',
    route: '{{ resource.endpoint }}/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Get {{ resource.endpoint }} by id: '${request.url}'`);
        try {
            const result = await {{ c }}Controller.get(request.params.id);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});
{%- endif %}
{%- if "list" in resource.operations %}

app.http('list{{ resource.name }}', {
    methods: ['GET'],
    authLevel: 'function',
    route: '{{ resource.endpoint }}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`List {{ resource.endpoint }}: '${request.url}'`);
        try {
            const limit = request.query.get('limit') ?? undefined;
            const result = await {{ c }}Controller.list(limit);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});
{%- endif %}
{%- if "create" in resource.operations %}

app.http('create{{ resource.name }}', {
    methods: ['POST'],
    authLevel: 'function',
    route: '{{ resource.endpoint }}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Create {{ resource.endpoint }}: '${request.url}'`);
        try {
            const item = await request.json() as {{ resource.name }};
            const result = await {{ c }}Controller.post(item);
            return jsonResponse(201, result);
        } catch (error) {
            return detectError(error);
        }
    },
});
{%- endif %}
{%- if "update" in resource.operations %}

app.http('update{{ resource.name }}', {
    methods: ['PATCH'],
    authLevel: 'function',
    route: '{{ resource.endpoint }}/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Update {{ resource.endpoint }}: '${request.url}'`);
        try {
            const item = await request.json() as {{ resource.name }};
            const result = await {{ c }}Controller.update({ id: request.params.id, ...item });
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});
{%- endif %}
{%- if "replace" in resource.operations %}

app.http('replace{{ resource.name }}', {
    methods: ['PUT'],
    authLevel: 'function',
    route: '{{ resource.endpoint }}/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Replace {{ resource.endpoint }}: '${request.url}'`);
        try {
            const item = await request.json() as {{ resource.name }};
            const result = await {{ c }}Controller.replace({ id: request.params.id, ...item });
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});
{%- endif %}
{%- if "delete" in resource.operations %}

app.http('delete{{ resource.name }}', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: '{{ resource.endpoint }}/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Delete {{ resource.endpoint }}: '${request.url}'`);
        try {
            await {{ c }}Controller.delete(request.params.id);
            return jsonResponse(200, { message: `{{ resource.name }} with ID ${request.params.id} deleted.` });
        } catch (error) {
            return detectError(error);
        }
    },
});
{%- endif %}
{%- endfor %}
