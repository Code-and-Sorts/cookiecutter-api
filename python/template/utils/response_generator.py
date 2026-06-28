import json
{% if cloud_service == 'Azure Function App' -%}
from azure.functions import HttpResponse
{%- endif %}
from models import ItemResponse

def response_generator(items: ItemResponse | list, status_code: int = 200):
{%- if cloud_service == 'GCP Cloud Function' %}
    headers = {'Content-Type': 'application/json'}
{%- endif %}

    if isinstance(items, list):
        if not items:
            body = json.dumps([])
        else:
            body = json.dumps([item.model_dump() for item in items])
    else:
        body = json.dumps(items.model_dump())

{%- if cloud_service == 'Azure Function App' %}
    return HttpResponse(body=body, status_code=status_code)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    return (body, status_code, headers)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    return {
        "statusCode": status_code,
        "headers": {"Content-Type": "application/json"},
        "body": body
    }
{%- endif %}
