{%- for resource in resources %}
from .{{ resource.name | to_snake }}_controller import {{ resource.name }}Controller
{%- endfor %}

__all__ = [
{%- for resource in resources %}
    "{{ resource.name }}Controller",
{%- endfor %}
]
