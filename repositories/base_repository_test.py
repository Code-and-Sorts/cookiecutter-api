import asyncio
import uuid
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from botocore.exceptions import ClientError
from models import BaseResponse
from repositories import BaseRepository
from repositories.base_repository import DYNAMODB_CONFIG
from errors import NotFoundError
from conftest import ITEM_ID


class _ItemResponse(BaseResponse):
    name: str


class _ItemRepository(BaseRepository[_ItemResponse]):
    resource_name = "Item"
    response_model = _ItemResponse


_TIMESTAMP = "repositories.base_repository.generate_utc_timestamp"
_NOW = "2026-01-01T00:00:00.000Z"
_CREATED = "2024-08-10T20:41:30.123Z"
_ID2 = "de6cbc87-5969-458c-8444-3512a82250bc"
_stored_item = {
    "id": ITEM_ID,
    "name": "mockName1",
    "isDeleted": False,
    "createdTimestamp": _CREATED,
    "updatedTimestamp": _CREATED,
    "createdBy": "creator",
}
_responses = [
    _ItemResponse(id=ITEM_ID, name="mockName1", createdTimestamp=_CREATED, createdBy="creator", updatedTimestamp=_CREATED),
    _ItemResponse(id=_ID2, name="mockName2", createdTimestamp=_CREATED, createdBy="creator", updatedTimestamp=_CREATED),
]


class _AsyncIterator:
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
    session = MagicMock()
    dynamodb = MagicMock()
    dynamodb.Table = AsyncMock(return_value=table_mock)
    cm = MagicMock()
    cm.__aenter__ = AsyncMock(return_value=dynamodb)
    cm.__aexit__ = AsyncMock(return_value=None)
    session.resource = MagicMock(return_value=cm)
    return session


def _offline_repository(stored=None):
    repository = _ItemRepository(MagicMock(), "items", "us-east-1")
    if stored is None:
        repository._get_stored = AsyncMock(side_effect=NotFoundError("Item with id x was not found."))
    else:
        repository._get_stored = AsyncMock(return_value=dict(stored))
    repository._write = AsyncMock()
    return repository


def _written(repository) -> dict:
    repository._write.assert_awaited_once()
    return repository._write.call_args.args[0]


def describe_base_repository_records():
    def describe_create():
        def test_stores_new_record_with_server_id_and_one_timestamp():
            repository = _offline_repository()
            # A second clock reading would differ, so both fields equal _NOW proves one reading.
            ticks = iter([_NOW, "2099-01-01T00:00:00.000Z"])
            with patch(_TIMESTAMP, side_effect=lambda: next(ticks)):
                result = asyncio.run(repository._create({"name": "mockName1"}))

            record = _written(repository)
            assert str(uuid.UUID(record["id"])) == record["id"]
            assert record == {
                "id": record["id"],
                "name": "mockName1",
                "isDeleted": False,
                "createdTimestamp": _NOW,
                "updatedTimestamp": _NOW,
            }
            assert result.model_dump() == {
                "id": record["id"],
                "name": "mockName1",
                "createdTimestamp": _NOW,
                "updatedTimestamp": _NOW,
            }

        def test_user_id_sets_created_and_updated_by():
            repository = _offline_repository()
            result = asyncio.run(repository._create({"name": "mockName1"}, "editor"))

            assert _written(repository)["createdBy"] == _written(repository)["updatedBy"] == "editor"
            assert result.createdBy == result.updatedBy == "editor"

        def test_generates_a_new_id_each_time():
            repository = _offline_repository()
            asyncio.run(repository._create({"name": "a"}))
            asyncio.run(repository._create({"name": "b"}))
            first, second = (call.args[0]["id"] for call in repository._write.call_args_list)
            assert first != second

    def describe_update():
        def test_merges_changes_and_keeps_creation_fields():
            repository = _offline_repository({**_stored_item, "extra": "kept", "updatedBy": "someone"})
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository._update(ITEM_ID, {"name": "mockName1-Update"}, "editor"))

            assert _written(repository) == {
                **_stored_item,
                "extra": "kept",
                "name": "mockName1-Update",
                "updatedTimestamp": _NOW,
                "updatedBy": "editor",
            }
            assert repository._write.call_args.args[1] is repository._get_stored.return_value
            assert result.model_dump() == {
                "id": ITEM_ID,
                "name": "mockName1-Update",
                "createdTimestamp": _CREATED,
                "createdBy": "creator",
                "updatedTimestamp": _NOW,
                "updatedBy": "editor",
            }

        def test_no_changes_or_user_id_refreshes_timestamp_and_drops_updated_by():
            repository = _offline_repository({**_stored_item, "updatedBy": "someone"})
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository._update(ITEM_ID, {}))

            assert _written(repository) == {**_stored_item, "updatedTimestamp": _NOW}
            assert "updatedBy" not in result.model_dump()

        def test_not_found_error():
            repository = _offline_repository()
            with pytest.raises(NotFoundError):
                asyncio.run(repository._update(ITEM_ID, {"name": "mockName1-Update"}))
            repository._write.assert_not_called()

    def describe_replace():
        def test_overwrites_fields_and_keeps_creation_fields():
            repository = _offline_repository({**_stored_item, "extra": "dropped", "updatedBy": "someone"})
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository._replace(ITEM_ID, {"name": "mockName1-Replace"}, "editor"))

            assert _written(repository) == {
                "id": ITEM_ID,
                "name": "mockName1-Replace",
                "isDeleted": False,
                "createdTimestamp": _CREATED,
                "createdBy": "creator",
                "updatedTimestamp": _NOW,
                "updatedBy": "editor",
            }
            assert repository._write.call_args.args[1] is repository._get_stored.return_value
            assert result.model_dump() == {
                "id": ITEM_ID,
                "name": "mockName1-Replace",
                "createdTimestamp": _CREATED,
                "createdBy": "creator",
                "updatedTimestamp": _NOW,
                "updatedBy": "editor",
            }

        def test_omits_unset_created_by_and_drops_updated_by_without_user_id():
            stored = {key: value for key, value in _stored_item.items() if key != "createdBy"}
            repository = _offline_repository({**stored, "updatedBy": "someone"})
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository._replace(ITEM_ID, {"name": "mockName1-Replace"}))

            assert not {"createdBy", "updatedBy"} & set(_written(repository))
            assert not {"createdBy", "updatedBy"} & set(result.model_dump())

        def test_not_found_error():
            repository = _offline_repository()
            with pytest.raises(NotFoundError):
                asyncio.run(repository._replace(ITEM_ID, {"name": "mockName1-Replace"}))
            repository._write.assert_not_called()


