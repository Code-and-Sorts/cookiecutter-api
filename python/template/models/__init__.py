from .base import (
    SYSTEM_FIELDS,
    BaseCreateRequest,
    BaseEntity,
    BaseReplaceRequest,
    BaseResponse,
    BaseUpdateRequest,
)
from .fields import generate_utc_timestamp
{%- for resource in path_resources %}
{%- set r = resource.name %}
{%- set ops = resource.operations %}
from .{{ resource.snake_name }} import (
    {{ r }}Entity,
{%- if 'create' in ops %}
    {{ r }}CreateRequest,
{%- endif %}
{%- if 'replace' in ops %}
    {{ r }}ReplaceRequest,
{%- endif %}
{%- if 'update' in ops %}
    {{ r }}UpdateRequest,
{%- endif %}
    {{ r }}Response,
)
{%- endfor %}

__all__ = [
    "SYSTEM_FIELDS",
    "BaseCreateRequest",
    "BaseEntity",
    "BaseReplaceRequest",
    "BaseResponse",
    "BaseUpdateRequest",
    "generate_utc_timestamp",
{%- for resource in path_resources %}
{%- set r = resource.name %}
{%- set ops = resource.operations %}
    "{{ r }}Entity",
{%- if 'create' in ops %}
    "{{ r }}CreateRequest",
{%- endif %}
{%- if 'replace' in ops %}
    "{{ r }}ReplaceRequest",
{%- endif %}
{%- if 'update' in ops %}
    "{{ r }}UpdateRequest",
{%- endif %}
    "{{ r }}Response",
{%- endfor %}
]
