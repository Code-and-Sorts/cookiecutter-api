from typing import List
from repositories import (
    CatRepository,
    DogRepository,
)
from repositories.repository import DEFAULT_LIST_LIMIT
from models import (
    CatResponse,
    Cat,
    DogResponse,
    Dog,
)


class CatService:
    def __init__(self, repository: CatRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> CatResponse:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[CatResponse]:
        return await self.repository.get_list(limit)

    async def create(self, item: Cat) -> CatResponse:
        return await self.repository.create(item)

    async def update(self, item: Cat) -> CatResponse:
        return await self.repository.update(item)

    async def soft_delete(self, item_id: str):
        await self.repository.delete(item_id)


class DogService:
    def __init__(self, repository: DogRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> DogResponse:
        return await self.repository.get_by_id(item_id)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[DogResponse]:
        return await self.repository.get_list(limit)

    async def create(self, item: Dog) -> DogResponse:
        return await self.repository.create(item)

    async def replace(self, item: Dog) -> DogResponse:
        return await self.repository.replace(item)

    async def soft_delete(self, item_id: str):
        await self.repository.delete(item_id)

