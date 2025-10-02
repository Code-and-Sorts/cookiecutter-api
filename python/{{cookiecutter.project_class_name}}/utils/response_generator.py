import json
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.functions import HttpResponse
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
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
def response_generator(items: {{ cookiecutter.project_class_name }}Response | list, status_code: int = 200):
    """Generate HTTP response for GCP Cloud Functions.
    Returns a tuple of (body, status_code, headers) which Flask understands.
    """
    headers = {'Content-Type': 'application/json'}
    
    if isinstance(items, list):
        if not items:
            return (json.dumps([]), status_code, headers)
        return (json.dumps([item.model_dump() for item in items]), status_code, headers)
    return (json.dumps(items.model_dump()), status_code, headers)
{%- endif %}
