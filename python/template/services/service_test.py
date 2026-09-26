import asyncio
import pytest
from unittest.mock import AsyncMock
from models import (
{%- for resource in resources %}
    {{ resource.name }},
    {{ resource.name }}Response,
{%- endfor %}
)
from services import (
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
)
from repositories import (
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
)

{% for resource in resources %}
{%- set r = resource.name %}
{%- set slug = r | to_snake %}
_{{ slug }}_responses = [
    {{ r }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    {{ r }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]
_{{ slug }}_update_response = {{ r }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')


def describe_{{ slug }}_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock({{ r }}Repository)
        mock.get_by_id.return_value = _{{ slug }}_responses[0]
        mock.get_list.return_value = _{{ slug }}_responses
        mock.create.return_value = _{{ slug }}_responses[0]
        mock.update.return_value = _{{ slug }}_update_response
        mock.replace.return_value = _{{ slug }}_update_response
        return mock

    @pytest.fixture
    def service(mock_repository):
        return {{ r }}Service(repository=mock_repository)
{%- if "get_by_id" in resource.operations %}

    def describe_get_by_id():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.get_by_id.assert_called_once()
            assert result == _{{ slug }}_responses[0]
{%- endif %}
{%- if "list" in resource.operations %}

    def describe_get_list():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_list())
            mock_repository.get_list.assert_called_once()
            assert result == _{{ slug }}_responses

        def test_passes_limit_through(service, mock_repository):
            asyncio.run(service.get_list(limit=25))
            mock_repository.get_list.assert_called_once_with(25)
{%- endif %}
{%- if "create" in resource.operations %}

    def describe_create():
        def test_successfully_call_repository(service, mock_repository):
            mock_item = {{ r }}(name='mockName1', type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(service.create(item=mock_item))
            mock_repository.create.assert_called_once()
            assert result == _{{ slug }}_responses[0]
{%- endif %}
{%- if "update" in resource.operations %}

    def describe_update():
        def test_successfully_call_repository(service, mock_repository):
            mock_update_item = {{ r }}(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')
            result = asyncio.run(service.update(item=mock_update_item))
            mock_repository.update.assert_called_once()
            assert result == _{{ slug }}_update_response
{%- endif %}
{%- if "replace" in resource.operations %}

    def describe_replace():
        def test_successfully_call_repository(service, mock_repository):
            mock_replace_item = {{ r }}(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')
            result = asyncio.run(service.replace(item=mock_replace_item))
            mock_repository.replace.assert_called_once()
            assert result == _{{ slug }}_update_response
{%- endif %}
{%- if "delete" in resource.operations %}

    def describe_soft_delete():
        def test_successfully_call_repository(service, mock_repository):
            asyncio.run(service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.delete.assert_called_once()
{%- endif %}

{% endfor %}