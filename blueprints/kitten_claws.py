import logging
import azure.functions as func
from controllers import KittenClawsController
from services import KittenClawsService
from repositories import KittenClawsRepository
from .database import handle

CONTAINER = "kittenclaws"

bp = func.Blueprint()


def _build_controller(container_client) -> KittenClawsController:
    return KittenClawsController(KittenClawsService(KittenClawsRepository(container_client)))


@bp.route(route="kittenclaws", methods=[func.HttpMethod.GET])
async def get_list_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("List kittenclaws processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_list(req.params.get("limit")))


@bp.route(route="kittenclaws/{id}", methods=[func.HttpMethod.GET])
async def get_by_id_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get kittenclaws by ID processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_by_id(req.route_params.get("id")))


@bp.route(route="kittenclaws", methods=[func.HttpMethod.POST])
async def create_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create kittenclaws processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.create(req.get_body(), user_id), 201, request=req)


@bp.route(route="kittenclaws/{id}", methods=[func.HttpMethod.PATCH])
async def update_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch kittenclaws processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.update(req.route_params.get("id"), req.get_body(), user_id), request=req)


@bp.route(route="kittenclaws/{id}", methods=[func.HttpMethod.DELETE])
async def delete_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete kittenclaws processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.delete(req.route_params.get("id"), user_id), request=req)
