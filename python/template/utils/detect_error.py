from typing import Optional
{% if cloud_service == 'Azure Function App' -%}
import azure.functions as func
{%- endif %}
from errors import BaseError
from pydantic import ValidationError, BaseModel

class ErrorResponse(BaseModel):
    type: Optional[str] = None
    message: str
    details: Optional[str] = None

def generate_error_response(message: str, type: str = None, status_code: int = 500):
    error_response = ErrorResponse(
        type=type,
        message=message
    )
{%- if cloud_service == 'Azure Function App' %}
    return func.HttpResponse(
        body=error_response.model_dump_json(exclude_none=True),
        status_code=status_code
    )
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    headers = {'Content-Type': 'application/json'}
    return (error_response.model_dump_json(exclude_none=True), status_code, headers)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    return {
        "statusCode": status_code,
        "headers": {"Content-Type": "application/json"},
        "body": error_response.model_dump_json(exclude_none=True)
    }
{%- endif %}

def detect_error(error: Exception):
    if error and isinstance(error, BaseError):
        return generate_error_response(
            type=error.type,
            message=str(error),
            status_code=error.status_code
        )
    if error and isinstance(error, ValidationError):
        return generate_error_response(
            type="ValidationError",
            message=str(error.errors()),
            status_code=422
        )

    return generate_error_response(
        type="UnknownError",
        message="Unknown Error.",
        status_code=500
    )
