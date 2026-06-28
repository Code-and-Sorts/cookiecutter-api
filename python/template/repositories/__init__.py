{% if cloud_service == 'Azure Function App' -%}
from .repository import Repository, Database

__all__ = ["Repository", "Database"]
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
from .repository import Repository

__all__ = ["Repository"]
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
from .repository import Repository

__all__ = ["Repository"]
{%- endif %}
