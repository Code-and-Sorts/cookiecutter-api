import asyncio
import pytest
from unittest.mock import AsyncMock
from models import Item, ItemResponse
from services import Service
from repositories import Repository

mock_item_responses = [
    ItemResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1',type='mockType1'),
    ItemResponse(id='de6cbc87-5969-458c-8444-3512a82250bc',name='mockName2',type='mockType2')
]
mock_update_item_response = ItemResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')

def describe_item_service():
    @pytest.fixture
    def mock_repository() -> Repository:
        mock = AsyncMock(Repository)
        mock.get_by_id.return_value = mock_item_responses[0]
        mock.get_list.return_value = mock_item_responses
        mock.create.return_value = mock_item_responses[0]
        mock.update.return_value = mock_update_item_response
        mock.replace.return_value = mock_update_item_response
        return mock

    @pytest.fixture
    def service(mock_repository) -> Service:
        return Service(repository=mock_repository)

    def describe_get_by_id():
        def test_successfully_call_repository(service: Service, mock_repository: Repository):
            result = asyncio.run(service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.get_by_id.assert_called_once()
            assert result == mock_item_responses[0]

    def describe_get_list():
        def test_successfully_call_repository(service: Service, mock_repository: Repository):
            result = asyncio.run(service.get_list())
            mock_repository.get_list.assert_called_once()
            assert result == mock_item_responses

        def test_passes_limit_through(service: Service, mock_repository: Repository):
            asyncio.run(service.get_list(limit=25))
            mock_repository.get_list.assert_called_once_with(25)

    def describe_create():
        def test_successfully_call_repository(service: Service, mock_repository: Repository):
            mock_item = Item(name='mockName1',type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(service.create(item=mock_item))
            mock_repository.create.assert_called_once()
            assert result == mock_item_responses[0]

    def describe_update():
        def test_successfully_call_repository(service: Service, mock_repository: Repository):
            mock_update_item = Item(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')
            result = asyncio.run(service.update(item=mock_update_item))
            mock_repository.update.assert_called_once()
            assert result == mock_update_item_response

    def describe_replace():
        def test_successfully_call_repository(service: Service, mock_repository: Repository):
            mock_replace_item = Item(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')
            result = asyncio.run(service.replace(item=mock_replace_item))
            mock_repository.replace.assert_called_once()
            assert result == mock_update_item_response

    def describe_soft_delete():
        def test_successfully_call_repository(service: Service, mock_repository: Repository):
            asyncio.run(service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.delete.assert_called_once()
