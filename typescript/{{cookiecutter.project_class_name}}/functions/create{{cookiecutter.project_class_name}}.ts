{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
import { Request, Response } from '@google-cloud/functions-framework';
{%- endif %}
import { {{cookiecutter.project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { {{cookiecutter.project_class_name}} } from '@models';
import { detectError } from '@utils';

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
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
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
export async function create{{cookiecutter.project_class_name}}(req: Request, res: Response): Promise<void> {
    console.log(`HTTP function processed request for url '${req.url}'`);

    try {
        const {{cookiecutter.project_lower_camel_name}}: {{cookiecutter.project_class_name}} = req.body as {{cookiecutter.project_class_name}};

        const controller = container.resolve({{cookiecutter.project_class_name}}Controller);

        const result = await controller.post({{cookiecutter.project_lower_camel_name}});

        res.status(201).json(result);
    } catch (error) {
        const errorResponse = detectError(error);
        res.status(errorResponse.status).json({ error: errorResponse.body });
    }
};
{%- endif %}
