from .base_repository import BaseRepository
{%- for resource in resources %}
from .{{ resource.name | to_snake }}_repository import {{ resource.name }}Repository
{%- endfor %}

__all__ = [
    "BaseRepository",
{%- for resource in resources %}
    "{{ resource.name }}Repository",
{%- endfor %}
]
