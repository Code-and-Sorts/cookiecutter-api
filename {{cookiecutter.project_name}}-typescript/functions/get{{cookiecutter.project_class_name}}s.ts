import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{cookiecutter.project_class_name}}Controller } from '@controller';
import { container } from '@config/inversity.config';
import { detectError } from '@utils';

export async function get{{cookiecutter.project_class_name}}s(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        const result = await controller.list();

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

app.http('get{{cookiecutter.project_class_name}}s', {
    methods: ['GET'],
    authLevel: 'function',
    route: '{{cookiecutter.project_endpoint}}',
    handler: get{{cookiecutter.project_class_name}}s
});
