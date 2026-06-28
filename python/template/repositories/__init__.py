{% if cloud_service == 'Azure Function App' -%}
from .{{project_slug}}_repository import {{ project_class_name }}Repository, Database

__all__ = ["{{ project_class_name }}Repository", "Database"]
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
from .{{project_slug}}_repository import {{ project_class_name }}Repository

__all__ = ["{{ project_class_name }}Repository"]
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
from .{{project_slug}}_repository import {{ project_class_name }}Repository

__all__ = ["{{ project_class_name }}Repository"]
{%- endif %}
