import { app, HttpRequest, HttpResponseInit, InvocationContext } from '@azure/functions';
import { {{project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { {{project_class_name}} } from '@models';
import { detectError } from '@utils';

export async function update{{project_class_name}}(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`Http function processed request for url '${request.url}'`);

    try {
        const {{project_lower_camel_name}}Id = request.params.id;
        const {{project_lower_camel_name}}: {{project_class_name}} = await request.json() as {{project_class_name}};

        const controller = container.resolve({{project_class_name}}Controller);

        const result = await controller.update({ id: {{project_lower_camel_name}}Id, ...{{project_lower_camel_name}} });

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

app.http('update{{project_class_name}}', {
    methods: ['PUT'],
    authLevel: 'function',
    route: '{{project_endpoint}}/{id}',
    handler: update{{project_class_name}}
});
