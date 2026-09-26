import asyncio
import pytest
from unittest.mock import AsyncMock
from models import (
    Cat,
    CatResponse,
    Dog,
    DogResponse,
)
from services import (
    CatService,
    DogService,
)
from repositories import (
    CatRepository,
    DogRepository,
)


_cat_responses = [
    CatResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    CatResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]
_cat_update_response = CatResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')


def describe_cat_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock(CatRepository)
        mock.get_by_id.return_value = _cat_responses[0]
        mock.get_list.return_value = _cat_responses
        mock.create.return_value = _cat_responses[0]
        mock.update.return_value = _cat_update_response
        mock.replace.return_value = _cat_update_response
        return mock

    @pytest.fixture
    def service(mock_repository):
        return CatService(repository=mock_repository)

    def describe_get_by_id():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.get_by_id.assert_called_once()
            assert result == _cat_responses[0]

    def describe_get_list():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_list())
            mock_repository.get_list.assert_called_once()
            assert result == _cat_responses

        def test_passes_limit_through(service, mock_repository):
            asyncio.run(service.get_list(limit=25))
            mock_repository.get_list.assert_called_once_with(25)

    def describe_create():
        def test_successfully_call_repository(service, mock_repository):
            mock_item = Cat(name='mockName1', type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(service.create(item=mock_item))
            mock_repository.create.assert_called_once()
            assert result == _cat_responses[0]

    def describe_update():
        def test_successfully_call_repository(service, mock_repository):
            mock_update_item = Cat(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')
            result = asyncio.run(service.update(item=mock_update_item))
            mock_repository.update.assert_called_once()
            assert result == _cat_update_response

    def describe_soft_delete():
        def test_successfully_call_repository(service, mock_repository):
            asyncio.run(service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.delete.assert_called_once()


_dog_responses = [
    DogResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    DogResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]
_dog_update_response = DogResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')


def describe_dog_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock(DogRepository)
        mock.get_by_id.return_value = _dog_responses[0]
        mock.get_list.return_value = _dog_responses
        mock.create.return_value = _dog_responses[0]
        mock.update.return_value = _dog_update_response
        mock.replace.return_value = _dog_update_response
        return mock

    @pytest.fixture
    def service(mock_repository):
        return DogService(repository=mock_repository)

    def describe_get_by_id():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.get_by_id.assert_called_once()
            assert result == _dog_responses[0]

    def describe_get_list():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_list())
            mock_repository.get_list.assert_called_once()
            assert result == _dog_responses

        def test_passes_limit_through(service, mock_repository):
            asyncio.run(service.get_list(limit=25))
            mock_repository.get_list.assert_called_once_with(25)

    def describe_create():
        def test_successfully_call_repository(service, mock_repository):
            mock_item = Dog(name='mockName1', type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(service.create(item=mock_item))
            mock_repository.create.assert_called_once()
            assert result == _dog_responses[0]

    def describe_replace():
        def test_successfully_call_repository(service, mock_repository):
            mock_replace_item = Dog(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')
            result = asyncio.run(service.replace(item=mock_replace_item))
            mock_repository.replace.assert_called_once()
            assert result == _dog_update_response

    def describe_soft_delete():
        def test_successfully_call_repository(service, mock_repository):
            asyncio.run(service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.delete.assert_called_once()

