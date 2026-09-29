import asyncio
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from models import (
    Cat,
    CatResponse,
    Dog,
    DogResponse,
)
from repositories import (
    CatRepository,
    DogRepository,
)
from errors import NotFoundError

_TIMESTAMP = "repositories.repository.generate_utc_timestamp"
_NOW = "2026-01-01T00:00:00Z"
_ID = "ac1df01c-7ece-4a20-ab60-179829dad8f5"
_stored_item = {
    "id": _ID,
    "name": "mockName1",
    "type": "mockType1",
    "isDeleted": False,
    "createdDate": "2024-08-10T20:41:30Z",
    "updatedDate": "2024-08-10T20:41:30Z"
}
_replacement = {
    "id": _ID,
    "name": "mockName1-Replace",
    "isDeleted": True,
    "createdDate": "1999-01-01T00:00:00Z"
}
_expected_replacement = {
    "id": _ID,
    "name": "mockName1-Replace",
    "isDeleted": False,
    "createdDate": "2024-08-10T20:41:30Z",
    "updatedDate": _NOW
}


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



def _make_session(table_mock):
    """Build a mock aioboto3 Session whose ``resource(...)`` async context
    manager yields a DynamoDB resource exposing the given table mock."""
    session = MagicMock()
    dynamodb = MagicMock()
    dynamodb.Table = AsyncMock(return_value=table_mock)
    cm = MagicMock()
    cm.__aenter__ = AsyncMock(return_value=dynamodb)
    cm.__aexit__ = AsyncMock(return_value=None)
    session.resource = MagicMock(return_value=cm)
    return session


