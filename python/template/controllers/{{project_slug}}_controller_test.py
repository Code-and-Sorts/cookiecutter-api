{% if cloud_service == 'Azure Function App' -%}
import asyncio
import pytest
from unittest.mock import AsyncMock, MagicMock
from azure.functions import HttpRequest
from models import {{ project_class_name }}Response, {{ project_class_name }}IdValidation
from controllers import {{ project_class_name }}Controller
from services import {{ project_class_name }}Service
from repositories.{{ project_slug }}_repository import DEFAULT_LIST_LIMIT
from unittest.mock import patch

def describe_item_controller():
    @pytest.fixture
    def mock_service():
        service = AsyncMock({{ project_class_name }}Service)
        return service

    @pytest.fixture
    def controller(mock_service):
        return {{ project_class_name }}Controller(service=mock_service)

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'ac1df01c-7ece-4a20-ab60-179829dad8f5'}

            expected_response = {{ project_class_name }}Response(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mock{{ project_class_name }}", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object({{ project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ project_class_name }}IdValidation:
                response = asyncio.run(controller.get_by_id(mock_request))

                Mock{{ project_class_name }}IdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'mockInvalidId'}

            with patch.object({{ project_class_name }}IdValidation, '__init__', side_effect=ValueError("Invalid ID")) as Mock{{ project_class_name }}IdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_request))

                Mock{{ project_class_name }}IdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.params = {}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.params = {'limit': '5'}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(5)
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
import asyncio
import pytest
from unittest.mock import AsyncMock, MagicMock
from models import {{ project_class_name }}Response, {{ project_class_name }}IdValidation
from controllers import {{ project_class_name }}Controller
from services import {{ project_class_name }}Service
from repositories.{{ project_slug }}_repository import DEFAULT_LIST_LIMIT
from unittest.mock import patch

def describe_item_controller():
    @pytest.fixture
    def mock_service():
        service = AsyncMock({{ project_class_name }}Service)
        return service

    @pytest.fixture
    def controller(mock_service):
        return {{ project_class_name }}Controller(service=mock_service)

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
            mock_request = MagicMock()
            mock_request.path = '/kitties/ac1df01c-7ece-4a20-ab60-179829dad8f5'

            expected_response = {{ project_class_name }}Response(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mock{{ project_class_name }}", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object({{ project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ project_class_name }}IdValidation:
                response = asyncio.run(controller.get_by_id(mock_request))

                Mock{{ project_class_name }}IdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_request = MagicMock()
            mock_request.path = '/kitties/mockInvalidId'

            with patch.object({{ project_class_name }}IdValidation, '__init__', side_effect=ValueError("Invalid ID")) as Mock{{ project_class_name }}IdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_request))

                Mock{{ project_class_name }}IdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_request = MagicMock()
            mock_request.args = {}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_request = MagicMock()
            mock_request.args = {'limit': '5'}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(5)
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
import asyncio
import pytest
from unittest.mock import AsyncMock
from models import {{ project_class_name }}Response, {{ project_class_name }}IdValidation
from controllers import {{ project_class_name }}Controller
from services import {{ project_class_name }}Service
from repositories.{{ project_slug }}_repository import DEFAULT_LIST_LIMIT
from unittest.mock import patch

def describe_item_controller():
    @pytest.fixture
    def mock_service():
        service = AsyncMock({{ project_class_name }}Service)
        return service

    @pytest.fixture
    def controller(mock_service):
        return {{ project_class_name }}Controller(service=mock_service)

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
            mock_event = {
                "pathParameters": {"item_id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"}
            }

            expected_response = {{ project_class_name }}Response(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mock{{ project_class_name }}", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object({{ project_class_name }}IdValidation, '__init__', return_value=None) as Mock{{ project_class_name }}IdValidation:
                response = asyncio.run(controller.get_by_id(mock_event))

                Mock{{ project_class_name }}IdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_event = {
                "pathParameters": {"item_id": "mockInvalidId"}
            }

            with patch.object({{ project_class_name }}IdValidation, '__init__', side_effect=ValueError("Invalid ID")) as Mock{{ project_class_name }}IdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_event))

                Mock{{ project_class_name }}IdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_event = {}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_event))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_event = {"queryStringParameters": {"limit": "5"}}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_event))
            mock_service.get_list.assert_called_once_with(5)
{%- endif %}
