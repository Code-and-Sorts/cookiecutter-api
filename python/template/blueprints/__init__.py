{%- set op_fn = {'list': 'get_list', 'get_by_id': 'get_by_id', 'create': 'create', 'update': 'update', 'replace': 'replace', 'delete': 'delete'} -%}
{%- if cloud_service == 'Azure Function App' -%}
# Each module owns one Blueprint; function_app.py registers every one.
from .health import bp as health_bp
{%- for resource in resources %}
from .{{ resource.name | to_snake }} import bp as {{ resource.name | to_snake }}_bp
{%- endfor %}

__all__ = [
    "health_bp",
{%- for resource in resources %}
    "{{ resource.name | to_snake }}_bp",
{%- endfor %}
]
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' -%}
# Each module holds one resource's Cloud Functions; main.py exports every one.
from .health import health
{%- for resource in resources %}
{%- set slug = resource.name | to_snake %}
from .{{ slug }} import {% for op in resource.operations %}{{ op_fn[op] }}_{{ slug }}{% if not loop.last %}, {% endif %}{% endfor %}
{%- endfor %}

__all__ = [
    "health",
{%- for resource in resources %}
{%- set slug = resource.name | to_snake %}
{%- for op in resource.operations %}
    "{{ op_fn[op] }}_{{ slug }}",
{%- endfor %}
{%- endfor %}
]
{%- endif %}
{%- if cloud_service == 'AWS Lambda' -%}
# Each module holds one resource's handlers and route table; lambda_app.py
# maps each endpoint to its route table.
from .health import health
{%- for resource in resources %}
from .{{ resource.name | to_snake }} import ROUTES as {{ resource.name | to_snake }}_routes
{%- endfor %}

__all__ = [
    "health",
{%- for resource in resources %}
    "{{ resource.name | to_snake }}_routes",
{%- endfor %}
]
{%- endif %}
