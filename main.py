import functions_framework
from flask import Request
from blueprints import ROUTES
from utils import detect_error, response_generator
from utils.routing import resolve


@functions_framework.http
def api(request: Request):
    try:
        handler, item_id = resolve(ROUTES, request.method, request.path)
        status_code, payload = handler(request, item_id)
        return response_generator(payload, status_code)
    except Exception as error:
        return detect_error(error)
