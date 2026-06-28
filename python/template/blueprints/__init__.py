{% if cloud_service == 'Azure Function App' -%}
from .{{project_slug}}_api import bp

__all__ = ["bp"]
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
# GCP Cloud Functions don't use blueprints, see main.py for function exports
__all__ = []
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
# AWS Lambda doesn't use blueprints, see lambda_app.py for function routing
__all__ = []
{%- endif %}
