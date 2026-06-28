import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { detectError } from '@utils';

export async function get{{project_class_name}}s(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const controller = container.resolve({{project_class_name}}Controller);

        const limit = request.query.get('limit') ?? undefined;
        const result = await controller.list(limit);

        return {
            status: 200,
            body: JSON.stringify(result, null, 2),
            headers: {
                'Content-Type': 'application/json'
            },
        };
    } catch (error) {
        return detectError(error);
    }
};

app.http('get{{project_class_name}}s', {
    methods: ['GET'],
    authLevel: 'function',
    route: '{{project_endpoint}}',
    handler: get{{project_class_name}}s
});