def describe_dynamodb_storage():
    @pytest.fixture
    def table():
        table = MagicMock()
        table.get_item = AsyncMock()
        table.scan = AsyncMock()
        table.put_item = AsyncMock()
        table.update_item = AsyncMock()
        return table

    def _repository(table):
        return _ItemRepository(_make_session(table), "items", "us-east-1")

    def test_calls_are_bounded(table):
        repository = _repository(table)
        asyncio.run(repository._write(_stored_item))

        repository.session.resource.assert_called_once_with("dynamodb", region_name="us-east-1", config=DYNAMODB_CONFIG)
        assert DYNAMODB_CONFIG.connect_timeout <= 1
        assert DYNAMODB_CONFIG.read_timeout <= 2
        assert DYNAMODB_CONFIG.retries == {"total_max_attempts": 2, "mode": "standard"}

    def describe_get_stored():
        def test_reads_item(table):
            table.get_item.return_value = {"Item": dict(_stored_item)}
            result = asyncio.run(_repository(table)._get_by_id(ITEM_ID))

            table.get_item.assert_awaited_once_with(Key={"id": ITEM_ID})
            assert result == _responses[0]

        @pytest.mark.parametrize("response", [{}, {"Item": {**_stored_item, "isDeleted": True}}])
        def test_missing_or_deleted_is_not_found(table, response):
            table.get_item.return_value = response
            with pytest.raises(NotFoundError) as error:
                asyncio.run(_repository(table)._get_by_id(ITEM_ID))
            assert str(error.value) == f"Item with id {ITEM_ID} was not found."

    def describe_write():
        def test_puts_item(table):
            asyncio.run(_repository(table)._write(_stored_item))
            table.put_item.assert_awaited_once_with(Item=_stored_item)

    def describe_get_list():
        def test_scans_undeleted_items_with_limit(table):
            table.scan.return_value = {"Items": [
                dict(_stored_item),
                {**_stored_item, "id": _ID2, "name": "mockName2"},
            ]}
            result = asyncio.run(_repository(table)._get_list(2))

            assert table.scan.await_args.kwargs["Limit"] == 2
            assert result == _responses

        def test_reads_more_pages_until_limit(table):
            table.scan.side_effect = [
                {"Items": [dict(_stored_item)], "LastEvaluatedKey": {"id": ITEM_ID}},
                {"Items": [
                    {**_stored_item, "id": _ID2, "name": "mockName2"},
                    {**_stored_item, "id": "third", "name": "mockName3"},
                ], "LastEvaluatedKey": {"id": "third"}},
            ]
            result = asyncio.run(_repository(table)._get_list(2))

            assert table.scan.await_count == 2
            assert table.scan.await_args.kwargs["ExclusiveStartKey"] == {"id": ITEM_ID}
            assert result == _responses

        def test_empty_result(table):
            table.scan.return_value = {"Items": []}
            assert asyncio.run(_repository(table)._get_list(100)) == []

    def describe_delete():
        @pytest.mark.parametrize("user_id, update, user_values", [
            ("editor", "SET isDeleted = :val, updatedTimestamp = :updated, updatedBy = :user", {":user": "editor"}),
            (None, "SET isDeleted = :val, updatedTimestamp = :updated REMOVE updatedBy", {}),
        ])
        def test_flags_item_deleted(table, user_id, update, user_values):
            with patch(_TIMESTAMP, return_value=_NOW):
                asyncio.run(_repository(table)._delete(ITEM_ID, user_id))

            table.update_item.assert_awaited_once_with(
                Key={"id": ITEM_ID},
                UpdateExpression=update,
                ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                ExpressionAttributeValues={":val": True, ":false": False, ":updated": _NOW, **user_values}
            )

        def test_missing_or_deleted_is_not_found(table):
            table.update_item.side_effect = ClientError(
                {"Error": {"Code": "ConditionalCheckFailedException", "Message": "failed"}}, "UpdateItem"
            )
            with pytest.raises(NotFoundError):
                asyncio.run(_repository(table)._delete(ITEM_ID))

        def test_other_errors_propagate(table):
            table.update_item.side_effect = ClientError(
                {"Error": {"Code": "ProvisionedThroughputExceededException", "Message": "slow down"}}, "UpdateItem"
            )
            with pytest.raises(ClientError):
                asyncio.run(_repository(table)._delete(ITEM_ID))
