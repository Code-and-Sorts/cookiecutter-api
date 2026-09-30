from blueprints import ROUTES
from utils import detect_error, response_generator
from utils.routing import resolve


def lambda_handler(event, context):
    try:
        # "resource" is the matched route template (e.g. /cats/{id}); the id is in pathParameters.
        route = event.get("resource") or event.get("path") or ""
        handler, path_id = resolve(ROUTES, event.get("httpMethod", ""), route)
        item_id = (event.get("pathParameters") or {}).get("id", path_id)
        status_code, payload = handler(event, item_id)
        return response_generator(payload, status_code)
    except Exception as error:
        return detect_error(error)
