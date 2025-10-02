{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import azure.functions as func
from blueprints import bp

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

app.register_functions(bp)
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
# GCP Cloud Functions use main.py as entry point
# This file is not used for GCP deployments
pass
{%- endif %}
