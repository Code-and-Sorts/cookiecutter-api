from typing import List, Optional


import json
from errors import ValidationError
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

    def _parse_body(self, event: dict) -> dict:
        try:
            body = event.get("body") or "{}"
            if event.get("isBase64Encoded"):
                import base64
                body = base64.b64decode(body).decode("utf-8")
            return json.loads(body)
        except (json.JSONDecodeError, Exception):
            raise ValidationError("Invalid JSON in request body.")

    async def get_by_id(self, event: dict) -> CatResponse:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        CatIdValidation(id=item_id)
        return await self.service.get_by_id(item_id)

    async def get_list(self, event: dict) -> List[CatResponse]:
        limit = _coerce_limit((event.get("queryStringParameters") or {}).get("limit"))
        return await self.service.get_list(limit)

    async def create(self, event: dict) -> CatResponse:
        item_json: dict = self._parse_body(event)
        item = Cat(**item_json)
        return await self.service.create(item)

    async def update(self, event: dict) -> CatResponse:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        item_data: dict = self._parse_body(event)
        item = Cat(**item_data)
        item.id = item_id
        return await self.service.update(item)

    async def soft_delete(self, event: dict) -> None:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        await self.service.soft_delete(item_id)


class DogController:
    def __init__(self, service: DogService):
        self.service = service

    def _parse_body(self, event: dict) -> dict:
        try:
            body = event.get("body") or "{}"
            if event.get("isBase64Encoded"):
                import base64
                body = base64.b64decode(body).decode("utf-8")
            return json.loads(body)
        except (json.JSONDecodeError, Exception):
            raise ValidationError("Invalid JSON in request body.")

    async def get_by_id(self, event: dict) -> DogResponse:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        DogIdValidation(id=item_id)
        return await self.service.get_by_id(item_id)

    async def get_list(self, event: dict) -> List[DogResponse]:
        limit = _coerce_limit((event.get("queryStringParameters") or {}).get("limit"))
        return await self.service.get_list(limit)

    async def create(self, event: dict) -> DogResponse:
        item_json: dict = self._parse_body(event)
        item = Dog(**item_json)
        return await self.service.create(item)

    async def replace(self, event: dict) -> DogResponse:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        item_data: dict = self._parse_body(event)
        item = Dog(**item_data)
        item.id = item_id
        return await self.service.replace(item)

    async def soft_delete(self, event: dict) -> None:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        await self.service.soft_delete(item_id)

