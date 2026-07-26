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

    def describe_create():
        def test_successfully_call(mock_firestore_collection):
            mock_doc_ref = MagicMock()
            mock_doc_ref.set = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = KittenClawsRepository(mock_firestore_collection)
            mock_item = KittenClaws(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            result = asyncio.run(repository.create(item=mock_item))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.set.assert_called_once()
            assert result.id == _kitten_claws_responses[0].id

    def describe_update():
        def test_successfully_call(mock_firestore_collection):
            mock_item_response = KittenClawsResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )

            mock_doc_ref = MagicMock()
            mock_doc_ref.update = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = KittenClawsRepository(mock_firestore_collection)
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
        def test_successfully_call(mock_firestore_collection):
            mock_item_response = KittenClawsResponse(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_doc_ref = MagicMock()
            mock_doc_ref.set = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = KittenClawsRepository(mock_firestore_collection)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item=KittenClaws(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                ))
                mock_doc_ref.set.assert_called_once()
                assert result.name == 'mockName1-Update'

    def describe_delete():
        def test_successfully_call(mock_firestore_collection):
            mock_doc = MagicMock()
            mock_doc.exists = True
            mock_doc.to_dict.return_value = {
                "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                "isDeleted": False
            }
            mock_doc_ref = MagicMock()
            mock_doc_ref.get = AsyncMock(return_value=mock_doc)
            mock_doc_ref.update = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = KittenClawsRepository(mock_firestore_collection)
            asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.update.assert_called_once_with({'isDeleted': True})

