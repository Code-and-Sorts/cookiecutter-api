import json
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.functions import HttpResponse
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
from flask import jsonify
{%- endif %}
from models import {{ cookiecutter.project_class_name }}Response

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
def response_generator(items: {{ cookiecutter.project_class_name }}Response | list, status_code: int = 200) -> HttpResponse:
    if isinstance(items, list):
        if not items:
            return HttpResponse(
                body=json.dumps([]),
                status_code=status_code
            )
        return HttpResponse(
            body=json.dumps([item.model_dump() for item in items]),
            status_code=status_code
        )
    return HttpResponse(
        body=json.dumps(items.model_dump()),
        status_code=status_code
    )
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
def response_generator_gcp(items: {{ cookiecutter.project_class_name }}Response | list, status_code: int = 200, headers: dict = None):
    """Generate HTTP response for Google Cloud Functions."""
    if headers is None:
        headers = {}
    
    if isinstance(items, list):
        if not items:
            return (jsonify([]), status_code, headers)
        return (jsonify([item.model_dump() for item in items]), status_code, headers)
    return (jsonify(items.model_dump()), status_code, headers)
{%- endif %}
