{% if cloud_service == 'Azure Function App' -%}
from .repository import (
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
    Database,
)

__all__ = [
{%- for resource in resources %}
    "{{ resource.name }}Repository",
{%- endfor %}
    "Database",
]
{%- else -%}
from .repository import (
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
)

__all__ = [
{%- for resource in resources %}
    "{{ resource.name }}Repository",
{%- endfor %}
]
{%- endif %}
