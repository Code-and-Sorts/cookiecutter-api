from typing import List
from models import DogEntity, DogCreateRequest, DogReplaceRequest, DogResponse
from .base_repository import BaseRepository


class DogRepository(BaseRepository[DogResponse]):
    resource_name = "Dog"
    entity_model = DogEntity
    response_model = DogResponse

    async def get_by_id(self, item_id: str) -> DogResponse:
        return await self._get_by_id(item_id)

    async def get_list(self, limit: int) -> List[DogResponse]:
        return await self._get_list(limit)

    async def create(self, item: DogCreateRequest, user_id: str | None = None) -> DogResponse:
        return await self._create(item.model_dump(), user_id)

    async def replace(self, item_id: str, item: DogReplaceRequest, user_id: str | None = None) -> DogResponse:
        return await self._update(item_id, item.model_dump(), user_id)

    async def delete(self, item_id: str, user_id: str | None = None) -> None:
        await self._delete(item_id, user_id)
