import asyncio
from unittest.mock import AsyncMock, MagicMock
from models import VisitEntity, VisitCreateRequest, VisitUpdateRequest, VisitResponse
from repositories import BaseRepository, VisitRepository
from conftest import ITEM_ID

_sample = {"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]}
_response = VisitResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]})


def _repository() -> VisitRepository:
    return VisitRepository(MagicMock())


def describe_visit_repository():
    def test_extends_base_repository_with_visit_models():
        assert issubclass(VisitRepository, BaseRepository)
        assert VisitRepository.entity_model is VisitEntity
        assert VisitRepository.response_model is VisitResponse
        assert VisitRepository.resource_name == "Visit"

    def test_entity_lists_every_client_field():
        assert VisitEntity.client_fields() == ["tenantId", "region", "priority", "rank", "labels", "reason", "visitedOn", "cost", "paid", "checkedAt"]

    def test_exposes_only_visit_operations():
        for method in ['get_list', 'replace', 'delete']:
            assert not hasattr(VisitRepository, method)

    def test_get_by_id_returns_visit_response():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, **_sample, "isDeleted": False})

        result = asyncio.run(repository.get_by_id(ITEM_ID))

        assert isinstance(result, VisitResponse)
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
            "reason": None,
            "visitedOn": None,
            "cost": None,
            "paid": False,
            "checkedAt": None,
        }

    def test_create_stores_client_fields():
        repository = _repository()
        repository._write = AsyncMock()

        result = asyncio.run(repository.create(VisitCreateRequest(**{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5}), "user-1"))

        record = repository._write.call_args.args[0]
        assert record["createdBy"] == "user-1"
        assert record["tenantId"] == "public"
        assert record["region"] == "eu"
        assert record["priority"] == 1
        assert record["rank"] == 1.5
        assert record["labels"] == []
        assert record["reason"] == "sample"
        assert record["visitedOn"] == "2026-01-01"
        assert record["cost"] == 1.5
        assert record["paid"] is False
        assert result.id == record["id"]

    def test_update_passes_only_the_fields_sent():
        repository = _repository()
        repository._update = AsyncMock(return_value=_response)

        assert asyncio.run(repository.update(ITEM_ID, VisitUpdateRequest())) == _response
        repository._update.assert_awaited_once_with(ITEM_ID, {}, None)

        asyncio.run(repository.update(ITEM_ID, VisitUpdateRequest(**{"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]}), "user-1"))
        repository._update.assert_awaited_with(ITEM_ID, {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]}, "user-1")
