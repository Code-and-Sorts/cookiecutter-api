import json
{%- if cloud_service == 'Azure Function App' %}
from azure.functions import HttpResponse
{%- endif %}
from pydantic import BaseModel

JSON_CONTENT_TYPE = "application/json"


def _plain(value):
    return value.model_dump() if isinstance(value, BaseModel) else value


def to_json(payload) -> str:
    """Serialize a model, a list of models, or plain JSON data."""
    if isinstance(payload, list):
        return json.dumps([_plain(item) for item in payload])
    return json.dumps(_plain(payload))


def response_generator(payload, status_code: int = 200):
    """Build a JSON response (``Content-Type: application/json``) for this cloud."""
    body = to_json(payload)
{%- if cloud_service == 'Azure Function App' %}
    return HttpResponse(body=body, status_code=status_code, mimetype=JSON_CONTENT_TYPE)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    return (body, status_code, {"Content-Type": JSON_CONTENT_TYPE})
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    return {
        "statusCode": status_code,
        "headers": {"Content-Type": JSON_CONTENT_TYPE},
        "body": body
    }
{%- endif %}
