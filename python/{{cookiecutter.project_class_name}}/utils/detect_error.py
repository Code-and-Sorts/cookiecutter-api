# from azure.functions import HttpResponse
from typing import Optional
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import azure.functions as func
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
from flask import jsonify
{%- endif %}
from errors import BaseError
from pydantic import ValidationError, BaseModel

class ErrorResponse(BaseModel):
    type: Optional[str] = None
    message: str
    details: Optional[str] = None

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
def generate_error_response(message: str, type: str = None, status_code: int = 500) -> func.HttpResponse:
    error_response = ErrorResponse(
        type=type,
        message=message
    )
    return func.HttpResponse(
        body=error_response.model_dump_json(exclude_none=True),
        status_code=status_code
    )

def detect_error(error: Exception) -> func.HttpResponse:
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
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
def generate_error_response_gcp(message: str, type: str = None, status_code: int = 500, headers: dict = None):
    """Generate error response for Google Cloud Functions."""
    if headers is None:
        headers = {}
    
    error_response = ErrorResponse(
        type=type,
        message=message
    )
    return (jsonify(error_response.model_dump(exclude_none=True)), status_code, headers)

def detect_error(error: Exception, headers: dict = None):
    """Detect and handle errors for Google Cloud Functions."""
    if headers is None:
        headers = {}
    
    if error and isinstance(error, BaseError):
        return generate_error_response_gcp(
            type=error.type,
            message=str(error),
            status_code=error.status_code,
            headers=headers
        )
    if error and isinstance(error, ValidationError):
        return generate_error_response_gcp(
            type="ValidationError",
            message=str(error.errors()),
            status_code=422,
            headers=headers
        )

    return generate_error_response_gcp(
        type="UnknownError",
        message="Unknown Error.",
        status_code=500,
        headers=headers
    )
{%- endif %}
