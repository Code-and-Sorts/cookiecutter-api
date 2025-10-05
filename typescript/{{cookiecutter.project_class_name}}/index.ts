// Export all {% if cookiecutter.cloud_service == 'Azure Function App' %}Azure Functions{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}Cloud Functions{% endif %}
export { create{{cookiecutter.project_class_name}} } from './functions/create{{cookiecutter.project_class_name}}';
export { get{{cookiecutter.project_class_name}} } from './functions/get{{cookiecutter.project_class_name}}';
export { get{{cookiecutter.project_class_name}}s } from './functions/get{{cookiecutter.project_class_name}}s';
export { update{{cookiecutter.project_class_name}} } from './functions/update{{cookiecutter.project_class_name}}';
export { delete{{cookiecutter.project_class_name}} } from './functions/delete{{cookiecutter.project_class_name}}';

