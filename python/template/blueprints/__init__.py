{%- if cloud_service == 'Azure Function App' -%}
# Each module owns one Blueprint (``bp``); function_app.py registers every one.
from . import (
{%- if health_endpoint %}
    health,
{%- endif %}
{%- for resource in resources %}
    {{ resource.name | to_snake }},
{%- endfor %}
)

BLUEPRINTS = [
{%- if health_endpoint %}
    health.bp,
{%- endif %}
{%- for resource in resources %}
    {{ resource.name | to_snake }}.bp,
{%- endfor %}
]

__all__ = ["BLUEPRINTS"]
{%- else -%}
# Each module holds one endpoint's route table; {% if cloud_service == 'GCP Cloud Function' %}main.py{% else %}lambda_app.py{% endif %} dispatches every
# request through ROUTES.
from . import (
{%- if health_endpoint %}
    health,
{%- endif %}
{%- for resource in resources %}
    {{ resource.name | to_snake }},
{%- endfor %}
)

# endpoint (first path segment) -> (HTTP method, path has an item id) -> handler
ROUTES = {
    module.ENDPOINT: module.ROUTES
    for module in (
{%- if health_endpoint %}
        health,
{%- endif %}
{%- for resource in resources %}
        {{ resource.name | to_snake }},
{%- endfor %}
    )
}

__all__ = ["ROUTES"]
{%- endif %}
