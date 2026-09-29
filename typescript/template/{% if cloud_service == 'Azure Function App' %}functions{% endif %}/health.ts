import { app, HttpResponseInit } from '@azure/functions';

app.http('health', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'health',
    handler: async (): Promise<HttpResponseInit> => ({
        status: 200,
        body: JSON.stringify({ status: 'ok' }, null, 2),
        headers: { 'Content-Type': 'application/json' },
    }),
});
