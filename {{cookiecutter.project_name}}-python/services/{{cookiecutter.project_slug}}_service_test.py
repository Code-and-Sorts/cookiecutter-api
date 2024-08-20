import pytest
from unittest.mock import MagicMock
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response
from services import {{ cookiecutter.project_class_name }}Service
from repositories import {{ cookiecutter.project_class_name }}Repository

mock_item_responses = [
    {{ cookiecutter.project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1',type='mockType1'),
    {{ cookiecutter.project_class_name }}Response(id='de6cbc87-5969-458c-8444-3512a82250bc',name='mockName2',type='mockType2')
]
mock_update_item_response = {{ cookiecutter.project_class_name }}Response(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')

def describe_item_service():
    @pytest.fixture
    def mock_repository() -> {{ cookiecutter.project_class_name }}Repository:
        mock = MagicMock({{ cookiecutter.project_class_name }}Repository)
        mock.get_by_id.return_value = mock_item_responses[0]
        mock.get_list.return_value = mock_item_responses
        mock.create.return_value = mock_item_responses[0]
        mock.update.return_value = mock_update_item_response
        return mock

    @pytest.fixture
    def service(mock_repository) -> {{ cookiecutter.project_class_name }}Service:
        return {{ cookiecutter.project_class_name }}Service(repository=mock_repository)

    def describe_get_by_id():
        def test_successfully_call_repository(service: {{ cookiecutter.project_class_name }}Service, mock_repository: {{ cookiecutter.project_class_name }}Repository):
            result = service.get_by_id(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_repository.get_by_id.assert_called_once()
            assert result == mock_item_responses[0]

    def describe_get_list():
        def test_successfully_call_repository(service: {{ cookiecutter.project_class_name }}Service, mock_repository: {{ cookiecutter.project_class_name }}Repository):
            result = service.get_list()
            mock_repository.get_list.assert_called_once()
            assert result == mock_item_responses

    def describe_create():
        def test_successfully_call_repository(service: {{ cookiecutter.project_class_name }}Service, mock_repository: {{ cookiecutter.project_class_name }}Repository):
            mock_item = {{ cookiecutter.project_class_name }}(name='mockName1',type='mockType1')
            mock_item.id = 'ac1df01c-7ece-4a20-ab60-179829dad8f5'
            result = service.create(item=mock_item)
            mock_repository.create.assert_called_once()
            assert result == mock_item_responses[0]

    def describe_update():
        def test_successfully_call_repository(service: {{ cookiecutter.project_class_name }}Service, mock_repository: {{ cookiecutter.project_class_name }}Repository):
            mock_update_item = {{ cookiecutter.project_class_name }}(id='ac1df01c-7ece-4a20-ab60-179829dad8f5',name='mockName1-Update',type='mockType1-Update')
            result = service.update(item=mock_update_item)
            mock_repository.update.assert_called_once()
            assert result == mock_update_item_response

    def describe_soft_delete():
        def test_successfully_call_repository(service: {{ cookiecutter.project_class_name }}Service, mock_repository: {{ cookiecutter.project_class_name }}Repository):
            result = service.soft_delete(item_id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
            mock_repository.delete.assert_called_once()
