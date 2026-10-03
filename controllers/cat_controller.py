from typing import List
from models import BaseCat, CatUpdate, CatResponse
from services import CatService
from .pagination import coerce_limit
from .validation import parse_body, require_uuid

RESOURCE = "Cat"


class CatController:
    def __init__(self, service: CatService):
        self.service = service

    async def get_by_id(self, item_id: str | None) -> CatResponse:
        return await self.service.get_by_id(require_uuid(item_id, RESOURCE))

    async def get_list(self, limit: str | None = None) -> List[CatResponse]:
        return await self.service.get_list(coerce_limit(limit))

    async def create(self, body: bytes | str | None, user_id: str | None = None) -> CatResponse:
        return await self.service.create(parse_body(body, BaseCat), user_id)

    async def update(self, item_id: str | None, body: bytes | str | None, user_id: str | None = None) -> CatResponse:
        item_id = require_uuid(item_id, RESOURCE)
        return await self.service.update(item_id, parse_body(body, CatUpdate), user_id)

    async def delete(self, item_id: str | None, user_id: str | None = None) -> dict:
        item_id = require_uuid(item_id, RESOURCE)
        await self.service.soft_delete(item_id, user_id)
        return {"message": f"{RESOURCE} with id {item_id} was deleted successfully."}
