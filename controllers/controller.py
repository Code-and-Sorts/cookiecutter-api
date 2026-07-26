from typing import List, Optional

from flask import Request

from services import (
    CatService,
    DogService,
)
from repositories.repository import DEFAULT_LIST_LIMIT
from models import (
    CatResponse,
    Cat,
    CatIdValidation,
    DogResponse,
    Dog,
    DogIdValidation,
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


class CatController:
    def __init__(self, service: CatService):
        self.service = service

    async def get_by_id(self, request: Request) -> CatResponse:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        CatIdValidation(id=item_id)
        return await self.service.get_by_id(item_id)

    async def get_list(self, request: Request) -> List[CatResponse]:
        limit = _coerce_limit(request.args.get('limit'))
        return await self.service.get_list(limit)

    async def create(self, request: Request) -> CatResponse:
        item_json: dict = request.get_json()
        item = Cat(**item_json)
        return await self.service.create(item)

    async def update(self, request: Request) -> CatResponse:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        item_data: dict = request.get_json()
        item = Cat(**item_data)
        item.id = item_id
        return await self.service.update(item)

    async def soft_delete(self, request: Request) -> None:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        await self.service.soft_delete(item_id)


class DogController:
    def __init__(self, service: DogService):
        self.service = service

    async def get_by_id(self, request: Request) -> DogResponse:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        DogIdValidation(id=item_id)
        return await self.service.get_by_id(item_id)

    async def get_list(self, request: Request) -> List[DogResponse]:
        limit = _coerce_limit(request.args.get('limit'))
        return await self.service.get_list(limit)

    async def create(self, request: Request) -> DogResponse:
        item_json: dict = request.get_json()
        item = Dog(**item_json)
        return await self.service.create(item)

    async def replace(self, request: Request) -> DogResponse:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        item_data: dict = request.get_json()
        item = Dog(**item_data)
        item.id = item_id
        return await self.service.replace(item)

    async def soft_delete(self, request: Request) -> None:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        await self.service.soft_delete(item_id)

