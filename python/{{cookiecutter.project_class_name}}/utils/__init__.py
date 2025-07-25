{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from .detect_error import detect_error
from .response_generator import response_generator

__all__ = ["detect_error","response_generator"]
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
from .detect_error import detect_error
from .response_generator import response_generator_gcp

__all__ = ["detect_error","response_generator_gcp"]
{%- endif %}
