import azure.functions as func
from errors import BaseError, ValidationError
from .detect_error import detect_error
from pydantic import BaseModel

class ExampleModel(BaseModel):
    name: str
    type: str

# Test detect_error for BaseError
def test_detect_error_base_error():
    try:
        raise BaseError("Test")
    except Exception as error:
        response = detect_error(error)
        assert isinstance(response, func.HttpResponse)
        assert response.status_code == 500
        assert response.get_body().decode() == '{"type":"UnknownError","message":"Test"}'

# Test detect_error for ValidationError
def test_detect_error_validation_error():
    try:
        raise ValidationError()
    except Exception as error:
        response = detect_error(error)
        assert isinstance(response, func.HttpResponse)
        assert response.status_code == 422
        assert response.get_body().decode() == '{"type":"ValidationError","message":"Validation Error."}'

# Test detect_error for Pydantic ValidationError
def test_detect_error_pydantic_validation_error():
    try:
        ExampleModel(name="Test").model_dump()
    except Exception as error:
        response = detect_error(error)
        assert isinstance(response, func.HttpResponse)
        assert response.status_code == 422
        assert '"type":"ValidationError"' in response.get_body().decode()

# Test detect_error for unknown error
def test_detect_error_unknown_error():
    try:
        raise Exception()
    except Exception as error:
        response = detect_error(error)
        assert isinstance(response, func.HttpResponse)
        assert response.status_code == 500
        assert response.get_body().decode() == '{"type":"UnknownError","message":"Unknown Error."}'
