import pytest
from azure.cosmos import ContainerProxy
from unittest.mock import patch
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response, generate_utc_timestamp
from repositories import {{ cookiecutter.project_class_name }}Repository
from errors import NotFoundError

mock_item_responses = [
    {{ cookiecutter.project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1',type='mockType1'),
    {{ cookiecutter.project_class_name }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc',name='mockName2',type='mockType2')
]
mock_update_item_response = {{ cookiecutter.project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')
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
