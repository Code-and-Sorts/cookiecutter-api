from typing import List
from repositories import CatRepository
from models import CatCreateRequest, CatUpdateRequest, CatResponse


class CatService:
    def __init__(self, repository: CatRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> CatResponse:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int) -> List[CatResponse]:
        return await self.repository.get_list(limit)

    async def create(self, item: CatCreateRequest, user_id: str | None = None) -> CatResponse:
        return await self.repository.create(item, user_id)

    async def update(self, item_id: str, changes: CatUpdateRequest, user_id: str | None = None) -> CatResponse:
        return await self.repository.update(item_id, changes, user_id)

    async def soft_delete(self, item_id: str, user_id: str | None = None) -> None:
        await self.repository.delete(item_id, user_id)
