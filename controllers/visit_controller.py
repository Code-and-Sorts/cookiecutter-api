from models import VisitCreateRequest, VisitUpdateRequest, VisitResponse
from services import VisitService
from .validation import parse_body, require_uuid

RESOURCE = "Visit"


class VisitController:
    def __init__(self, service: VisitService):
        self.service = service

    async def get_by_id(self, item_id: str | None) -> VisitResponse:
        return await self.service.get_by_id(require_uuid(item_id, RESOURCE))

    async def create(self, body: bytes | str | None, user_id: str | None = None) -> VisitResponse:
        return await self.service.create(parse_body(body, VisitCreateRequest), user_id)

    async def update(self, item_id: str | None, body: bytes | str | None, user_id: str | None = None) -> VisitResponse:
        item_id = require_uuid(item_id, RESOURCE)
        return await self.service.update(item_id, parse_body(body, VisitUpdateRequest), user_id)
