import { app } from '@azure/functions';
import { dogController } from '@config/container';
import { parseJsonBody } from '@utils';
import { handle, userIdFrom } from './response';

// Methods and paths with no registered function get the Functions host's own 404.
// Writes read the user id before the body so an invalid header is rejected first.

app.http('getByIdDog', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'dogs/{id}',
    handler: handle(200, async (request) => dogController.get(request.params.id)),
});

app.http('listDog', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'dogs',
    handler: handle(200, async (request) => dogController.list(request.query.get('limit'))),
});

app.http('createDog', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'dogs',
    handler: handle(201, async (request) => {
        const userId = userIdFrom(request);
        return dogController.post(parseJsonBody(await request.text()), userId);
    }),
});

app.http('replaceDog', {
    methods: ['PUT'],
    authLevel: 'function',
    route: 'dogs/{id}',
    handler: handle(200, async (request) => {
        const userId = userIdFrom(request);
        return dogController.replace(request.params.id, parseJsonBody(await request.text()), userId);
    }),
});

app.http('deleteDog', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: 'dogs/{id}',
    handler: handle(200, async (request) => dogController.delete(request.params.id, userIdFrom(request))),
});
