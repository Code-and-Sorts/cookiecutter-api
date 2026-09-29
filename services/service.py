from typing import List
from repositories import (
    KittenClawsRepository,
)
from repositories.repository import DEFAULT_LIST_LIMIT
from models import (
    KittenClawsResponse,
    KittenClaws,
)


class KittenClawsService:
    def __init__(self, repository: KittenClawsRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> KittenClawsResponse:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[KittenClawsResponse]:
        return await self.repository.get_list(limit)

    async def create(self, item: KittenClaws) -> KittenClawsResponse:
        return await self.repository.create(item)

    async def update(self, item: KittenClaws) -> KittenClawsResponse:
        return await self.repository.update(item)

    async def soft_delete(self, item_id: str):
        await self.repository.delete(item_id)

