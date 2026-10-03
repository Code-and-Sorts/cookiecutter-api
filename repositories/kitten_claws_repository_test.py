import asyncio
from unittest.mock import AsyncMock, MagicMock
from models import KittenClawsEntity, KittenClawsCreateRequest, KittenClawsUpdateRequest, KittenClawsResponse
from repositories import BaseRepository, KittenClawsRepository
from conftest import ITEM_ID

_sample = {"name": "sample"}
_response = KittenClawsResponse(id=ITEM_ID, **{"name": "sample"})


def _repository() -> KittenClawsRepository:
    return KittenClawsRepository(MagicMock(), "kittenclaws", "us-east-1")


def describe_kitten_claws_repository():
    def test_extends_base_repository_with_kitten_claws_models():
        assert issubclass(KittenClawsRepository, BaseRepository)
        assert KittenClawsRepository.entity_model is KittenClawsEntity
        assert KittenClawsRepository.response_model is KittenClawsResponse
        assert KittenClawsRepository.resource_name == "KittenClaws"

    def test_entity_lists_every_client_field():
        assert KittenClawsEntity.client_fields() == ["name"]

    def test_exposes_only_kitten_claws_operations():
        for method in ['replace']:
            assert not hasattr(KittenClawsRepository, method)

    def test_get_by_id_returns_kitten_claws_response():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, **_sample, "isDeleted": False})

        result = asyncio.run(repository.get_by_id(ITEM_ID))

        assert isinstance(result, KittenClawsResponse)
        assert result == _response

    def test_a_record_without_a_field_reads_its_static_default():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, "isDeleted": False})

        result = asyncio.run(repository.get_by_id(ITEM_ID)).model_dump()

        assert result == {
            "id": ITEM_ID,
            "name": None,
        }

    def test_get_list_delegates_to_base():
        repository = _repository()
        repository._get_list = AsyncMock(return_value=[_response])

        assert asyncio.run(repository.get_list(25)) == [_response]
        repository._get_list.assert_awaited_once_with(25)

    def test_create_stores_client_fields():
        repository = _repository()
        repository._write = AsyncMock()

        result = asyncio.run(repository.create(KittenClawsCreateRequest(**{"name": "sample"}), "user-1"))

        record = repository._write.call_args.args[0]
        assert record["createdBy"] == "user-1"
        assert record["name"] == "sample"
        assert result.id == record["id"]

    def test_update_passes_only_the_fields_sent():
        repository = _repository()
        repository._update = AsyncMock(return_value=_response)

        assert asyncio.run(repository.update(ITEM_ID, KittenClawsUpdateRequest())) == _response
        repository._update.assert_awaited_once_with(ITEM_ID, {}, None)

        asyncio.run(repository.update(ITEM_ID, KittenClawsUpdateRequest(**{"name": "sample"}), "user-1"))
        repository._update.assert_awaited_with(ITEM_ID, {"name": "sample"}, "user-1")

    def test_delete_delegates_to_base():
        repository = _repository()
        repository._delete = AsyncMock()

        asyncio.run(repository.delete(ITEM_ID, "user-1"))
        repository._delete.assert_awaited_once_with(ITEM_ID, "user-1")
