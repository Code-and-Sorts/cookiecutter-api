{%- if cloud_service == 'Azure Function App' -%}
import azure.functions as func

bp = func.Blueprint()


@bp.route(route="health", methods=[func.HttpMethod.GET])
async def health(req: func.HttpRequest) -> func.HttpResponse:
    return func.HttpResponse(
        body='{"status": "ok"}',
        status_code=200,
        mimetype="application/json"
    )
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' -%}
from flask import Request


def health(request: Request):
    """HTTP Cloud Function liveness probe."""
    return ('{"status": "ok"}', 200, {'Content-Type': 'application/json'})
{%- endif %}
{%- if cloud_service == 'AWS Lambda' -%}
def health(event):
    """Lambda handler liveness probe."""
    return {
        "statusCode": 200,
        "headers": {"Content-Type": "application/json"},
        "body": '{"status": "ok"}'
    }
{%- endif %}
