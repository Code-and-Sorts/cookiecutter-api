{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
{% if 'list' in used_ops -%}
from typing import List
{% endif -%}
from repositories import (
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
)
{% if 'list' in used_ops -%}
from repositories.repository import DEFAULT_LIST_LIMIT
{% endif -%}
from models import (
{%- for resource in resources %}
{%- if ('get_by_id' in resource.operations) or ('list' in resource.operations) or ('create' in resource.operations) or ('update' in resource.operations) or ('replace' in resource.operations) %}
    {{ resource.name }}Response,
{%- endif %}
{%- if ('create' in resource.operations) or ('update' in resource.operations) or ('replace' in resource.operations) %}
    {{ resource.name }},
{%- endif %}
{%- endfor %}
)

{% for resource in resources %}
class {{ resource.name }}Service:
    def __init__(self, repository: {{ resource.name }}Repository):
        self.repository = repository
{%- if "get_by_id" in resource.operations %}

    async def get_by_id(self, item_id: str) -> {{ resource.name }}Response:
        return await self.repository.get_by_id(item_id)
{%- endif %}
{%- if "list" in resource.operations %}

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[{{ resource.name }}Response]:
        return await self.repository.get_list(limit)
{%- endif %}
{%- if "create" in resource.operations %}

    async def create(self, item: {{ resource.name }}) -> {{ resource.name }}Response:
        return await self.repository.create(item)
{%- endif %}
{%- if "update" in resource.operations %}

    async def update(self, item: {{ resource.name }}) -> {{ resource.name }}Response:
        return await self.repository.update(item)
{%- endif %}
{%- if "replace" in resource.operations %}

    async def replace(self, item: {{ resource.name }}) -> {{ resource.name }}Response:
        return await self.repository.replace(item)
{%- endif %}
{%- if "delete" in resource.operations %}

    async def soft_delete(self, item_id: str):
        await self.repository.delete(item_id)
{%- endif %}

{% endfor %}