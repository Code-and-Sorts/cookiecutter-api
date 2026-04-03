import pytest
from unittest.mock import patch, MagicMock
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response, generate_utc_timestamp
from repositories import {{ cookiecutter.project_class_name }}Repository
from errors import NotFoundError

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.cosmos import ContainerProxy
{%- endif %}

mock_item_responses = [
    {{ cookiecutter.project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1',type='mockType1'),
    {{ cookiecutter.project_class_name }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc',name='mockName2',type='mockType2')
]
mock_update_item_response = {{ cookiecutter.project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
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


def describe_item_service():
    @pytest.fixture
    def mock_cosmos_client():
        with patch('azure.cosmos.ContainerProxy') as mock_container_client:
            return mock_container_client

    def describe_get_by_id():
        def test_successfully_call(mock_cosmos_client: ContainerProxy):
            mock_cosmos_client.query_items.return_value = mock_query
            repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            result = repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_cosmos_client.query_items.assert_called_once()
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false",
                parameters=[{"name": "@id", "value": "ac1df01c-7ece-4a20-ab60-179829dad8f5"}],
                enable_cross_partition_query=True
            )
            assert result == mock_item_responses[0]

        def test_not_found_error(mock_cosmos_client: ContainerProxy):
            mock_cosmos_client.query_items.return_value = None
            try:
                repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            except Exception as error:
                repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert isinstance(error, NotFoundError)

    def describe_get_list():
        def test_successfully_call(mock_cosmos_client: ContainerProxy):
            mock_cosmos_client.query_items.return_value = mock_query
            repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            result = repository.get_list()
            mock_cosmos_client.query_items.assert_called_once()
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.isDeleted = false",
                enable_cross_partition_query=True
            )
            assert result == mock_item_responses

        def test_successfully_call_empty_result(mock_cosmos_client: ContainerProxy):
            mock_cosmos_client.query_items.return_value = []
            repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            result = repository.get_list()
            mock_cosmos_client.query_items.assert_called_once()
            mock_cosmos_client.query_items.assert_called_once_with(
                query="SELECT * FROM c WHERE c.isDeleted = false",
                enable_cross_partition_query=True
            )
            assert result == []

    def describe_create():
        def test_successfully_call(mock_cosmos_client: ContainerProxy):
            mock_cosmos_client.create_item.return_value = mock_query[0]
            repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            mock_item = {{ cookiecutter.project_class_name }}(**mock_query[0])
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = repository.create(item=mock_item)
            mock_cosmos_client.create_item.assert_called_once()
            mock_cosmos_client.create_item.assert_called_once_with(mock_query[0])
            assert result == mock_item_responses[0]

    def describe_update():
        @patch('models.generate_utc_timestamp', return_value='2024-08-10T20:41:30Z')
        def test_successfully_call(mock_cosmos_client: ContainerProxy):
            mock_item_response = {{ cookiecutter.project_class_name }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )
            new_item_dict = {
                "name": "mockName1-Update",
                "type": "mockType1-Update"
            }
            mock_cosmos_client.upsert_item.return_value = mock_upsert
            repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            with patch('repositories.{{ cookiecutter.project_class_name }}Repository.get_by_id') as mock_get_by_id:
                mock_get_by_id.return_value = mock_item_response
                result = repository.update(
                    item={{ cookiecutter.project_class_name }}(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                )
                repository.get_by_id.assert_called_once()
                mock_cosmos_client.upsert_item.assert_called_once()
                assert result.id == 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
                assert result.name == 'mockName1-Update'
                assert result.type == 'mockType1-Update'

    def describe_delete():
        def test_successfully_call(mock_cosmos_client: ContainerProxy):
            mock_cosmos_client.patch_item.return_value = mock_query[0]
            repository = {{ cookiecutter.project_class_name }}Repository(mock_cosmos_client)
            repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_cosmos_client.patch_item.assert_called_once_with(
                item='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                partition_key='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                patch_operations=[{ 'op': 'replace', 'path': '/isDeleted', 'value': True }],
                filter_predicate='from c WHERE c.isDeleted = false'
            )
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}


def describe_item_service():
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
            mock_doc_ref.get.return_value = mock_doc
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            result = repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            assert result == mock_item_responses[0]

        def test_not_found_error(mock_firestore_collection):
            mock_doc = MagicMock()
            mock_doc.exists = False
            mock_doc_ref = MagicMock()
            mock_doc_ref.get.return_value = mock_doc
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            try:
                repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert False, "Should have raised NotFoundError"
            except NotFoundError:
                pass

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
            mock_query.stream.return_value = [mock_doc1, mock_doc2]
            mock_firestore_collection.where.return_value = mock_query

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            result = repository.get_list()

            mock_firestore_collection.where.assert_called_once_with('isDeleted', '==', False)
            assert result == mock_item_responses

        def test_successfully_call_empty_result(mock_firestore_collection):
            mock_query = MagicMock()
            mock_query.stream.return_value = []
            mock_firestore_collection.where.return_value = mock_query

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            result = repository.get_list()

            mock_firestore_collection.where.assert_called_once_with('isDeleted', '==', False)
            assert result == []

    def describe_create():
        def test_successfully_call(mock_firestore_collection):
            mock_doc_ref = MagicMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            mock_item = {{ cookiecutter.project_class_name }}(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            result = repository.create(item=mock_item)

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.set.assert_called_once()
            assert result.id == mock_item_responses[0].id

    def describe_update():
        def test_successfully_call(mock_firestore_collection):
            mock_item_response = {{ cookiecutter.project_class_name }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )

            mock_doc_ref = MagicMock()
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            with patch.object(repository, 'get_by_id', return_value=mock_item_response):
                result = repository.update(
                    item={{ cookiecutter.project_class_name }}(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                )
                assert result.id == 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
                assert result.name == 'mockName1-Update'
                assert result.type == 'mockType1-Update'

    def describe_delete():
        def test_successfully_call(mock_firestore_collection):
            mock_doc = MagicMock()
            mock_doc.exists = True
            mock_doc.to_dict.return_value = {
                "id": "ac1df01c-7ece-4a20-ab60-179829dad8f5",
                "isDeleted": False
            }
            mock_doc_ref = MagicMock()
            mock_doc_ref.get.return_value = mock_doc
            mock_firestore_collection.document.return_value = mock_doc_ref

            repository = {{ cookiecutter.project_class_name }}Repository(mock_firestore_collection)
            repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')

            mock_firestore_collection.document.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_doc_ref.update.assert_called_once_with({'isDeleted': True})
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}


def describe_item_service():
    @pytest.fixture
    def mock_dynamodb_table():
        return MagicMock()

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

            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            result = repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')

            mock_dynamodb_table.get_item.assert_called_once_with(Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"})
            assert result == mock_item_responses[0]

        def test_not_found_error(mock_dynamodb_table):
            mock_dynamodb_table.get_item.return_value = {}

            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            try:
                repository.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert False, "Should have raised NotFoundError"
            except NotFoundError:
                pass

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

            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            result = repository.get_list()

            mock_dynamodb_table.scan.assert_called_once()
            assert result == mock_item_responses

        def test_successfully_call_empty_result(mock_dynamodb_table):
            mock_dynamodb_table.scan.return_value = {"Items": []}

            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            result = repository.get_list()

            mock_dynamodb_table.scan.assert_called_once()
            assert result == []

    def describe_create():
        def test_successfully_call(mock_dynamodb_table):
            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            mock_item = {{ cookiecutter.project_class_name }}(
                name='mockName1',
                type='mockType1',
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5'
            )
            result = repository.create(item=mock_item)

            mock_dynamodb_table.put_item.assert_called_once()
            assert result.id == mock_item_responses[0].id

    def describe_update():
        def test_successfully_call(mock_dynamodb_table):
            mock_item_response = {{ cookiecutter.project_class_name }}Response(
                id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                name='mockName1',
                type='mockType1'
            )

            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            with patch.object(repository, 'get_by_id', return_value=mock_item_response):
                result = repository.update(
                    item={{ cookiecutter.project_class_name }}(
                        id='ac1df01c-7ece-4a20-ab60-179829dad8f5',
                        name='mockName1-Update',
                        type='mockType1-Update'
                    )
                )
                assert result.id == 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
                assert result.name == 'mockName1-Update'
                assert result.type == 'mockType1-Update'

    def describe_delete():
        def test_successfully_call(mock_dynamodb_table):
            repository = {{ cookiecutter.project_class_name }}Repository(mock_dynamodb_table)
            repository.delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')

            mock_dynamodb_table.update_item.assert_called_once_with(
                Key={"id": "ac1df01c-7ece-4a20-ab60-179829dad8f5"},
                UpdateExpression="SET isDeleted = :val",
                ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                ExpressionAttributeValues={":val": True, ":false": False}
            )
{%- endif %}
