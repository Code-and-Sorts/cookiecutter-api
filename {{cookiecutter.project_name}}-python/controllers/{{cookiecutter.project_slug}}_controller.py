from typing import List
import azure.functions as func
from services import {{ cookiecutter.project_class_name }}Service
from models import {{ cookiecutter.project_class_name }}Response, {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}IdValidation

class {{ cookiecutter.project_class_name }}Controller:
    def __init__(self, service: {{ cookiecutter.project_class_name }}Service):
        self.service = service

    def get_by_id(self, req: func.HttpRequest) -> {{ cookiecutter.project_class_name }}Response:
        item_id: str = req.route_params.get('item_id')
        {{ cookiecutter.project_class_name }}IdValidation(id=item_id)
        return self.service.get_by_id(item_id)

    def get_list(self) -> List[{{ cookiecutter.project_class_name }}Response]:
        return self.service.get_list()

    def create(self, req: func.HttpRequest) -> {{ cookiecutter.project_class_name }}Response:
        item_json = req.get_json()
        item = {{ cookiecutter.project_class_name }}(**item_json)
        return self.service.create(item)

    def update(self, req: func.HttpRequest) -> {{ cookiecutter.project_class_name }}Response:
        item_id = req.route_params.get('item_id')
        item_data = req.get_json()
        item = {{ cookiecutter.project_class_name }}(**item_data)
        item.id = item_id
        return self.service.update(item)

    def soft_delete(self, req: func.HttpRequest):
        item_id = req.route_params.get('item_id')
        self.service.soft_delete(item_id)
