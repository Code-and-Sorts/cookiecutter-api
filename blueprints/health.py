import azure.functions as func
from utils import response_generator

ENDPOINT = "health"
BODY = {"status": "ok"}

bp = func.Blueprint()


# Anonymous, unlike the resource functions: load balancers and uptime probes
# call it without a function key.
@bp.route(route=ENDPOINT, methods=[func.HttpMethod.GET], auth_level=func.AuthLevel.ANONYMOUS)
async def health(req: func.HttpRequest) -> func.HttpResponse:
    return response_generator(BODY)
