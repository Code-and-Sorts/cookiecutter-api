import logging
import azure.functions as func
from controllers import CatController
from services import CatService
from repositories import CatRepository
from .database import handle

CONTAINER = "animals"

bp = func.Blueprint()


def _build_controller(container_client) -> CatController:
    return CatController(CatService(CatRepository(container_client)))


@bp.route(route="cats", methods=[func.HttpMethod.GET])
async def get_list_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("List cats processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_list(req.params.get("limit")))


@bp.route(route="cats/{id}", methods=[func.HttpMethod.GET])
async def get_by_id_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get cats by ID processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_by_id(req.route_params.get("id")))


@bp.route(route="cats", methods=[func.HttpMethod.POST])
async def create_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create cats processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.create(req.get_body(), user_id), 201, request=req)


@bp.route(route="cats/{id}", methods=[func.HttpMethod.PATCH])
async def update_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch cats processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.update(req.route_params.get("id"), req.get_body(), user_id), request=req)


@bp.route(route="cats/{id}", methods=[func.HttpMethod.DELETE])
async def delete_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete cats processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.delete(req.route_params.get("id"), user_id), request=req)
