import asyncio
import pytest
from unittest.mock import AsyncMock
from models import (
    KittenClaws,
    KittenClawsResponse,
)
from services import (
    KittenClawsService,
)
from repositories import (
    KittenClawsRepository,
)


_kitten_claws_responses = [
    KittenClawsResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    KittenClawsResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]
_kitten_claws_update_response = KittenClawsResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')


def describe_kitten_claws_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock(KittenClawsRepository)
        mock.get_by_id.return_value = _kitten_claws_responses[0]
        mock.get_list.return_value = _kitten_claws_responses
        mock.create.return_value = _kitten_claws_responses[0]
        mock.update.return_value = _kitten_claws_update_response
        mock.replace.return_value = _kitten_claws_update_response
        return mock

    @pytest.fixture
    def service(mock_repository):
        return KittenClawsService(repository=mock_repository)

    def describe_get_by_id():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.get_by_id.assert_called_once()
            assert result == _kitten_claws_responses[0]

    def describe_get_list():
        def test_successfully_call_repository(service, mock_repository):
            result = asyncio.run(service.get_list())
            mock_repository.get_list.assert_called_once()
            assert result == _kitten_claws_responses

        def test_passes_limit_through(service, mock_repository):
            asyncio.run(service.get_list(limit=25))
            mock_repository.get_list.assert_called_once_with(25)

    def describe_create():
        def test_successfully_call_repository(service, mock_repository):
            mock_item = KittenClaws(name='mockName1', type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(service.create(item=mock_item))
            mock_repository.create.assert_called_once()
            assert result == _kitten_claws_responses[0]

    def describe_update():
        def test_successfully_call_repository(service, mock_repository):
            mock_update_item = KittenClaws(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1-Update', type='mockType1-Update')
            result = asyncio.run(service.update(item=mock_update_item))
            mock_repository.update.assert_called_once()
            assert result == _kitten_claws_update_response

    def describe_soft_delete():
        def test_successfully_call_repository(service, mock_repository):
            asyncio.run(service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_repository.delete.assert_called_once()

