import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import {
    catController,
    dogController,
} from '@config/inversity.config';
import {
    Cat,
    Dog,
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


app.http('getByIdCat', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'cats/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Get cats by id: '${request.url}'`);
        try {
            const result = await catController.get(request.params.id);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('listCat', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'cats',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`List cats: '${request.url}'`);
        try {
            const limit = request.query.get('limit') ?? undefined;
            const result = await catController.list(limit);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('createCat', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'cats',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Create cats: '${request.url}'`);
        try {
            const item = await request.json() as Cat;
            const result = await catController.post(item);
            return jsonResponse(201, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('updateCat', {
    methods: ['PATCH'],
    authLevel: 'function',
    route: 'cats/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Update cats: '${request.url}'`);
        try {
            const item = await request.json() as Cat;
            const result = await catController.update({ id: request.params.id, ...item });
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('deleteCat', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: 'cats/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Delete cats: '${request.url}'`);
        try {
            await catController.delete(request.params.id);
            return jsonResponse(200, { message: `Cat with ID ${request.params.id} deleted.` });
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('getByIdDog', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'dogs/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Get dogs by id: '${request.url}'`);
        try {
            const result = await dogController.get(request.params.id);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('listDog', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'dogs',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`List dogs: '${request.url}'`);
        try {
            const limit = request.query.get('limit') ?? undefined;
            const result = await dogController.list(limit);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('createDog', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'dogs',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Create dogs: '${request.url}'`);
        try {
            const item = await request.json() as Dog;
            const result = await dogController.post(item);
            return jsonResponse(201, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('replaceDog', {
    methods: ['PUT'],
    authLevel: 'function',
    route: 'dogs/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Replace dogs: '${request.url}'`);
        try {
            const item = await request.json() as Dog;
            const result = await dogController.replace({ id: request.params.id, ...item });
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('deleteDog', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: 'dogs/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Delete dogs: '${request.url}'`);
        try {
            await dogController.delete(request.params.id);
            return jsonResponse(200, { message: `Dog with ID ${request.params.id} deleted.` });
        } catch (error) {
            return detectError(error);
        }
    },
});
