import asyncio
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from models import (
    Cat,
    CatResponse,
    Dog,
    DogResponse,
    generate_utc_timestamp,
)
from repositories import (
    CatRepository,
    DogRepository,
)
from errors import NotFoundError


class _AsyncIterator:
    """Minimal async-iterable wrapper so mocks can stand in for the
    async iterators returned by the cloud SDKs (Cosmos query_items,
    Firestore stream)."""

    def __init__(self, items):
        self._items = list(items)

    def __aiter__(self):
        self._iter = iter(self._items)
        return self

    async def __anext__(self):
        try:
            return next(self._iter)
        except StopIteration:
            raise StopAsyncIteration

mock_query = [
    {
        "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
        "name": "mockName1",
        "type": "mockType1",
        "isDeleted": False,
        "createdDate": "2024-08-10T20:41:30Z",
        "updatedDate": "2024-08-10T20:41:30Z"
    },
    {
        "id": "de6cbc87-5969-458c-8444-3512a82250bc",
        "name": "mockName2",
        "type": "mockType2",
        "isDeleted": False,
        "createdDate": "2024-08-10T20:41:30Z",
        "updatedDate": "2024-08-10T20:41:30Z"
    }
]
mock_upsert = {
    "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
    "name": "mockName1-Update",
    "type": "mockType1-Update",
    "isDeleted": False,
    "createdDate": "2024-08-10T20:41:30Z",
    "updatedDate": "2024-08-10T20:41:30Z"
}


_cat_responses = [
    CatResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    CatResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_cat_repository():
    @pytest.fixture
    def mock_cosmos_client():
        client = MagicMock()
        client.create_item = AsyncMock()
        client.upsert_item = AsyncMock()
        client.patch_item = AsyncMock()
        return client

    def describe_get_by_id():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator(mock_query)
            repository = CatRepository(mock_cosmos_client)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false",
                parameters=[{"name": "@id", "value": "ac1df01c-7ece-4a20-ab60-179829dad8f5"}]
            )
            assert result == _cat_responses[0]

        def test_not_found_error(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator([])
            repository = CatRepository(mock_cosmos_client)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

    def describe_get_list():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator(mock_query)
            repository = CatRepository(mock_cosmos_client)
            result = asyncio.run(repository.get_list())
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100"
            )
            assert result == _cat_responses

        def test_successfully_call_empty_result(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator([])
            repository = CatRepository(mock_cosmos_client)
            result = asyncio.run(repository.get_list())
            assert result == []

    def describe_create():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.create_item.return_value = mock_query[0]
            repository = CatRepository(mock_cosmos_client)
            mock_item = Cat(**mock_query[0])
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(repository.create(item=mock_item))
            mock_cosmos_client.create_item.assert_called_once_with(mock_query[0])
            assert result == _cat_responses[0]

    def describe_update():
        def test_successfully_call(mock_cosmos_client):
            mock_item_response = CatResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = CatRepository(mock_cosmos_client)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.update(
                    item=Cat(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                repository.get_by_id.assert_called_once()
                mock_cosmos_client.upsert_item.assert_called_once()
                assert result.id == 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
                assert result.name == 'mockName1-Update'
                assert result.type == 'mockType1-Update'

    def describe_replace():
        def test_successfully_call(mock_cosmos_client):
            mock_item_response = CatResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = CatRepository(mock_cosmos_client)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item=Cat(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                repository.get_by_id.assert_called_once()
                mock_cosmos_client.upsert_item.assert_called_once()
                assert result.name == 'mockName1-Update'

    def describe_delete():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.patch_item.return_value = mock_query[0]
            repository = CatRepository(mock_cosmos_client)
            asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_cosmos_client.patch_item.assert_called_once_with(
                item='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                partition_key='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                patch_operations=[{ 'op': 'replace', 'path': '/isDeleted', 'value': True }],
                filter_predicate='from c WHERE c.isDeleted = false'
            )

_dog_responses = [
    DogResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    DogResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_dog_repository():
    @pytest.fixture
    def mock_cosmos_client():
        client = MagicMock()
        client.create_item = AsyncMock()
        client.upsert_item = AsyncMock()
        client.patch_item = AsyncMock()
        return client

    def describe_get_by_id():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator(mock_query)
            repository = DogRepository(mock_cosmos_client)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false",
                parameters=[{"name": "@id", "value": "ac1df01c-7ece-4a20-ab60-179829dad8f5"}]
            )
            assert result == _dog_responses[0]

        def test_not_found_error(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator([])
            repository = DogRepository(mock_cosmos_client)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

    def describe_get_list():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator(mock_query)
            repository = DogRepository(mock_cosmos_client)
            result = asyncio.run(repository.get_list())
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100"
            )
            assert result == _dog_responses

        def test_successfully_call_empty_result(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator([])
            repository = DogRepository(mock_cosmos_client)
            result = asyncio.run(repository.get_list())
            assert result == []

    def describe_create():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.create_item.return_value = mock_query[0]
            repository = DogRepository(mock_cosmos_client)
            mock_item = Dog(**mock_query[0])
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(repository.create(item=mock_item))
            mock_cosmos_client.create_item.assert_called_once_with(mock_query[0])
            assert result == _dog_responses[0]

    def describe_update():
        def test_successfully_call(mock_cosmos_client):
            mock_item_response = DogResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = DogRepository(mock_cosmos_client)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.update(
                    item=Dog(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                repository.get_by_id.assert_called_once()
                mock_cosmos_client.upsert_item.assert_called_once()
                assert result.id == 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
                assert result.name == 'mockName1-Update'
                assert result.type == 'mockType1-Update'

    def describe_replace():
        def test_successfully_call(mock_cosmos_client):
            mock_item_response = DogResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = DogRepository(mock_cosmos_client)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item=Dog(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                repository.get_by_id.assert_called_once()
                mock_cosmos_client.upsert_item.assert_called_once()
                assert result.name == 'mockName1-Update'

    def describe_delete():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.patch_item.return_value = mock_query[0]
            repository = DogRepository(mock_cosmos_client)
            asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_cosmos_client.patch_item.assert_called_once_with(
                item='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                partition_key='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                patch_operations=[{ 'op': 'replace', 'path': '/isDeleted', 'value': True }],
                filter_predicate='from c WHERE c.isDeleted = false'
            )


