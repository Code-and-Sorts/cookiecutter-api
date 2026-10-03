import { app } from '@azure/functions';
import { visitController } from '@config/container';
import { parseJsonBody } from '@utils';
import { handle, userIdFrom } from './response';

// Methods and paths with no registered function get the Functions host's own 404.
// Writes read the user id before the body so an invalid header is rejected first.

app.http('getByIdVisit', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'visits/{id}',
    handler: handle(200, async (request) => visitController.get(request.params.id)),
});

app.http('createVisit', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'visits',
    handler: handle(201, async (request) => {
        const userId = userIdFrom(request);
        return visitController.post(parseJsonBody(await request.text()), userId);
    }),
});

app.http('updateVisit', {
    methods: ['PATCH'],
    authLevel: 'function',
    route: 'visits/{id}',
    handler: handle(200, async (request) => {
        const userId = userIdFrom(request);
        return visitController.update(request.params.id, parseJsonBody(await request.text()), userId);
    }),
});
