{%- from 'python/_macros.jinja' import model_names -%}
{%- set names = ["BaseCreateRequest", "BaseEntity", "BaseReplaceRequest", "BaseResponse", "BaseUpdateRequest", "generate_utc_timestamp"] -%}
from .base import BaseCreateRequest, BaseEntity, BaseReplaceRequest, BaseResponse, BaseUpdateRequest
from .fields import generate_utc_timestamp
{%- for resource in path_resources %}
{%- set models = model_names(resource, entity=true, always_response=true) %}
{%- set _ = names.extend(models.split(", ")) %}
from .{{ resource.snake_name }} import {{ models }}
{%- endfor %}

__all__ = [
{%- for name in names %}
    "{{ name }}",
{%- endfor %}
]