_cat_responses = [
    CatResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    CatResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_cat_repository():
    @pytest.fixture
    def mock_dynamodb_table():
        table = MagicMock()
        table.get_item = AsyncMock()
        table.scan = AsyncMock()
        table.put_item = AsyncMock()
        table.update_item = AsyncMock()
        return table

    def _repository(table):
        return CatRepository(_make_session(table), "animals", "us-east-1")

    def describe_get_by_id():
        def test_successfully_call(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {
                "Item": {
                    "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                    "name": "mockName1",
                    "type": "mockType1",
                    "isDeleted": False
                }
            }

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_dynamodb_table.get_item.assert_called_once_with(Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"})
            assert result == _cat_responses[0]

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {}

            repository = _repository(mock_dynamodb_table)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

    def describe_get_list():
        def test_successfully_call(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {
                "Items": [
                    {
                        "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                        "name": "mockName1",
                        "type": "mockType1",
                        "isDeleted": False
                    },
                    {
                        "id": "de6cbc87-5969-458c-8444-3512a82250bc",
                        "name": "mockName2",
                        "type": "mockType2",
                        "isDeleted": False
                    }
                ]
            }

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_list())

            mock_dynamodb_table.scan.assert_called_once()
            assert result == _cat_responses

        def test_successfully_call_empty_result(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {"Items": []}

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_list())

            mock_dynamodb_table.scan.assert_called_once()
            assert result == []

    def describe_create():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            mock_item = Cat(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.create(item=mock_item))

            mock_dynamodb_table.put_item.assert_called_once_with(Item={
                "id": _ID,
                "name": "mockName1",
                "type": "mockType1",
                "isDeleted": False,
                "createdDate": _NOW,
                "updatedDate": _NOW,
            })
            assert result == _cat_responses[0]

    def describe_update():
        def test_merges_changes_into_stored_item(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {"Item": dict(_stored_item)}

            repository = _repository(mock_dynamodb_table)
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.update(item=Cat(id=_ID, name='mockName1-Update')))

            mock_dynamodb_table.put_item.assert_called_once_with(
                Item={**_stored_item, "name": "mockName1-Update", "updatedDate": _NOW}
            )
            assert result == CatResponse(id=_ID, name='mockName1-Update', type='mockType1')

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {}

            repository = _repository(mock_dynamodb_table)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.update(item=Cat(id=_ID, name='mockName1-Update')))
            mock_dynamodb_table.put_item.assert_not_called()

    def describe_replace():
        def test_overwrites_item_and_keeps_created_date(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {"Item": dict(_stored_item)}

            repository = _repository(mock_dynamodb_table)
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.replace(item=Cat(**_replacement)))

            mock_dynamodb_table.put_item.assert_called_once_with(Item=_expected_replacement)
            assert result == CatResponse(id=_ID, name='mockName1-Replace')

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {"Item": {**_stored_item, "isDeleted": True}}

            repository = _repository(mock_dynamodb_table)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.replace(item=Cat(**_replacement)))
            mock_dynamodb_table.put_item.assert_not_called()

    def describe_delete():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            with patch(_TIMESTAMP, return_value=_NOW):
                asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_dynamodb_table.update_item.assert_called_once_with(
                Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"},
                UpdateExpression="SET isDeleted = :val, updatedDate = :updated",
                ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                ExpressionAttributeValues={":val": True, ":false": False, ":updated": _NOW}
            )

_dog_responses = [
    DogResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    DogResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_dog_repository():
    @pytest.fixture
    def mock_dynamodb_table():
        table = MagicMock()
        table.get_item = AsyncMock()
        table.scan = AsyncMock()
        table.put_item = AsyncMock()
        table.update_item = AsyncMock()
        return table

    def _repository(table):
        return DogRepository(_make_session(table), "animals", "us-east-1")

    def describe_get_by_id():
        def test_successfully_call(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {
                "Item": {
                    "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                    "name": "mockName1",
                    "type": "mockType1",
                    "isDeleted": False
                }
            }

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_dynamodb_table.get_item.assert_called_once_with(Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"})
            assert result == _dog_responses[0]

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {}

            repository = _repository(mock_dynamodb_table)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

    def describe_get_list():
        def test_successfully_call(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {
                "Items": [
                    {
                        "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                        "name": "mockName1",
                        "type": "mockType1",
                        "isDeleted": False
                    },
                    {
                        "id": "de6cbc87-5969-458c-8444-3512a82250bc",
                        "name": "mockName2",
                        "type": "mockType2",
                        "isDeleted": False
                    }
                ]
            }

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_list())

            mock_dynamodb_table.scan.assert_called_once()
            assert result == _dog_responses

        def test_successfully_call_empty_result(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {"Items": []}

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_list())

            mock_dynamodb_table.scan.assert_called_once()
            assert result == []

    def describe_create():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            mock_item = Dog(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.create(item=mock_item))

            mock_dynamodb_table.put_item.assert_called_once_with(Item={
                "id": _ID,
                "name": "mockName1",
                "type": "mockType1",
                "isDeleted": False,
                "createdDate": _NOW,
                "updatedDate": _NOW,
            })
            assert result == _dog_responses[0]

    def describe_update():
        def test_merges_changes_into_stored_item(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {"Item": dict(_stored_item)}

            repository = _repository(mock_dynamodb_table)
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.update(item=Dog(id=_ID, name='mockName1-Update')))

            mock_dynamodb_table.put_item.assert_called_once_with(
                Item={**_stored_item, "name": "mockName1-Update", "updatedDate": _NOW}
            )
            assert result == DogResponse(id=_ID, name='mockName1-Update', type='mockType1')

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {}

            repository = _repository(mock_dynamodb_table)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.update(item=Dog(id=_ID, name='mockName1-Update')))
            mock_dynamodb_table.put_item.assert_not_called()

    def describe_replace():
        def test_overwrites_item_and_keeps_created_date(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {"Item": dict(_stored_item)}

            repository = _repository(mock_dynamodb_table)
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.replace(item=Dog(**_replacement)))

            mock_dynamodb_table.put_item.assert_called_once_with(Item=_expected_replacement)
            assert result == DogResponse(id=_ID, name='mockName1-Replace')

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {"Item": {**_stored_item, "isDeleted": True}}

            repository = _repository(mock_dynamodb_table)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.replace(item=Dog(**_replacement)))
            mock_dynamodb_table.put_item.assert_not_called()

    def describe_delete():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            with patch(_TIMESTAMP, return_value=_NOW):
                asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_dynamodb_table.update_item.assert_called_once_with(
                Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"},
                UpdateExpression="SET isDeleted = :val, updatedDate = :updated",
                ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                ExpressionAttributeValues={":val": True, ":false": False, ":updated": _NOW}
            )
