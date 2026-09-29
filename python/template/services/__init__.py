from .service import (
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
)

__all__ = [
{%- for resource in resources %}
    "{{ resource.name }}Service",
{%- endfor %}
]
