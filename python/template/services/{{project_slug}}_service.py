
from typing import List
from repositories import {{ project_class_name }}Repository
from repositories.{{ project_slug }}_repository import DEFAULT_LIST_LIMIT
from models import {{ project_class_name }}, {{ project_class_name }}Response


class {{ project_class_name }}Service:
    def __init__(self, repository: {{ project_class_name }}Repository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> {{ project_class_name }}Response:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[{{ project_class_name }}Response]:
        return await self.repository.get_list(limit)

    async def create(self, item: {{ project_class_name }}) -> {{ project_class_name }}Response:
        return await self.repository.create(item)

    async def update(self, item: {{ project_class_name }}) -> {{ project_class_name }}Response:
        return await self.repository.update(item)

    async def soft_delete(self, item_id: str):
        await self.repository.delete(item_id)
