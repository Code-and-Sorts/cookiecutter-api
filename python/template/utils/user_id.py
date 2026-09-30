{% if cloud_service == 'Azure Function App' -%}
import azure.functions as func
{% endif -%}
{% if cloud_service == 'GCP Cloud Function' -%}
from flask import Request
{% endif -%}
from errors import ValidationError

USER_ID_HEADER = "X-User-Id"
MAX_USER_ID_LENGTH = 256
USER_ID_TOO_LONG_MESSAGE = f"{USER_ID_HEADER} must be at most {MAX_USER_ID_LENGTH} characters."


{% if cloud_service == 'Azure Function App' -%}
def user_id_from(req: func.HttpRequest) -> str | None:
    value = req.headers.get(USER_ID_HEADER)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' -%}
def user_id_from(request: Request) -> str | None:
    value = request.headers.get(USER_ID_HEADER)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' -%}
def user_id_from(event: dict) -> str | None:
    # API Gateway keeps the client's header casing.
    headers = event.get("headers") or {}
    value = next((value for name, value in headers.items() if name.lower() == USER_ID_HEADER.lower()), None)
{%- endif %}
    user_id = (value or "").strip()
    if len(user_id) > MAX_USER_ID_LENGTH:
        raise ValidationError(USER_ID_TOO_LONG_MESSAGE)
    return user_id or None
