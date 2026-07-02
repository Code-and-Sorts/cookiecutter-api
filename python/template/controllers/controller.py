{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
{%- set uses_body = ('create' in used_ops) or ('update' in used_ops) or ('replace' in used_ops) -%}
from typing import {% if 'list' in used_ops %}List, {% endif %}Optional
{% if cloud_service == 'Azure Function App' -%}
import azure.functions as func
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
from flask import Request
{%- endif %}
{% if cloud_service == 'AWS Lambda' and uses_body -%}
import json
from errors import ValidationError
{%- endif %}
from services import (
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
)
from repositories.repository import DEFAULT_LIST_LIMIT
from models import (
{%- for resource in resources %}
{%- if ('get_by_id' in resource.operations) or ('list' in resource.operations) or ('create' in resource.operations) or ('update' in resource.operations) or ('replace' in resource.operations) %}
    {{ resource.name }}Response,
{%- endif %}
{%- if ('create' in resource.operations) or ('update' in resource.operations) or ('replace' in resource.operations) %}
    {{ resource.name }},
{%- endif %}
{%- if 'get_by_id' in resource.operations %}
    {{ resource.name }}IdValidation,
{%- endif %}
{%- endfor %}
)

# Upper bound on a single list page, protecting the datastore from
# pathologically large reads even if a client asks for more.
MAX_LIST_LIMIT = 1000


def _coerce_limit(raw: Optional[str]) -> int:
    try:
        limit = int(raw)
    except (TypeError, ValueError):
        return DEFAULT_LIST_LIMIT
    if limit < 1:
        return DEFAULT_LIST_LIMIT
    return min(limit, MAX_LIST_LIMIT)

{% for resource in resources %}
{%- set uses_body_r = ('create' in resource.operations) or ('update' in resource.operations) or ('replace' in resource.operations) %}
class {{ resource.name }}Controller:
    def __init__(self, service: {{ resource.name }}Service):
        self.service = service
{%- if cloud_service == 'AWS Lambda' and uses_body_r %}

    def _parse_body(self, event: dict) -> dict:
        try:
            body = event.get("body") or "{}"
            if event.get("isBase64Encoded"):
                import base64
                body = base64.b64decode(body).decode("utf-8")
            return json.loads(body)
        except (json.JSONDecodeError, Exception):
            raise ValidationError("Invalid JSON in request body.")
{%- endif %}
{%- if "get_by_id" in resource.operations %}
{%- if cloud_service == 'Azure Function App' %}

    async def get_by_id(self, req: func.HttpRequest) -> {{ resource.name }}Response:
        item_id: str = req.route_params.get('item_id')
        {{ resource.name }}IdValidation(id=item_id)
        return await self.service.get_by_id(item_id)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    async def get_by_id(self, request: Request) -> {{ resource.name }}Response:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        {{ resource.name }}IdValidation(id=item_id)
        return await self.service.get_by_id(item_id)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    async def get_by_id(self, event: dict) -> {{ resource.name }}Response:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        {{ resource.name }}IdValidation(id=item_id)
        return await self.service.get_by_id(item_id)
{%- endif %}
{%- endif %}
{%- if "list" in resource.operations %}
{%- if cloud_service == 'Azure Function App' %}

    async def get_list(self, req: func.HttpRequest) -> List[{{ resource.name }}Response]:
        limit = _coerce_limit(req.params.get('limit'))
        return await self.service.get_list(limit)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    async def get_list(self, request: Request) -> List[{{ resource.name }}Response]:
        limit = _coerce_limit(request.args.get('limit'))
        return await self.service.get_list(limit)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    async def get_list(self, event: dict) -> List[{{ resource.name }}Response]:
        limit = _coerce_limit((event.get("queryStringParameters") or {}).get("limit"))
        return await self.service.get_list(limit)
{%- endif %}
{%- endif %}
{%- if "create" in resource.operations %}
{%- if cloud_service == 'Azure Function App' %}

    async def create(self, req: func.HttpRequest) -> {{ resource.name }}Response:
        item_json: dict = req.get_json()
        item = {{ resource.name }}(**item_json)
        return await self.service.create(item)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    async def create(self, request: Request) -> {{ resource.name }}Response:
        item_json: dict = request.get_json()
        item = {{ resource.name }}(**item_json)
        return await self.service.create(item)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    async def create(self, event: dict) -> {{ resource.name }}Response:
        item_json: dict = self._parse_body(event)
        item = {{ resource.name }}(**item_json)
        return await self.service.create(item)
{%- endif %}
{%- endif %}
{%- if "update" in resource.operations %}
{%- if cloud_service == 'Azure Function App' %}

    async def update(self, req: func.HttpRequest) -> {{ resource.name }}Response:
        item_id: str = req.route_params.get('item_id')
        item_data: dict = req.get_json()
        item = {{ resource.name }}(**item_data)
        item.id = item_id
        return await self.service.update(item)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    async def update(self, request: Request) -> {{ resource.name }}Response:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        item_data: dict = request.get_json()
        item = {{ resource.name }}(**item_data)
        item.id = item_id
        return await self.service.update(item)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    async def update(self, event: dict) -> {{ resource.name }}Response:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        item_data: dict = self._parse_body(event)
        item = {{ resource.name }}(**item_data)
        item.id = item_id
        return await self.service.update(item)
{%- endif %}
{%- endif %}
{%- if "replace" in resource.operations %}
{%- if cloud_service == 'Azure Function App' %}

    async def replace(self, req: func.HttpRequest) -> {{ resource.name }}Response:
        item_id: str = req.route_params.get('item_id')
        item_data: dict = req.get_json()
        item = {{ resource.name }}(**item_data)
        item.id = item_id
        return await self.service.replace(item)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    async def replace(self, request: Request) -> {{ resource.name }}Response:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        item_data: dict = request.get_json()
        item = {{ resource.name }}(**item_data)
        item.id = item_id
        return await self.service.replace(item)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    async def replace(self, event: dict) -> {{ resource.name }}Response:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        item_data: dict = self._parse_body(event)
        item = {{ resource.name }}(**item_data)
        item.id = item_id
        return await self.service.replace(item)
{%- endif %}
{%- endif %}
{%- if "delete" in resource.operations %}
{%- if cloud_service == 'Azure Function App' %}

    async def soft_delete(self, req: func.HttpRequest) -> None:
        item_id: str = req.route_params.get('item_id')
        await self.service.soft_delete(item_id)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    async def soft_delete(self, request: Request) -> None:
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        await self.service.soft_delete(item_id)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    async def soft_delete(self, event: dict) -> None:
        item_id: str = (event.get("pathParameters") or {}).get("item_id")
        await self.service.soft_delete(item_id)
{%- endif %}
{%- endif %}

{% endfor %}