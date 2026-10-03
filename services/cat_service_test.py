import asyncio
import pytest
from unittest.mock import AsyncMock
from models import CatCreateRequest, CatReplaceRequest, CatUpdateRequest, CatResponse
from services import CatService
from repositories import CatRepository
from conftest import ITEM_ID

_responses = [
    CatResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z"}),
    CatResponse(id="de6cbc87-5969-458c-8444-3512a82250bc"),
]


def describe_cat_service():
    @pytest.fixture
    def mock_repository():
        mock = AsyncMock(CatRepository)
        mock.get_by_id.return_value = _responses[0]
        mock.create.return_value = _responses[0]
        mock.update.return_value = _responses[0]
        mock.replace.return_value = _responses[0]
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
            item = CatCreateRequest(**{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"})
            assert asyncio.run(service.create(item, "user-1")) == _responses[0]
            mock_repository.create.assert_awaited_once_with(item, "user-1")

    def describe_update():
        def test_calls_repository(service, mock_repository):
            changes = CatUpdateRequest(**{"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "ageYears": 0, "weightKg": 1.5, "indoor": True, "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tags": [], "adoptedAt": "2026-01-15T10:00:00.000Z", "notes": "$none"})
            assert asyncio.run(service.update(ITEM_ID, changes, "user-1")) == _responses[0]
            mock_repository.update.assert_awaited_once_with(ITEM_ID, changes, "user-1")

    def describe_replace():
        def test_calls_repository(service, mock_repository):
            item = CatReplaceRequest(**{"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"})
            assert asyncio.run(service.replace(ITEM_ID, item, "user-1")) == _responses[0]
            mock_repository.replace.assert_awaited_once_with(ITEM_ID, item, "user-1")

    def describe_soft_delete():
        def test_calls_repository(service, mock_repository):
            asyncio.run(service.soft_delete(ITEM_ID, "user-1"))
            mock_repository.delete.assert_awaited_once_with(ITEM_ID, "user-1")
