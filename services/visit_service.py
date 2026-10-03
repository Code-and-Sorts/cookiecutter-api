from repositories import VisitRepository
from models import VisitCreateRequest, VisitUpdateRequest, VisitResponse


class VisitService:
    def __init__(self, repository: VisitRepository):
        self.repository = repository

    async def get_by_id(self, item_id: str) -> VisitResponse:
        return await self.repository.get_by_id(item_id)

    async def create(self, item: VisitCreateRequest, user_id: str | None = None) -> VisitResponse:
        return await self.repository.create(item, user_id)

    async def update(self, item_id: str, changes: VisitUpdateRequest, user_id: str | None = None) -> VisitResponse:
        return await self.repository.update(item_id, changes, user_id)
