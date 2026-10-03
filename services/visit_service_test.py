import asyncio
import pytest
from unittest.mock import AsyncMock
from models import VisitCreateRequest, VisitUpdateRequest, VisitResponse
from services import VisitService
from repositories import VisitRepository
from conftest import ITEM_ID

_responses = [
    VisitResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]}),
    VisitResponse(id="de6cbc87-5969-458c-8444-3512a82250bc"),
]


def describe_visit_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock(VisitRepository)
        mock.get_by_id.return_value = _responses[0]
        mock.create.return_value = _responses[0]
        mock.update.return_value = _responses[0]
        return mock

    @pytest.fixture
    def service(mock_repository):
        return VisitService(repository=mock_repository)

    def describe_get_by_id():
        def test_calls_repository(service, mock_repository):
            assert asyncio.run(service.get_by_id(ITEM_ID)) == _responses[0]
            mock_repository.get_by_id.assert_awaited_once_with(ITEM_ID)

    def describe_create():
        def test_calls_repository(service, mock_repository):
            item = VisitCreateRequest(**{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5})
            assert asyncio.run(service.create(item, "user-1")) == _responses[0]
            mock_repository.create.assert_awaited_once_with(item, "user-1")

    def describe_update():
        def test_calls_repository(service, mock_repository):
            changes = VisitUpdateRequest(**{"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]})
            assert asyncio.run(service.update(ITEM_ID, changes, "user-1")) == _responses[0]
            mock_repository.update.assert_awaited_once_with(ITEM_ID, changes, "user-1")
