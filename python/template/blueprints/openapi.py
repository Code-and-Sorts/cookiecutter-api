import json
from pathlib import Path
{%- if cloud_service == 'Azure Function App' %}
import azure.functions as func
from utils import response_generator
{%- endif %}

ENDPOINT = "openapi.json"
SPEC = json.loads((Path(__file__).parent.parent / "openapi.json").read_text(encoding="utf-8"))
{%- if cloud_service == 'Azure Function App' %}

bp = func.Blueprint()


@bp.route(route=ENDPOINT, methods=[func.HttpMethod.GET], auth_level=func.AuthLevel.ANONYMOUS)
async def openapi(req: func.HttpRequest) -> func.HttpResponse:
    return response_generator(SPEC)
{%- else %}


def openapi({% if cloud_service == 'GCP Cloud Function' %}request{% else %}event{% endif %}, item_id: str | None) -> tuple[int, dict]:
    return 200, SPEC


ROUTES = {
    ("GET", False): openapi,
}
{%- endif %}
