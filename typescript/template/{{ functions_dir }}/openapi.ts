import { app } from '@azure/functions';
import spec from '../openapi.json' with { type: 'json' };
import { handle } from './response';

app.http('openapi', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'openapi.json',
    handler: handle(200, async () => spec),
});
