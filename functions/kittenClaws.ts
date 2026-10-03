import { app } from '@azure/functions';
import { kittenClawsController } from '@config/container';
import { parseJsonBody } from '@utils';
import { handle, userIdFrom } from './response';

// Methods and paths with no registered function get the Functions host's own 404.
// Writes read the user id before the body so an invalid header is rejected first.

app.http('getByIdKittenClaws', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'kittenclaws/{id}',
    handler: handle(200, async (request) => kittenClawsController.get(request.params.id)),
});

app.http('listKittenClaws', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'kittenclaws',
    handler: handle(200, async (request) => kittenClawsController.list(request.query.get('limit'))),
});

app.http('createKittenClaws', {
    methods: ['POST'],
    authLevel: 'function',
    route: 'kittenclaws',
    handler: handle(201, async (request) => {
        const userId = userIdFrom(request);
        return kittenClawsController.post(parseJsonBody(await request.text()), userId);
    }),
});

app.http('updateKittenClaws', {
    methods: ['PATCH'],
    authLevel: 'function',
    route: 'kittenclaws/{id}',
    handler: handle(200, async (request) => {
        const userId = userIdFrom(request);
        return kittenClawsController.update(request.params.id, parseJsonBody(await request.text()), userId);
    }),
});

app.http('deleteKittenClaws', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: 'kittenclaws/{id}',
    handler: handle(200, async (request) => kittenClawsController.delete(request.params.id, userIdFrom(request))),
});
