import asyncio
import uuid
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from google.cloud.firestore import DELETE_FIELD, FieldFilter
from models import BaseResponse
from repositories import BaseRepository
from repositories.base_repository import FIRESTORE_CALL_OPTIONS
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


def _doc_ref(collection, data):
    doc = MagicMock()
    doc.exists = data is not None
    doc.to_dict.return_value = data
    doc_ref = MagicMock()
    doc_ref.get = AsyncMock(return_value=doc)
    doc_ref.set = AsyncMock()
    doc_ref.update = AsyncMock()
    collection.document.return_value = doc_ref
    return doc_ref


def describe_firestore_storage():
    @pytest.fixture
    def collection():
        return MagicMock()

    def test_calls_are_bounded():
        retry = FIRESTORE_CALL_OPTIONS["retry"]
        assert FIRESTORE_CALL_OPTIONS["timeout"] <= 3
        assert retry._timeout <= 5
        assert retry._maximum <= 1

    def describe_get_stored():
        def test_reads_document(collection):
            _doc_ref(collection, dict(_stored_item))
            result = asyncio.run(_ItemRepository(collection)._get_by_id(ITEM_ID))

            collection.document.assert_called_once_with(ITEM_ID)
            collection.document.return_value.get.assert_awaited_once_with(**FIRESTORE_CALL_OPTIONS)
            assert result == _responses[0]

        @pytest.mark.parametrize("data", [None, {**_stored_item, "isDeleted": True}])
        def test_missing_or_deleted_is_not_found(collection, data):
            _doc_ref(collection, data)
            with pytest.raises(NotFoundError) as error:
                asyncio.run(_ItemRepository(collection)._get_by_id(ITEM_ID))
            assert str(error.value) == f"Item with id {ITEM_ID} was not found."

    def describe_write():
        def test_sets_document_by_id(collection):
            doc_ref = _doc_ref(collection, None)
            asyncio.run(_ItemRepository(collection)._write(_stored_item))

            collection.document.assert_called_once_with(ITEM_ID)
            doc_ref.set.assert_awaited_once_with(_stored_item, **FIRESTORE_CALL_OPTIONS)

    def describe_get_list():
        def test_filters_undeleted_with_limit(collection):
            docs = [MagicMock(), MagicMock()]
            docs[0].to_dict.return_value = dict(_stored_item)
            docs[1].to_dict.return_value = {**_stored_item, "id": _ID2, "name": "mockName2"}
            query = MagicMock()
            query.limit.return_value = query
            query.stream.return_value = _AsyncIterator(docs)
            collection.where.return_value = query

            result = asyncio.run(_ItemRepository(collection)._get_list(7))

            (filter_arg,) = collection.where.call_args.kwargs.values()
            assert isinstance(filter_arg, FieldFilter)
            assert (filter_arg.field_path, filter_arg.op_string, filter_arg.value) == ("isDeleted", "==", False)
            query.limit.assert_called_once_with(7)
            query.stream.assert_called_once_with(**FIRESTORE_CALL_OPTIONS)
            assert result == _responses

        def test_empty_result(collection):
            query = MagicMock()
            query.limit.return_value = query
            query.stream.return_value = _AsyncIterator([])
            collection.where.return_value = query

            assert asyncio.run(_ItemRepository(collection)._get_list(100)) == []
            query.limit.assert_called_once_with(100)

    def describe_delete():
        @pytest.mark.parametrize("user_id, updated_by", [("editor", "editor"), (None, DELETE_FIELD)])
        def test_flags_document_deleted(collection, user_id, updated_by):
            doc_ref = _doc_ref(collection, dict(_stored_item))
            with patch(_TIMESTAMP, return_value=_NOW):
                asyncio.run(_ItemRepository(collection)._delete(ITEM_ID, user_id))

            collection.document.assert_called_once_with(ITEM_ID)
            doc_ref.update.assert_awaited_once_with(
                {'isDeleted': True, 'updatedTimestamp': _NOW, 'updatedBy': updated_by}, **FIRESTORE_CALL_OPTIONS
            )

        @pytest.mark.parametrize("data", [None, {**_stored_item, "isDeleted": True}])
        def test_missing_or_deleted_is_not_found(collection, data):
            doc_ref = _doc_ref(collection, data)
            with pytest.raises(NotFoundError):
                asyncio.run(_ItemRepository(collection)._delete(ITEM_ID))
            doc_ref.update.assert_not_called()
