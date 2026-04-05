{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from .{{cookiecutter.project_slug}}_repository import {{ cookiecutter.project_class_name }}Repository, Database

__all__ = ["{{ cookiecutter.project_class_name }}Repository", "Database"]
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
from .{{cookiecutter.project_slug}}_repository import {{ cookiecutter.project_class_name }}Repository

__all__ = ["{{ cookiecutter.project_class_name }}Repository"]
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
from .{{cookiecutter.project_slug}}_repository import {{ cookiecutter.project_class_name }}Repository

__all__ = ["{{ cookiecutter.project_class_name }}Repository"]
{%- endif %}
