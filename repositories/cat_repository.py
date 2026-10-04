from typing import List
from models import BaseCat, CatUpdate, CatResponse
from .base_repository import BaseRepository


class CatRepository(BaseRepository[CatResponse]):
    resource_name = "Cat"
    response_model = CatResponse

    async def get_by_id(self, item_id: str) -> CatResponse:
        return await self._get_by_id(item_id)

    async def get_list(self, limit: int) -> List[CatResponse]:
        return await self._get_list(limit)

    async def create(self, item: BaseCat, user_id: str | None = None) -> CatResponse:
        return await self._create(item.model_dump(), user_id)

    async def update(self, item_id: str, changes: CatUpdate, user_id: str | None = None) -> CatResponse:
        return await self._update(item_id, changes.model_dump(exclude_unset=True), user_id)

    async def delete(self, item_id: str, user_id: str | None = None) -> None:
        await self._delete(item_id, user_id)
