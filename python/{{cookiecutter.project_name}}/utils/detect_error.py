# from azure.functions import HttpResponse
from typing import Optional
import azure.functions as func
from errors import BaseError
from pydantic import ValidationError, BaseModel

class ErrorResponse(BaseModel):
    type: Optional[str] = None
    message: str
    details: Optional[str] = None

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
