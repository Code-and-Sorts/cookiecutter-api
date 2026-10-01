import { app } from '@azure/functions';
import spec from '../openapi.json' with { type: 'json' };
import { handle } from './response';

// Anonymous like the health check: the document describes the API, not its data.
app.http('openapi', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'openapi.json',
    handler: handle(200, async () => spec),
});
