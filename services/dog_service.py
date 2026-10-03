from typing import List
from repositories import DogRepository
from models import BaseDog, DogResponse


class DogService:
    def __init__(self, repository: DogRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> DogResponse:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int) -> List[DogResponse]:
        return await self.repository.get_list(limit)

    async def create(self, item: BaseDog, user_id: str | None = None) -> DogResponse:
        return await self.repository.create(item, user_id)

    async def replace(self, item_id: str, item: BaseDog, user_id: str | None = None) -> DogResponse:
        return await self.repository.replace(item_id, item, user_id)

    async def soft_delete(self, item_id: str, user_id: str | None = None) -> None:
        await self.repository.delete(item_id, user_id)
