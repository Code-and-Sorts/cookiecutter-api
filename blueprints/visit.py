import logging
import azure.functions as func
from controllers import VisitController
from services import VisitService
from repositories import VisitRepository
from .database import handle

CONTAINER = "visits"

bp = func.Blueprint()


def _build_controller(container_client) -> VisitController:
    return VisitController(VisitService(VisitRepository(container_client)))


@bp.route(route="visits/{id}", methods=[func.HttpMethod.GET])
async def get_by_id_visit(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get visits by ID processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_by_id(req.route_params.get("id")))


@bp.route(route="visits", methods=[func.HttpMethod.POST])
async def create_visit(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create visits processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.create(req.get_body(), user_id), 201, request=req)


@bp.route(route="visits/{id}", methods=[func.HttpMethod.PATCH])
async def update_visit(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch visits processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.update(req.route_params.get("id"), req.get_body(), user_id), request=req)
