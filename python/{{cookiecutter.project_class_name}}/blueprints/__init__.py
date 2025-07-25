{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from .{{cookiecutter.project_slug}}_api import bp

__all__ = ["bp"]
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
from .{{cookiecutter.project_slug}}_api import app

__all__ = ["app"]
{%- endif %}
