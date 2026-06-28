import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { detectError } from '@utils';

export async function get{{project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{project_lower_camel_name}}Id = request.params.id;

        const controller = container.resolve({{project_class_name}}Controller);

        const result = await controller.get({{project_lower_camel_name}}Id);

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

app.http('get{{project_class_name}}', {
    methods: ['GET'],
    authLevel: 'function',
    route: '{{project_endpoint}}/{id}',
    handler: get{{project_class_name}}
});
