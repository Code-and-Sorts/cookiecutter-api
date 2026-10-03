import { app } from '@azure/functions';
import { handle } from './response';

app.http('health', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'health',
    handler: handle(200, async () => ({ status: 'ok' })),
});
