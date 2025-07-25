import pytest
from unittest.mock import AsyncMock, MagicMock
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.functions import HttpRequest
{%- endif %}
from models import {{ cookiecutter.project_class_name }}Response, {{ cookiecutter.project_class_name }}IdValidation
from controllers import {{ cookiecutter.project_class_name }}Controller
from services import {{ cookiecutter.project_class_name }}Service
from unittest.mock import patch

def describe_item_controller():
    @pytest.fixture
    def mock_service():
        service = AsyncMock({{ cookiecutter.project_class_name }}Service)
        return service

    @pytest.fixture
    def controller(mock_service):
        return {{ cookiecutter.project_class_name }}Controller(service=mock_service)

    def describe_get_by_id():
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
        def test_ok_valid_uuid(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'ac1df01c-7ece-4a20-ab60-179829dad8f5'}

            expected_response = {{ cookiecutter.project_class_name }}Response(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mock{{ cookiecutter.project_class_name }}", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object({{ cookiecutter.project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ cookiecutter.project_class_name }}IdValidation:
                response = controller.get_by_id(mock_request)

                Mock{{ cookiecutter.project_class_name }}IdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'mockInvalidId'}

            with patch.object({{ cookiecutter.project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ cookiecutter.project_class_name }}IdValidation:
                try:
                    controller.get_by_id(mock_request)
                except Exception as error:
                    assert isinstance(error, ValueError)
                    Mock{{ cookiecutter.project_class_name }}IdValidation.assert_called_once_with(id='mockInvalidId')
                    mock_service.get_by_id.assert_not_called()
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
        def test_ok_valid_uuid(controller, mock_service):
            item_id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'

            expected_response = {{ cookiecutter.project_class_name }}Response(id=item_id, name="mock{{ cookiecutter.project_class_name }}", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object({{ cookiecutter.project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ cookiecutter.project_class_name }}IdValidation:
                response = controller.get_by_id(item_id)

                Mock{{ cookiecutter.project_class_name }}IdValidation.assert_called_once_with(id=item_id)
                mock_service.get_by_id.assert_called_once_with(item_id)
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            item_id = 'mockInvalidId'

            with patch.object({{ cookiecutter.project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ cookiecutter.project_class_name }}IdValidation:
                try:
                    controller.get_by_id(item_id)
                except Exception as error:
                    assert isinstance(error, ValueError)
                    Mock{{ cookiecutter.project_class_name }}IdValidation.assert_called_once_with(id=item_id)
                    mock_service.get_by_id.assert_not_called()
{%- endif %}
