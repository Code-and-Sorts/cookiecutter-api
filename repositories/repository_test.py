import asyncio
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from models import (
    KittenClaws,
    KittenClawsResponse,
    generate_utc_timestamp,
)
from repositories import (
    KittenClawsRepository,
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


_kitten_claws_responses = [
    KittenClawsResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    KittenClawsResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_kitten_claws_repository():
    @pytest.fixture
    def mock_dynamodb_table():
        table = MagicMock()
        table.get_item = AsyncMock()
        table.scan = AsyncMock()
        table.put_item = AsyncMock()
        table.update_item = AsyncMock()
        return table

    def _repository(table):
        return KittenClawsRepository(_make_session(table), "kitty_cats", "us-east-1")

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
            assert result == _kitten_claws_responses[0]

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
            assert result == _kitten_claws_responses

        def test_successfully_call_empty_result(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {"Items": []}

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_list())

            mock_dynamodb_table.scan.assert_called_once()
            assert result == []

    def describe_create():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            mock_item = KittenClaws(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            result = asyncio.run(repository.create(item=mock_item))

            mock_dynamodb_table.put_item.assert_called_once()
            assert result.id == _kitten_claws_responses[0].id

    def describe_update():
        def test_successfully_call(mock_dynamodb_table):
            mock_item_response = KittenClawsResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )

            repository = _repository(mock_dynamodb_table)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.update(
                    item=KittenClaws(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                assert result.id == 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
                assert result.name == 'mockName1-Update'
                assert result.type == 'mockType1-Update'

    def describe_replace():
        def test_successfully_call(mock_dynamodb_table):
            mock_item_response = KittenClawsResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            repository = _repository(mock_dynamodb_table)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item=KittenClaws(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                mock_dynamodb_table.put_item.assert_called_once()
                assert result.name == 'mockName1-Update'

    def describe_delete():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_dynamodb_table.update_item.assert_called_once_with(
                Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"},
                UpdateExpression="SET isDeleted = :val",
                ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                ExpressionAttributeValues={":val": True, ":false": False}
            )
