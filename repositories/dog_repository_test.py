import asyncio
from unittest.mock import AsyncMock, MagicMock
from models import BaseDog, DogResponse
from repositories import BaseRepository, DogRepository
from conftest import ITEM_ID, TIMESTAMPS

_response = DogResponse(id=ITEM_ID, name="mockName1", **TIMESTAMPS)


def _repository() -> DogRepository:
    return DogRepository(MagicMock())


def describe_dog_repository():
    def test_extends_base_repository_with_dog_models():
        assert issubclass(DogRepository, BaseRepository)
        assert DogRepository.response_model is DogResponse
        assert DogRepository.resource_name == "Dog"

    def test_exposes_only_dog_operations():
        for method in ['update']:
            assert not hasattr(DogRepository, method)

    def test_get_by_id_returns_dog_response():
        repository = _repository()
        repository._get_stored = AsyncMock(return_value={"id": ITEM_ID, "name": "mockName1", "isDeleted": False, **TIMESTAMPS})

        result = asyncio.run(repository.get_by_id(ITEM_ID))

        assert isinstance(result, DogResponse)
        assert result == _response

    def test_get_list_delegates_to_base():
        repository = _repository()
        repository._get_list = AsyncMock(return_value=[_response])

        assert asyncio.run(repository.get_list(25)) == [_response]
        repository._get_list.assert_awaited_once_with(25)

    def test_create_stores_client_fields():
        repository = _repository()
        repository._write = AsyncMock()

        result = asyncio.run(repository.create(BaseDog(name="mockName1"), "user-1"))

        record = repository._write.call_args.args[0]
        assert (record["name"], record["createdBy"]) == ("mockName1", "user-1")
        assert result == DogResponse.model_validate(record)

    def test_replace_passes_all_client_fields():
        repository = _repository()
        repository._replace = AsyncMock(return_value=_response)

        assert asyncio.run(repository.replace(ITEM_ID, BaseDog(name="mockName1"), "user-1")) == _response
        repository._replace.assert_awaited_once_with(ITEM_ID, {"name": "mockName1"}, "user-1")

    def test_delete_delegates_to_base():
        repository = _repository()
        repository._delete = AsyncMock()

        asyncio.run(repository.delete(ITEM_ID, "user-1"))
        repository._delete.assert_awaited_once_with(ITEM_ID, "user-1")
