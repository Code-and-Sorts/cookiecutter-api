import { app } from '@azure/functions';
import { catController } from '@config/container';
import { parseJsonBody } from '@utils';
import { handle, userIdFrom } from './response';

// Methods and paths with no registered function get the Functions host's own 404.
// Writes read the user id before the body so an invalid header is rejected first.

app.http('getByIdCat', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'cats/{id}',
    handler: handle(200, async (request) => catController.get(request.params.id)),
});

app.http('listCat', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'cats',
    handler: handle(200, async (request) => catController.list(request.query.get('limit'))),
});

app.http('createCat', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'cats',
    handler: handle(201, async (request) => {
        const userId = userIdFrom(request);
        return catController.post(parseJsonBody(await request.text()), userId);
    }),
});

app.http('updateCat', {
    methods: ['PATCH'],
    authLevel: 'function',
    route: 'cats/{id}',
    handler: handle(200, async (request) => {
        const userId = userIdFrom(request);
        return catController.update(request.params.id, parseJsonBody(await request.text()), userId);
    }),
});

app.http('deleteCat', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: 'cats/{id}',
    handler: handle(200, async (request) => catController.delete(request.params.id, userIdFrom(request))),
});
