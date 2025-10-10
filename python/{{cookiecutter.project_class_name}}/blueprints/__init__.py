{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from .{{cookiecutter.project_slug}}_api import bp

__all__ = ["bp"]
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
# GCP Cloud Functions don't use blueprints, see main.py for function exports
__all__ = []
{%- endif %}
