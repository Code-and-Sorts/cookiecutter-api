import asyncio
from unittest.mock import AsyncMock, MagicMock
from models import BaseCat, CatUpdate, CatResponse
from repositories import BaseRepository, CatRepository
from conftest import ITEM_ID, TIMESTAMPS

_response = CatResponse(id=ITEM_ID, name="mockName1", **TIMESTAMPS)


def _repository() -> CatRepository:
    return CatRepository(MagicMock(), "animals", "us-east-1")


def describe_cat_repository():
    def test_extends_base_repository_with_cat_models():
        assert issubclass(CatRepository, BaseRepository)
        assert CatRepository.response_model is CatResponse
        assert CatRepository.resource_name == "Cat"

    def test_exposes_only_cat_operations():
        for method in ['replace']:
            assert not hasattr(CatRepository, method)

    def test_get_by_id_returns_cat_response():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, "name": "mockName1", "isDeleted": False, **TIMESTAMPS})

        result = asyncio.run(repository.get_by_id(ITEM_ID))

        assert isinstance(result, CatResponse)
        assert result == _response

    def test_get_list_delegates_to_base():
        repository = _repository()
        repository._get_list = AsyncMock(return_value=[_response])

        assert asyncio.run(repository.get_list(25)) == [_response]
        repository._get_list.assert_awaited_once_with(25)

    def test_create_stores_client_fields():
        repository = _repository()
        repository._write = AsyncMock()

        result = asyncio.run(repository.create(BaseCat(name="mockName1"), "user-1"))

        record = repository._write.call_args.args[0]
        assert (record["name"], record["createdBy"]) == ("mockName1", "user-1")
        assert result == CatResponse.model_validate(record)

    def test_update_passes_only_the_fields_sent():
        repository = _repository()
        repository._update = AsyncMock(return_value=_response)

        assert asyncio.run(repository.update(ITEM_ID, CatUpdate())) == _response
        repository._update.assert_awaited_once_with(ITEM_ID, {}, None)

        asyncio.run(repository.update(ITEM_ID, CatUpdate(name="mockName1"), "user-1"))
        repository._update.assert_awaited_with(ITEM_ID, {"name": "mockName1"}, "user-1")

    def test_delete_delegates_to_base():
        repository = _repository()
        repository._delete = AsyncMock()

        asyncio.run(repository.delete(ITEM_ID, "user-1"))
        repository._delete.assert_awaited_once_with(ITEM_ID, "user-1")
