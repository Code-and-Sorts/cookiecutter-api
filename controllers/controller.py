from typing import List, Optional
import azure.functions as func


from services import (
    KittenClawsService,
)
from repositories.repository import DEFAULT_LIST_LIMIT
from models import (
    KittenClawsResponse,
    KittenClaws,
    KittenClawsIdValidation,
)

MAX_LIST_LIMIT = 1000


def _coerce_limit(raw: Optional[str]) -> int:
    try:
        limit = int(raw)
    except (TypeError, ValueError):
        return DEFAULT_LIST_LIMIT
    if limit < 1:
        return DEFAULT_LIST_LIMIT
    return min(limit, MAX_LIST_LIMIT)


class KittenClawsController:
    def __init__(self, service: KittenClawsService):
        self.service = service

    async def get_by_id(self, req: func.HttpRequest) -> KittenClawsResponse:
        item_id: str = req.route_params.get('item_id')
        KittenClawsIdValidation(id=item_id)
        return await self.service.get_by_id(item_id)

    async def get_list(self, req: func.HttpRequest) -> List[KittenClawsResponse]:
        limit = _coerce_limit(req.params.get('limit'))
        return await self.service.get_list(limit)

    async def create(self, req: func.HttpRequest) -> KittenClawsResponse:
        item_json: dict = req.get_json()
        item = KittenClaws(**item_json)
        return await self.service.create(item)

    async def update(self, req: func.HttpRequest) -> KittenClawsResponse:
        item_id: str = req.route_params.get('item_id')
        item_data: dict = req.get_json()
        item = KittenClaws(**item_data)
        item.id = item_id
        return await self.service.update(item)

    async def soft_delete(self, req: func.HttpRequest) -> None:
        item_id: str = req.route_params.get('item_id')
        await self.service.soft_delete(item_id)

