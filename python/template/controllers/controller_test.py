import asyncio
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
{% if cloud_service == 'Azure Function App' -%}
from azure.functions import HttpRequest
{%- endif %}
from models import (
{%- for resource in resources %}
    {{ resource.name }}Response,
    {{ resource.name }}IdValidation,
{%- endfor %}
)
from controllers import (
{%- for resource in resources %}
    {{ resource.name }}Controller,
{%- endfor %}
)
from services import (
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
)
from repositories.repository import DEFAULT_LIST_LIMIT

{% for resource in resources %}
{%- set r = resource.name %}
{%- set slug = r | to_snake %}
def describe_{{ slug }}_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock({{ r }}Service)

    @pytest.fixture
    def controller(mock_service):
        return {{ r }}Controller(service=mock_service)
{%- if "get_by_id" in resource.operations %}

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
{%- if cloud_service == 'Azure Function App' %}
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'ac1df01c-7ece-4a20-ab60-179829dad8f5'}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
            mock_request = MagicMock()
            mock_request.path = '/{{ resource.endpoint }}/ac1df01c-7ece-4a20-ab60-179829dad8f5'
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
            mock_request = {"pathParameters": {"item_id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"}}
{%- endif %}
            expected_response = {{ r }}Response(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mockName", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object({{ r }}IdValidation, '__init__', return_value=None) as MockIdValidation:
                response = asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
{%- if cloud_service == 'Azure Function App' %}
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'mockInvalidId'}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
            mock_request = MagicMock()
            mock_request.path = '/{{ resource.endpoint }}/mockInvalidId'
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
            mock_request = {"pathParameters": {"item_id": "mockInvalidId"}}
{%- endif %}
            with patch.object({{ r }}IdValidation, '__init__', side_effect=ValueError("Invalid ID")) as MockIdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()
{%- endif %}
{%- if "list" in resource.operations %}

    def describe_get_list():
        def test_default_limit(controller, mock_service):
{%- if cloud_service == 'Azure Function App' %}
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.params = {}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
            mock_request = MagicMock()
            mock_request.args = {}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
            mock_request = {}
{%- endif %}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
{%- if cloud_service == 'Azure Function App' %}
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.params = {'limit': '5'}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
            mock_request = MagicMock()
            mock_request.args = {'limit': '5'}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
            mock_request = {"queryStringParameters": {"limit": "5"}}
{%- endif %}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(5)
{%- endif %}

{% endfor %}