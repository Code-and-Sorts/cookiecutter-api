{%- for resource in resources %}
from .{{ resource.name | to_snake }}_service import {{ resource.name }}Service
{%- endfor %}

__all__ = [
{%- for resource in resources %}
    "{{ resource.name }}Service",
{%- endfor %}
]
