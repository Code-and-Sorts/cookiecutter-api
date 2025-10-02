import json
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.functions import HttpResponse
{%- endif %}
from models import {{ cookiecutter.project_class_name }}Response

def response_generator(items: {{ cookiecutter.project_class_name }}Response | list, status_code: int = 200):
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    headers = {'Content-Type': 'application/json'}
{%- endif %}
    
    if isinstance(items, list):
        if not items:
            body = json.dumps([])
        else:
            body = json.dumps([item.model_dump() for item in items])
    else:
        body = json.dumps(items.model_dump())
    
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
    return HttpResponse(body=body, status_code=status_code)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    return (body, status_code, headers)
{%- endif %}
