import azure.functions as func
from errors import BaseError, ValidationError
from .detect_error import detect_error
from pydantic import BaseModel

class ExampleModel(BaseModel):
    name: str
    type: str

def describe_detect_error():
    def test_base_error_response():
        try:
            raise BaseError("Test")
        except Exception as error:
            response = detect_error(error)
            assert isinstance(response, func.HttpResponse)
            assert response.status_code == 500
            assert response.get_body().decode() == '{"type":"UnknownError","message":"Test"}'

    def test_validation_error_response():
        try:
            raise ValidationError()
        except Exception as error:
            response = detect_error(error)
            assert isinstance(response, func.HttpResponse)
            assert response.status_code == 422
            assert response.get_body().decode() == '{"type":"ValidationError","message":"Validation Error."}'

    def test_pydantic_validation_error_response():
        try:
            ExampleModel(name="Test").model_dump()
        except Exception as error:
            response = detect_error(error)
            assert isinstance(response, func.HttpResponse)
            assert response.status_code == 422
            assert '"type":"ValidationError"' in response.get_body().decode()

    def test_generic_exception_response():
        try:
            raise Exception()
        except Exception as error:
            response = detect_error(error)
            assert isinstance(response, func.HttpResponse)
            assert response.status_code == 500
            assert response.get_body().decode() == '{"type":"UnknownError","message":"Unknown Error."}'
