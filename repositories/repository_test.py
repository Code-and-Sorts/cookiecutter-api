import asyncio
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from models import (
    KittenClaws,
    KittenClawsResponse,
)
from repositories import (
    KittenClawsRepository,
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



_kitten_claws_responses = [
    KittenClawsResponse(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    KittenClawsResponse(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_kitten_claws_repository():
    @pytest.fixture
    def mock_firestore_collection():
        return MagicMock()

    def describe_get_by_id():
        def test_successfully_call(mock_firestore_collection):
            mock_doc = MagicMock()
            mock_doc.exists = True
            mock_doc.to_dict.return_value = {
                "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                "name": "mockName1",
                "type": "mockType1"
            }
            mock_doc_ref = MagicMock()
            mock_doc_ref.get = AsyncMock(return_value=mock_doc)
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = KittenClawsRepository(mock_firestore_collection)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            assert result == _kitten_claws_responses[0]

        def test_not_found_error(mock_firestore_collection):
            mock_doc = MagicMock()
            mock_doc.exists = False
            mock_doc_ref = MagicMock()
            mock_doc_ref.get = AsyncMock(return_value=mock_doc)
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = KittenClawsRepository(mock_firestore_collection)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

    def describe_get_list():
        def test_successfully_call(mock_firestore_collection):
            mock_doc1 = MagicMock()
            mock_doc1.to_dict.return_value = {
                "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                "name": "mockName1",
                "type": "mockType1"
            }
            mock_doc2 = MagicMock()
            mock_doc2.to_dict.return_value = {
                "id": "de6cbc87-5969-458c-8444-3512a82250bc",
                "name": "mockName2",
                "type": "mockType2"
            }

            mock_query = MagicMock()
            mock_query.limit.return_value = mock_query
            mock_query.stream.return_value = _AsyncIterator([mock_doc1, mock_doc2])
            mock_firestore_collection.where.return_value = mock_query

            repository = KittenClawsRepository(mock_firestore_collection)
            result = asyncio.run(repository.get_list())

            mock_firestore_collection.where.assert_called_once_with('isDeleted', '==', False)
            mock_query.limit.assert_called_once_with(100)
            assert result == _kitten_claws_responses

        def test_successfully_call_empty_result(mock_firestore_collection):
            mock_query = MagicMock()
            mock_query.limit.return_value = mock_query
            mock_query.stream.return_value = _AsyncIterator([])
            mock_firestore_collection.where.return_value = mock_query

            repository = KittenClawsRepository(mock_firestore_collection)
            result = asyncio.run(repository.get_list())

            mock_firestore_collection.where.assert_called_once_with('isDeleted', '==', False)
            assert result == []

    def _stored_doc_ref(collection, data):
        mock_doc = MagicMock()
        mock_doc.exists = data is not None
        mock_doc.to_dict.return_value = data
        mock_doc_ref = MagicMock()
        mock_doc_ref.get = AsyncMock(return_value=mock_doc)
        mock_doc_ref.set = AsyncMock()
        mock_doc_ref.update = AsyncMock()
        collection.document.return_value = mock_doc_ref
        return mock_doc_ref

    def describe_create():
        def test_successfully_call(mock_firestore_collection):
            mock_doc_ref = _stored_doc_ref(mock_firestore_collection, None)

            repository = KittenClawsRepository(mock_firestore_collection)
            mock_item = KittenClaws(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.create(item=mock_item))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.set.assert_called_once_with({
                "id": _ID,
                "name": "mockName1",
                "type": "mockType1",
                "isDeleted": False,
                "createdDate": _NOW,
                "updatedDate": _NOW,
            })
            assert result == _kitten_claws_responses[0]

    def describe_update():
        def test_merges_changes_into_stored_item(mock_firestore_collection):
            mock_doc_ref = _stored_doc_ref(mock_firestore_collection, dict(_stored_item))

            repository = KittenClawsRepository(mock_firestore_collection)
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.update(item=KittenClaws(id=_ID, name='mockName1-Update')))

            mock_doc_ref.set.assert_called_once_with(
                {**_stored_item, "name": "mockName1-Update", "updatedDate": _NOW}
            )
            assert result == KittenClawsResponse(id=_ID, name='mockName1-Update', type='mockType1')

        def test_not_found_error(mock_firestore_collection):
            mock_doc_ref = _stored_doc_ref(mock_firestore_collection, {**_stored_item, "isDeleted": True})

            repository = KittenClawsRepository(mock_firestore_collection)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.update(item=KittenClaws(id=_ID, name='mockName1-Update')))
            mock_doc_ref.set.assert_not_called()

    def describe_replace():
        def test_overwrites_item_and_keeps_created_date(mock_firestore_collection):
            mock_doc_ref = _stored_doc_ref(mock_firestore_collection, dict(_stored_item))

            repository = KittenClawsRepository(mock_firestore_collection)
            with patch(_TIMESTAMP, return_value=_NOW):
                result = asyncio.run(repository.replace(item=KittenClaws(**_replacement)))

            mock_doc_ref.set.assert_called_once_with(_expected_replacement)
            assert result == KittenClawsResponse(id=_ID, name='mockName1-Replace')

        def test_not_found_error(mock_firestore_collection):
            mock_doc_ref = _stored_doc_ref(mock_firestore_collection, None)

            repository = KittenClawsRepository(mock_firestore_collection)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.replace(item=KittenClaws(**_replacement)))
            mock_doc_ref.set.assert_not_called()

    def describe_delete():
        def test_successfully_call(mock_firestore_collection):
            mock_doc_ref = _stored_doc_ref(mock_firestore_collection, {
                "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                "isDeleted": False
            })

            repository = KittenClawsRepository(mock_firestore_collection)
            with patch(_TIMESTAMP, return_value=_NOW):
                asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.update.assert_called_once_with({'isDeleted': True, 'updatedDate': _NOW})

