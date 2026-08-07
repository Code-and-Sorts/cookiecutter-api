import asyncio
import pytest
from unittest.mock import patch, MagicMock, AsyncMock
from models import (
{%- for resource in resources %}
    {{ resource.name }},
    {{ resource.name }}Response,
{%- endfor %}
    generate_utc_timestamp,
)
from repositories import (
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
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

{% if cloud_service == 'Azure Function App' -%}
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

{% for resource in resources %}
{%- set r = resource.name %}
{%- set slug = r | to_snake %}
_{{ slug }}_responses = [
    {{ r }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    {{ r }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_{{ slug }}_repository():
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
            repository = {{ r }}Repository(mock_cosmos_client)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false",
                parameters=[{"name": "@id", "value": "ac1df01c-7ece-4a20-ab60-179829dad8f5"}]
            )
            assert result == _{{ slug }}_responses[0]

        def test_not_found_error(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator([])
            repository = {{ r }}Repository(mock_cosmos_client)
            with pytest.raises(NotFoundError):
                asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

    def describe_get_list():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator(mock_query)
            repository = {{ r }}Repository(mock_cosmos_client)
            result = asyncio.run(repository.get_list())
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100"
            )
            assert result == _{{ slug }}_responses

        def test_successfully_call_empty_result(mock_cosmos_client):
            mock_cosmos_client.query_items.return_value = _AsyncIterator([])
            repository = {{ r }}Repository(mock_cosmos_client)
            result = asyncio.run(repository.get_list())
            assert result == []

    def describe_create():
        def test_successfully_call(mock_cosmos_client):
            mock_cosmos_client.create_item.return_value = mock_query[0]
            repository = {{ r }}Repository(mock_cosmos_client)
            mock_item = {{ r }}(**mock_query[0])
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = asyncio.run(repository.create(item=mock_item))
            mock_cosmos_client.create_item.assert_called_once_with(mock_query[0])
            assert result == _{{ slug }}_responses[0]

    def describe_update():
        def test_successfully_call(mock_cosmos_client):
            mock_item_response = {{ r }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = {{ r }}Repository(mock_cosmos_client)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.update(
                    item={{ r }}(
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
            mock_item_response = {{ r }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = {{ r }}Repository(mock_cosmos_client)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item={{ r }}(
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
            repository = {{ r }}Repository(mock_cosmos_client)
            asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))
            mock_cosmos_client.patch_item.assert_called_once_with(
                item='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                partition_key='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                patch_operations=[{ 'op': 'replace', 'path': '/isDeleted', 'value': True }],
                filter_predicate='from c WHERE c.isDeleted = false'
            )
{% endfor %}
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
{% for resource in resources %}
{%- set r = resource.name %}
{%- set slug = r | to_snake %}
_{{ slug }}_responses = [
    {{ r }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    {{ r }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_{{ slug }}_repository():
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

            repository = {{ r }}Repository(mock_firestore_collection)
            result = asyncio.run(repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            assert result == _{{ slug }}_responses[0]

        def test_not_found_error(mock_firestore_collection):
            mock_doc = MagicMock()
            mock_doc.exists = False
            mock_doc_ref = MagicMock()
            mock_doc_ref.get = AsyncMock(return_value=mock_doc)
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ r }}Repository(mock_firestore_collection)
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

            repository = {{ r }}Repository(mock_firestore_collection)
            result = asyncio.run(repository.get_list())

            mock_firestore_collection.where.assert_called_once_with('isDeleted', '==', False)
            mock_query.limit.assert_called_once_with(100)
            assert result == _{{ slug }}_responses

        def test_successfully_call_empty_result(mock_firestore_collection):
            mock_query = MagicMock()
            mock_query.limit.return_value = mock_query
            mock_query.stream.return_value = _AsyncIterator([])
            mock_firestore_collection.where.return_value = mock_query

            repository = {{ r }}Repository(mock_firestore_collection)
            result = asyncio.run(repository.get_list())

            mock_firestore_collection.where.assert_called_once_with('isDeleted', '==', False)
            assert result == []

    def describe_create():
        def test_successfully_call(mock_firestore_collection):
            mock_doc_ref = MagicMock()
            mock_doc_ref.set = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ r }}Repository(mock_firestore_collection)
            mock_item = {{ r }}(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            result = asyncio.run(repository.create(item=mock_item))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.set.assert_called_once()
            assert result.id == _{{ slug }}_responses[0].id

    def describe_update():
        def test_successfully_call(mock_firestore_collection):
            mock_item_response = {{ r }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )

            mock_doc_ref = MagicMock()
            mock_doc_ref.update = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ r }}Repository(mock_firestore_collection)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.update(
                    item={{ r }}(
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
            mock_item_response = {{ r }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            mock_doc_ref = MagicMock()
            mock_doc_ref.set = AsyncMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ r }}Repository(mock_firestore_collection)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item={{ r }}(
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

            repository = {{ r }}Repository(mock_firestore_collection)
            asyncio.run(repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5'))

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.update.assert_called_once_with({'isDeleted': True})
{% endfor %}
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
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

{% for resource in resources %}
{%- set r = resource.name %}
{%- set slug = r | to_snake %}
_{{ slug }}_responses = [
    {{ r }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5', name='mockName1', type='mockType1'),
    {{ r }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc', name='mockName2', type='mockType2')
]


def describe_{{ slug }}_repository():
    @pytest.fixture
    def mock_dynamodb_table():
        table = MagicMock()
        table.get_item = AsyncMock()
        table.scan = AsyncMock()
        table.put_item = AsyncMock()
        table.update_item = AsyncMock()
        return table

    def _repository(table):
        return {{ r }}Repository(_make_session(table), "{{ resource.container }}", "us-east-1")

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
            assert result == _{{ slug }}_responses[0]

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
            assert result == _{{ slug }}_responses

        def test_successfully_call_empty_result(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {"Items": []}

            repository = _repository(mock_dynamodb_table)
            result = asyncio.run(repository.get_list())

            mock_dynamodb_table.scan.assert_called_once()
            assert result == []

    def describe_create():
        def test_successfully_call(mock_dynamodb_table):
            repository = _repository(mock_dynamodb_table)
            mock_item = {{ r }}(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            result = asyncio.run(repository.create(item=mock_item))

            mock_dynamodb_table.put_item.assert_called_once()
            assert result.id == _{{ slug }}_responses[0].id

    def describe_update():
        def test_successfully_call(mock_dynamodb_table):
            mock_item_response = {{ r }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )

            repository = _repository(mock_dynamodb_table)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.update(
                    item={{ r }}(
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
            mock_item_response = {{ r }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            repository = _repository(mock_dynamodb_table)
            with patch.object(repository, 'get_by_id', new=AsyncMock(return_value=mock_item_response)):
                result = asyncio.run(repository.replace(
                    item={{ r }}(
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
{% endfor %}
{%- endif %}