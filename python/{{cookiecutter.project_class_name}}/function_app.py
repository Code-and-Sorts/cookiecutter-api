{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import azure.functions as func
from blueprints import bp

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

app.register_functions(bp)
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
import functions_framework
from blueprints import app

# Register the Google Cloud Functions
# The actual endpoints are defined in blueprints/{{cookiecutter.project_slug}}_api.py
{%- endif %}
