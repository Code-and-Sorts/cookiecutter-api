{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
import { Request, Response } from '@google-cloud/functions-framework';
{%- endif %}
import { {{cookiecutter.project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { detectError } from '@utils';

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
export async function delete{{cookiecutter.project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{cookiecutter.project_lower_camel_name}}Id = request.params.id;

        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        await controller.delete({{cookiecutter.project_lower_camel_name}}Id);

        return {
            status: 200,
            body: `{{cookiecutter.project_class_name}} with ID ${ {{cookiecutter.project_lower_camel_name}}Id} deleted.`,
            headers: {
                'Content-Type': 'application/json'
            },
        };
    } catch (error) {
        return detectError(error);
    }
};

app.http('delete{{cookiecutter.project_class_name}}', {
    methods: ['DELETE'],
    authLevel: 'function',
    route: '{{cookiecutter.project_endpoint}}/{id}',
    handler: delete{{cookiecutter.project_class_name}}
});
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
export async function delete{{cookiecutter.project_class_name}}(req: Request, res: Response): Promise<void> {
    console.log(`HTTP function processed request for url '${req.url}'`);

    try {
        const {{cookiecutter.project_lower_camel_name}}Id = req.params.id || req.query.id as string;

        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        await controller.delete({{cookiecutter.project_lower_camel_name}}Id);

        res.status(200).json({ message: `{{cookiecutter.project_class_name}} with ID ${ {{cookiecutter.project_lower_camel_name}}Id} deleted.` });
    } catch (error) {
        const errorResponse = detectError(error);
        res.status(errorResponse.status).json({ error: errorResponse.body });
    }
};
{%- endif %}
