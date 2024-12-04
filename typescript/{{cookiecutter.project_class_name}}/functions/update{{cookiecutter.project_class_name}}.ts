import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{cookiecutter.project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { {{cookiecutter.project_class_name}} } from '@models';
import { detectError } from '@utils';

export async function update{{cookiecutter.project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{cookiecutter.project_lower_camel_name}}Id = request.params.id;
        const {{cookiecutter.project_lower_camel_name}}: {{cookiecutter.project_class_name}} = await request.json() as {{cookiecutter.project_class_name}};

        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        const result = await controller.update({ id: {{cookiecutter.project_lower_camel_name}}Id, ...{{cookiecutter.project_lower_camel_name}} });

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

app.http('update{{cookiecutter.project_class_name}}', {
    methods: ['PUT'],
    authLevel: 'function',
    route: '{{cookiecutter.project_endpoint}}/{id}',
    handler: update{{cookiecutter.project_class_name}}
});
