import { app, HttpResponseInit } from '@azure/functions';

export async function health(): Promise<HttpResponseInit> {
    return {
        status: 200,
        body: JSON.stringify({ status: 'ok' }),
        headers: {
            'Content-Type': 'application/json'
        },
    };
};

app.http('health', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'health',
    handler: health
});
