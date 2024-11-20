import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{cookiecutter.project_class_name}}Controller } from '@controller';
import { container } from '@config/inversity.config';
import { {{cookiecutter.project_class_name}} } from '@models';
import { detectError } from '@utils';

export async function create{{cookiecutter.project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{cookiecutter.project_lower_camel_name}}: {{cookiecutter.project_class_name}} = await request.json() as {{cookiecutter.project_class_name}};

        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        const result = await controller.post({{cookiecutter.project_lower_camel_name}});

        return {
            status: 201,
            body: JSON.stringify(result, null, 2),
            headers: {
                'Content-Type': 'application/json'
            },
        };
    } catch (error) {
        return detectError(error);
    }
};

app.http('create{{cookiecutter.project_class_name}}', {
    methods: ['POST'],
    authLevel: 'function',
    route: '{{cookiecutter.project_endpoint}}',
    handler: create{{cookiecutter.project_class_name}}
});
