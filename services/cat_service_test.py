import asyncio
import pytest
from unittest.mock import AsyncMock
from models import BaseCat, CatUpdate, CatResponse
from services import CatService
from repositories import CatRepository
from conftest import ITEM_ID, TIMESTAMPS

_responses = [
    CatResponse(id=ITEM_ID, name="mockName1", **TIMESTAMPS),
    CatResponse(id="de6cbc87-5969-458c-8444-3512a82250bc", name="mockName2", **TIMESTAMPS),
]


def describe_cat_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock(CatRepository)
        mock.get_by_id.return_value = _responses[0]
        mock.create.return_value = _responses[0]
        mock.update.return_value = _responses[0]
        mock.get_list.return_value = _responses
        return mock

    @pytest.fixture
    def service(mock_repository):
        return CatService(repository=mock_repository)

    def describe_get_by_id():
        def test_calls_repository(service, mock_repository):
            assert asyncio.run(service.get_by_id(ITEM_ID)) == _responses[0]
            mock_repository.get_by_id.assert_awaited_once_with(ITEM_ID)

    def describe_get_list():
        def test_passes_limit_through(service, mock_repository):
            assert asyncio.run(service.get_list(25)) == _responses
            mock_repository.get_list.assert_awaited_once_with(25)

    def describe_create():
        def test_calls_repository(service, mock_repository):
            item = BaseCat(name="mockName1")
            assert asyncio.run(service.create(item, "user-1")) == _responses[0]
            mock_repository.create.assert_awaited_once_with(item, "user-1")

    def describe_update():
        def test_calls_repository(service, mock_repository):
            changes = CatUpdate(name="mockName1")
            assert asyncio.run(service.update(ITEM_ID, changes, "user-1")) == _responses[0]
            mock_repository.update.assert_awaited_once_with(ITEM_ID, changes, "user-1")

    def describe_soft_delete():
        def test_calls_repository(service, mock_repository):
            asyncio.run(service.soft_delete(ITEM_ID, "user-1"))
            mock_repository.delete.assert_awaited_once_with(ITEM_ID, "user-1")
