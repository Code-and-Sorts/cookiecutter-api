from .controller import (
{%- for resource in resources %}
    {{ resource.name }}Controller,
{%- endfor %}
)

__all__ = [
{%- for resource in resources %}
    "{{ resource.name }}Controller",
{%- endfor %}
]
