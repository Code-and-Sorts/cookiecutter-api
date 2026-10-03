import asyncio
from unittest.mock import AsyncMock, MagicMock
from models import CatEntity, CatCreateRequest, CatReplaceRequest, CatUpdateRequest, CatResponse
from repositories import BaseRepository, CatRepository
from conftest import ITEM_ID

_sample = {"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"}
_response = CatResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z"})


def _repository() -> CatRepository:
    return CatRepository(MagicMock(), "cats", "us-east-1")


def describe_cat_repository():
    def test_extends_base_repository_with_cat_models():
        assert issubclass(CatRepository, BaseRepository)
        assert CatRepository.entity_model is CatEntity
        assert CatRepository.response_model is CatResponse
        assert CatRepository.resource_name == "Cat"

    def test_entity_lists_every_client_field():
        assert CatEntity.client_fields() == ["tenantId", "region", "priority", "rank", "labels", "name", "breed", "ageYears", "weightKg", "indoor", "birthDate", "microchipId", "ownerEmail", "website", "tagCode", "tags", "scores", "adoptedAt", "lastVisit", "notes"]

    def test_get_by_id_returns_cat_response():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, **_sample, "isDeleted": False})

        result = asyncio.run(repository.get_by_id(ITEM_ID))

        assert isinstance(result, CatResponse)
        assert result == _response

    def test_a_record_without_a_field_reads_its_static_default():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, "isDeleted": False})

        result = asyncio.run(repository.get_by_id(ITEM_ID)).model_dump()

        assert result == {
            "id": ITEM_ID,
            "tenantId": "public",
            "region": "eu",
            "priority": None,
            "rank": None,
            "labels": [],
            "name": None,
            "breed": "tabby",
            "ageYears": 0,
            "weightKg": None,
            "indoor": True,
            "birthDate": None,
            "microchipId": None,
            "ownerEmail": "unknown@example.com",
            "website": None,
            "tagCode": None,
            "tags": [],
            "scores": None,
            "adoptedAt": None,
            "lastVisit": "2026-01-01T00:00:00.000Z",
        }

    def test_get_list_delegates_to_base():
        repository = _repository()
        repository._get_list = AsyncMock(return_value=[_response])

        assert asyncio.run(repository.get_list(25)) == [_response]
        repository._get_list.assert_awaited_once_with(25)

    def test_create_stores_client_fields():
        repository = _repository()
        repository._write = AsyncMock()

        result = asyncio.run(repository.create(CatCreateRequest(**{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"}), "user-1"))

        record = repository._write.call_args.args[0]
        assert record["createdBy"] == "user-1"
        assert record["tenantId"] == "public"
        assert record["region"] == "eu"
        assert record["priority"] == 1
        assert record["rank"] == 1.5
        assert record["labels"] == []
        assert record["name"] == "sample"
        assert record["breed"] == "tabby"
        assert record["ageYears"] == 0
        assert record["weightKg"] == 1.5
        assert record["indoor"] is True
        assert record["birthDate"] == "2026-01-01"
        assert record["microchipId"] == "6f1c2a3b-4d5e-4f60-8a7b-000000000000"
        assert record["ownerEmail"] == "unknown@example.com"
        assert record["website"] == "https://example.com/items/1"
        assert record["tagCode"] == "ABC-123"
        assert record["tags"] == []
        assert record["scores"] == [1]
        assert record["adoptedAt"] == "2026-01-15T10:00:00.000Z"
        assert record["lastVisit"] == "2026-01-01T00:00:00.000Z"
        assert record["notes"] == "$none"
        assert result.id == record["id"]

    def test_update_passes_only_the_fields_sent():
        repository = _repository()
        repository._update = AsyncMock(return_value=_response)

        assert asyncio.run(repository.update(ITEM_ID, CatUpdateRequest())) == _response
        repository._update.assert_awaited_once_with(ITEM_ID, {}, None)

        asyncio.run(repository.update(ITEM_ID, CatUpdateRequest(**{"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "ageYears": 0, "weightKg": 1.5, "indoor": True, "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tags": [], "adoptedAt": "2026-01-15T10:00:00.000Z", "notes": "$none"}), "user-1"))
        repository._update.assert_awaited_with(ITEM_ID, {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "ageYears": 0, "weightKg": 1.5, "indoor": True, "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tags": [], "adoptedAt": "2026-01-15T10:00:00.000Z", "notes": "$none"}, "user-1")

    def test_replace_passes_every_accepted_field():
        repository = _repository()
        repository._update = AsyncMock(return_value=_response)

        item = CatReplaceRequest(**{"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"})
        assert asyncio.run(repository.replace(ITEM_ID, item, "user-1")) == _response
        repository._update.assert_awaited_once_with(ITEM_ID, {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"}, "user-1")

    def test_delete_delegates_to_base():
        repository = _repository()
        repository._delete = AsyncMock()

        asyncio.run(repository.delete(ITEM_ID, "user-1"))
        repository._delete.assert_awaited_once_with(ITEM_ID, "user-1")
