import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{cookiecutter.project_class_name}}Controller } from '@controller';
import { container } from '@config/inversity.config';
import { detectError } from '@utils';

export async function get{{cookiecutter.project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{cookiecutter.project_lower_camel_name}}Id = request.params.id;

        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        const result = await controller.get({{cookiecutter.project_lower_camel_name}}Id);

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

app.http('get{{cookiecutter.project_class_name}}', {
    methods: ['GET'],
    authLevel: 'function',
    route: '{{cookiecutter.project_endpoint}}/{id}',
    handler: get{{cookiecutter.project_class_name}}
});
