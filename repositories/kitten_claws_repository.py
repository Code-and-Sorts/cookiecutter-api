from typing import List
from models import KittenClawsEntity, KittenClawsCreateRequest, KittenClawsUpdateRequest, KittenClawsResponse
from .base_repository import BaseRepository


class KittenClawsRepository(BaseRepository[KittenClawsResponse]):
    resource_name = "KittenClaws"
    entity_model = KittenClawsEntity
    response_model = KittenClawsResponse

    async def get_by_id(self, item_id: str) -> KittenClawsResponse:
        return await self._get_by_id(item_id)

    async def get_list(self, limit: int) -> List[KittenClawsResponse]:
        return await self._get_list(limit)

    async def create(self, item: KittenClawsCreateRequest, user_id: str | None = None) -> KittenClawsResponse:
        return await self._create(item.model_dump(), user_id)

    async def update(self, item_id: str, changes: KittenClawsUpdateRequest, user_id: str | None = None) -> KittenClawsResponse:
        return await self._update(item_id, changes.model_dump(exclude_unset=True), user_id)

    async def delete(self, item_id: str, user_id: str | None = None) -> None:
        await self._delete(item_id, user_id)
