import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { detectError } from '@utils';

export async function delete{{project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{project_lower_camel_name}}Id = request.params.id;

        const controller = container.resolve({{project_class_name}}Controller);

        await controller.delete({{project_lower_camel_name}}Id);

        return {
            status: 200,
            body: `{{project_class_name}} with ID ${ {{project_lower_camel_name}}Id} deleted.`,
            headers: {
                'Content-Type': 'application/json'
            },
        };
    } catch (error) {
        return detectError(error);
    }
};

app.http('delete{{project_class_name}}', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: '{{project_endpoint}}/{id}',
    handler: delete{{project_class_name}}
});
