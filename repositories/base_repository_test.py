import asyncio
import uuid
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from azure.core import MatchConditions
from azure.cosmos.exceptions import (
    CosmosAccessConditionFailedError,
    CosmosHttpResponseError,
    CosmosResourceNotFoundError,
)
from models import BaseResponse
from repositories import BaseRepository
from errors import NotFoundError
from utils.detect_error import detect_error
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


def _offline_repository(stored=None):
    repository = _ItemRepository(MagicMock())
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


def describe_cosmos_storage():
    @pytest.fixture
    def container():
        client = MagicMock()
        client.upsert_item = AsyncMock()
        client.replace_item = AsyncMock()
        client.patch_item = AsyncMock()
        client.read = AsyncMock(return_value={"id": "items"})
        return client

    def describe_get_stored():
        def test_queries_undeleted_item(container):
            container.query_items.return_value = _AsyncIterator([_stored_item])
            result = asyncio.run(_ItemRepository(container)._get_by_id(ITEM_ID))

            container.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false",
                parameters=[{"name": "@id", "value": ITEM_ID}]
            )
            assert result == _responses[0]

        def test_not_found_error(container):
            container.query_items.return_value = _AsyncIterator([])
            with pytest.raises(NotFoundError) as error:
                asyncio.run(_ItemRepository(container)._get_by_id(ITEM_ID))
            assert str(error.value) == f"Item with id {ITEM_ID} was not found."

    def describe_write():
        def test_upserts_a_new_record(container):
            asyncio.run(_ItemRepository(container)._write(_stored_item))
            container.upsert_item.assert_awaited_once_with(_stored_item)

        def test_replaces_a_record_only_if_the_etag_it_was_read_with_matches(container):
            record = {**_stored_item, "name": "changed"}
            asyncio.run(_ItemRepository(container)._write(record, {**_stored_item, "_etag": "etag-1"}))
            container.replace_item.assert_awaited_once_with(
                ITEM_ID, record, etag="etag-1", match_condition=MatchConditions.IfNotModified
            )
            container.upsert_item.assert_not_called()

        @pytest.mark.parametrize("write", [
            lambda repository: repository._update(ITEM_ID, {"name": "raced"}),
            lambda repository: repository._replace(ITEM_ID, {"name": "raced"}),
        ], ids=["update", "replace"])
        def test_a_write_that_lost_a_race_is_a_generic_500(container, write):
            container.query_items.return_value = _AsyncIterator([{**_stored_item, "_etag": "etag-1"}])
            container.replace_item.side_effect = CosmosAccessConditionFailedError(status_code=412, message="raced")
            with pytest.raises(CosmosAccessConditionFailedError) as error:
                asyncio.run(write(_ItemRepository(container)))

            assert container.replace_item.call_args.kwargs["etag"] == "etag-1"
            assert detect_error(error.value).status_code == 500

    def describe_get_list():
        def test_queries_undeleted_items_with_limit(container):
            container.query_items.return_value = _AsyncIterator([
                {**_stored_item, "_rid": "x"},
                {**_stored_item, "id": _ID2, "name": "mockName2"},
            ])
            result = asyncio.run(_ItemRepository(container)._get_list(5))

            container.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 5"
            )
            assert result == _responses

        def test_empty_result(container):
            container.query_items.return_value = _AsyncIterator([])
            assert asyncio.run(_ItemRepository(container)._get_list(100)) == []

    def describe_delete():
        @pytest.mark.parametrize("user_id, updated_by", [
            ("editor", [{ 'op': 'set', 'path': '/updatedBy', 'value': "editor" }]),
            (None, [{ 'op': 'set', 'path': '/updatedBy', 'value': "" }, { 'op': 'remove', 'path': '/updatedBy' }]),
        ])
        def test_patches_is_deleted_and_updated_fields(container, user_id, updated_by):
            with patch(_TIMESTAMP, return_value=_NOW):
                asyncio.run(_ItemRepository(container)._delete(ITEM_ID, user_id))

            container.patch_item.assert_awaited_once_with(
                item=ITEM_ID,
                partition_key=ITEM_ID,
                patch_operations=[
                    { 'op': 'set', 'path': '/isDeleted', 'value': True },
                    { 'op': 'set', 'path': '/updatedTimestamp', 'value': _NOW },
                    *updated_by,
                ],
                filter_predicate='from c WHERE c.isDeleted = false'
            )

        @pytest.mark.parametrize("error", [
            CosmosAccessConditionFailedError(status_code=412, message="already deleted"),
            CosmosResourceNotFoundError(status_code=404, message="missing"),
        ])
        def test_missing_or_deleted_is_not_found(container, error):
            container.patch_item.side_effect = error
            with pytest.raises(NotFoundError):
                asyncio.run(_ItemRepository(container)._delete(ITEM_ID))

        @pytest.mark.parametrize("sub_status", [1003, 1008])
        def test_missing_container_or_database_propagates(container, sub_status):
            container.patch_item.side_effect = CosmosResourceNotFoundError(
                status_code=404, message="Owner resource does not exist", sub_status=sub_status
            )
            with pytest.raises(CosmosResourceNotFoundError):
                asyncio.run(_ItemRepository(container)._delete(ITEM_ID))
            container.read.assert_not_called()

        def test_missing_container_without_sub_status_propagates(container):
            container.patch_item.side_effect = CosmosResourceNotFoundError(status_code=404, message="missing")
            container.read.side_effect = CosmosResourceNotFoundError(status_code=404, message="Collection not found")
            with pytest.raises(CosmosResourceNotFoundError, match="Collection not found"):
                asyncio.run(_ItemRepository(container)._delete(ITEM_ID))

        def test_missing_item_with_zero_sub_status_is_not_found(container):
            container.patch_item.side_effect = CosmosResourceNotFoundError(status_code=404, message="missing", sub_status=0)
            with pytest.raises(NotFoundError):
                asyncio.run(_ItemRepository(container)._delete(ITEM_ID))

        def test_other_errors_propagate(container):
            container.patch_item.side_effect = CosmosHttpResponseError(status_code=503, message="down")
            with pytest.raises(CosmosHttpResponseError):
                asyncio.run(_ItemRepository(container)._delete(ITEM_ID))
