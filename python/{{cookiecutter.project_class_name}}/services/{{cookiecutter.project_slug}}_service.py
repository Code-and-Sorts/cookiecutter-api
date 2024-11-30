
from typing import List
from repositories import {{ cookiecutter.project_class_name }}Repository
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response


class {{ cookiecutter.project_class_name }}Service:
    def __init__(self, repository: {{ cookiecutter.project_class_name }}Repository):
        self.repository = repository

    def get_by_id(self, item_id: str) -> {{ cookiecutter.project_class_name }}Response:
        return self.repository.get_by_id(item_id)

    def get_list(self) -> List[{{ cookiecutter.project_class_name }}Response]:
        return self.repository.get_list()

    def create(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        return self.repository.create(item)

    def update(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        return self.repository.update(item)

    def soft_delete(self, item_id: str):
        self.repository.delete(item_id)
