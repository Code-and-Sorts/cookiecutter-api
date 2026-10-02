import asyncio
from unittest.mock import AsyncMock, MagicMock
from models import BaseKittenClaws, KittenClawsUpdate, KittenClawsResponse
from repositories import BaseRepository, KittenClawsRepository
from conftest import ITEM_ID

_response = KittenClawsResponse(id=ITEM_ID, name="mockName1")


def _repository() -> KittenClawsRepository:
    return KittenClawsRepository(MagicMock(), "kittenclaws", "us-east-1")


def describe_kitten_claws_repository():
    def test_extends_base_repository_with_kitten_claws_models():
        assert issubclass(KittenClawsRepository, BaseRepository)
        assert KittenClawsRepository.response_model is KittenClawsResponse
        assert KittenClawsRepository.resource_name == "KittenClaws"

    def test_exposes_only_kitten_claws_operations():
        for method in ['replace']:
            assert not hasattr(KittenClawsRepository, method)

    def test_get_by_id_returns_kitten_claws_response():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, "name": "mockName1", "isDeleted": False})

        result = asyncio.run(repository.get_by_id(ITEM_ID))

        assert isinstance(result, KittenClawsResponse)
        assert result == _response

    def test_get_list_delegates_to_base():
        repository = _repository()
        repository._get_list = AsyncMock(return_value=[_response])

        assert asyncio.run(repository.get_list(25)) == [_response]
        repository._get_list.assert_awaited_once_with(25)

    def test_create_stores_client_fields():
        repository = _repository()
        repository._write = AsyncMock()

        result = asyncio.run(repository.create(BaseKittenClaws(name="mockName1"), "user-1"))

        record = repository._write.call_args.args[0]
        assert (record["name"], record["createdBy"]) == ("mockName1", "user-1")
        assert result == KittenClawsResponse(id=record["id"], name="mockName1")

    def test_update_passes_only_the_fields_sent():
        repository = _repository()
        repository._update = AsyncMock(return_value=_response)

        assert asyncio.run(repository.update(ITEM_ID, KittenClawsUpdate())) == _response
        repository._update.assert_awaited_once_with(ITEM_ID, {}, None)

        asyncio.run(repository.update(ITEM_ID, KittenClawsUpdate(name="mockName1"), "user-1"))
        repository._update.assert_awaited_with(ITEM_ID, {"name": "mockName1"}, "user-1")

    def test_delete_delegates_to_base():
        repository = _repository()
        repository._delete = AsyncMock()

        asyncio.run(repository.delete(ITEM_ID, "user-1"))
        repository._delete.assert_awaited_once_with(ITEM_ID, "user-1")
