import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import {
    kittenClawsController,
} from '@config/inversity.config';
import {
    KittenClaws,
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


app.http('getByIdKittenClaws', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'kitties/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Get kitties by id: '${request.url}'`);
        try {
            const result = await kittenClawsController.get(request.params.id);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('listKittenClaws', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'kitties',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`List kitties: '${request.url}'`);
        try {
            const limit = request.query.get('limit') ?? undefined;
            const result = await kittenClawsController.list(limit);
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('createKittenClaws', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'kitties',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Create kitties: '${request.url}'`);
        try {
            const item = await request.json() as KittenClaws;
            const result = await kittenClawsController.post(item);
            return jsonResponse(201, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('updateKittenClaws', {
    methods: ['PATCH'],
    authLevel: 'function',
    route: 'kitties/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Update kitties: '${request.url}'`);
        try {
            const item = await request.json() as KittenClaws;
            const result = await kittenClawsController.update({ id: request.params.id, ...item });
            return jsonResponse(200, result);
        } catch (error) {
            return detectError(error);
        }
    },
});

app.http('deleteKittenClaws', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: 'kitties/{id}',
    handler: async (request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> => {
        context.log(`Delete kitties: '${request.url}'`);
        try {
            await kittenClawsController.delete(request.params.id);
            return jsonResponse(200, { message: `KittenClaws with ID ${request.params.id} deleted.` });
        } catch (error) {
            return detectError(error);
        }
    },
});
