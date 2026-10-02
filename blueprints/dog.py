import logging
import azure.functions as func
from controllers import DogController
from services import DogService
from repositories import DogRepository
from .database import handle

CONTAINER = "animals"

bp = func.Blueprint()


def _build_controller(container_client) -> DogController:
    return DogController(DogService(DogRepository(container_client)))


@bp.route(route="dogs", methods=[func.HttpMethod.GET])
async def get_list_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("List dogs processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_list(req.params.get("limit")))


@bp.route(route="dogs/{id}", methods=[func.HttpMethod.GET])
async def get_by_id_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get dogs by ID processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c: c.get_by_id(req.route_params.get("id")))


@bp.route(route="dogs", methods=[func.HttpMethod.POST])
async def create_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create dogs processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.create(req.get_body(), user_id), 201, request=req)


@bp.route(route="dogs/{id}", methods=[func.HttpMethod.PUT])
async def replace_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Replace dogs processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.replace(req.route_params.get("id"), req.get_body(), user_id), request=req)


@bp.route(route="dogs/{id}", methods=[func.HttpMethod.DELETE])
async def delete_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete dogs processed a request.")
    return await handle(CONTAINER, _build_controller, lambda c, user_id: c.delete(req.route_params.get("id"), user_id), request=req)
