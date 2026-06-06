
from typing import List
from repositories import {{ cookiecutter.project_class_name }}Repository
from repositories.{{ cookiecutter.project_slug }}_repository import DEFAULT_LIST_LIMIT
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response


class {{ cookiecutter.project_class_name }}Service:
    def __init__(self, repository: {{ cookiecutter.project_class_name }}Repository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> {{ cookiecutter.project_class_name }}Response:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[{{ cookiecutter.project_class_name }}Response]:
        return await self.repository.get_list(limit)

    async def create(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        return await self.repository.create(item)

    async def update(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        return await self.repository.update(item)

    async def soft_delete(self, item_id: str):
        await self.repository.delete(item_id)
