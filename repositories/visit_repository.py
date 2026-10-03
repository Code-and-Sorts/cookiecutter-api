from models import VisitEntity, VisitCreateRequest, VisitUpdateRequest, VisitResponse
from .base_repository import BaseRepository


class VisitRepository(BaseRepository[VisitResponse]):
    resource_name = "Visit"
    entity_model = VisitEntity
    response_model = VisitResponse

    async def get_by_id(self, item_id: str) -> VisitResponse:
        return await self._get_by_id(item_id)

    async def create(self, item: VisitCreateRequest, user_id: str | None = None) -> VisitResponse:
        return await self._create(item.model_dump(), user_id)

    async def update(self, item_id: str, changes: VisitUpdateRequest, user_id: str | None = None) -> VisitResponse:
        return await self._update(item_id, changes.model_dump(exclude_unset=True), user_id)
