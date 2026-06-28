import asyncio
import pytest
from unittest.mock import AsyncMock
from models import {{ project_class_name }}, {{ project_class_name }}Response
from services import {{ project_class_name }}Service
from repositories import {{ project_class_name }}Repository

mock_item_responses = [
    {{ project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1',type='mockType1'),
    {{ project_class_name }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc',name='mockName2',type='mockType2')
]
mock_update_item_response = {{ project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')

def describe_item_service():
    @pytest.fixture
    def mock_repository() -> {{ project_class_name }}Repository:
        mock = AsyncMock({{ project_class_name }}Repository)
        mock.get_by_id.return_value = mock_item_responses[0]
        mock.get_list.return_value = mock_item_responses
        mock.create.return_value = mock_item_responses[0]
        mock.update.return_value = mock_update_item_response
        return mock

    @pytest.fixture
    def service(mock_repository) -> {{ project_class_name }}Service:
        return {{ project_class_name }}Service(repository=mock_repository)

    def describe_get_by_id():
        def test_successfully_call_repository(service: {{ project_class_name }}Service, mock_repository: {{ project_class_name }}Repository):
            result = asyncio.run(service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.get_by_id.assert_called_once()
            assert result == mock_item_responses[0]

    def describe_get_list():
        def test_successfully_call_repository(service: {{ project_class_name }}Service, mock_repository: {{ project_class_name }}Repository):
            result = asyncio.run(service.get_list())
            mock_repository.get_list.assert_called_once()
            assert result == mock_item_responses

        def test_passes_limit_through(service: {{ project_class_name }}Service, mock_repository: {{ project_class_name }}Repository):
            asyncio.run(service.get_list(limit=25))
            mock_repository.get_list.assert_called_once_with(25)

    def describe_create():
        def test_successfully_call_repository(service: {{ project_class_name }}Service, mock_repository: {{ project_class_name }}Repository):
            mock_item = {{ project_class_name }}(name='mockName1',type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(service.create(item=mock_item))
            mock_repository.create.assert_called_once()
            assert result == mock_item_responses[0]

    def describe_update():
        def test_successfully_call_repository(service: {{ project_class_name }}Service, mock_repository: {{ project_class_name }}Repository):
            mock_update_item = {{ project_class_name }}(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')
            result = asyncio.run(service.update(item=mock_update_item))
            mock_repository.update.assert_called_once()
            assert result == mock_update_item_response

    def describe_soft_delete():
        def test_successfully_call_repository(service: {{ project_class_name }}Service, mock_repository: {{ project_class_name }}Repository):
            asyncio.run(service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.delete.assert_called_once()
