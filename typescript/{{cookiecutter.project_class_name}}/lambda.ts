import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { {{cookiecutter.project_class_name}}Controller } from '@controllers';
import { container } from '@config/inversity.config';
import { {{cookiecutter.project_class_name}} } from '@models';
import { detectError } from '@utils';

const jsonResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

export const handler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
  const controller = container.resolve({{cookiecutter.project_class_name}}Controller);
  const id = event.pathParameters?.id;

  try {
    switch (event.httpMethod) {
      case 'GET': {
        if (id) {
          const result = await controller.get(id);
          return jsonResponse(200, result);
        }
        const results = await controller.list();
        return jsonResponse(200, results);
      }

      case 'POST': {
        const {{cookiecutter.project_lower_camel_name}}: {{cookiecutter.project_class_name}} = JSON.parse(event.body || '{}');
        const created = await controller.post({{cookiecutter.project_lower_camel_name}});
        return jsonResponse(201, created);
      }

      case 'PUT':
      case 'PATCH': {
        const {{cookiecutter.project_lower_camel_name}}: {{cookiecutter.project_class_name}} = JSON.parse(event.body || '{}');
        const updated = await controller.update({ id, ...{{cookiecutter.project_lower_camel_name}} });
        return jsonResponse(200, updated);
      }

      case 'DELETE': {
        if (!id) {
          return jsonResponse(400, { error: 'Missing item ID.' });
        }
        await controller.delete(id);
        return jsonResponse(200, {
          message: `{{cookiecutter.project_class_name}} with ID ${id} deleted.`,
        });
      }

      default:
        return jsonResponse(405, { error: 'Method not allowed.' });
    }
  } catch (error) {
    const errorResponse = detectError(error);
    return jsonResponse(errorResponse.status, errorResponse.body);
  }
};
