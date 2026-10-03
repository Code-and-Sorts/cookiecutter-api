from typing import List
from repositories import KittenClawsRepository
from models import KittenClawsCreateRequest, KittenClawsUpdateRequest, KittenClawsResponse


class KittenClawsService:
    def __init__(self, repository: KittenClawsRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> KittenClawsResponse:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int) -> List[KittenClawsResponse]:
        return await self.repository.get_list(limit)

    async def create(self, item: KittenClawsCreateRequest, user_id: str | None = None) -> KittenClawsResponse:
        return await self.repository.create(item, user_id)

    async def update(self, item_id: str, changes: KittenClawsUpdateRequest, user_id: str | None = None) -> KittenClawsResponse:
        return await self.repository.update(item_id, changes, user_id)

    async def soft_delete(self, item_id: str, user_id: str | None = None) -> None:
        await self.repository.delete(item_id, user_id)
