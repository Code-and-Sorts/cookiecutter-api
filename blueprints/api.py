import logging
from controllers import (
    KittenClawsController,
)
from services import (
    KittenClawsService,
)
from repositories import (
    KittenClawsRepository,
)
from config import get_settings
from utils import detect_error, response_generator
import azure.functions as func
from azure.cosmos.aio import CosmosClient

bp = func.Blueprint()

settings = get_settings()

def _build_kitten_claws_controller(container_client):
    return KittenClawsController(KittenClawsService(KittenClawsRepository(container_client)))


async def _run(container_id, build_controller, operation):
    async with CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key) as client:
        database_client = client.get_database_client(settings.cosmos_db_database_name)
        container_client = database_client.get_container_client(settings.container_names[container_id])
        return await operation(build_controller(container_client))



@bp.route(route="health", methods=[func.HttpMethod.GET])
async def health(req: func.HttpRequest) -> func.HttpResponse:
    return func.HttpResponse(
        body='{"status": "ok"}',
        status_code=200,
        mimetype="application/json"
    )



@bp.route(route="kitties/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get kitties by ID processed a request.")
    try:
        item = await _run("kitty_cats", _build_kitten_claws_controller, lambda c: c.get_by_id(req))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)

@bp.route(route="kitties", methods=[func.HttpMethod.GET])
async def get_list_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get kitties list processed a request.")
    try:
        items = await _run("kitty_cats", _build_kitten_claws_controller, lambda c: c.get_list(req))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)

@bp.route(route="kitties", methods=[func.HttpMethod.POST])
async def create_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create kitties processed a request.")
    try:
        created_item = await _run("kitty_cats", _build_kitten_claws_controller, lambda c: c.create(req))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)

@bp.route(route="kitties/{item_id}", methods=[func.HttpMethod.PATCH])
async def update_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch kitties processed a request.")
    try:
        updated_item = await _run("kitty_cats", _build_kitten_claws_controller, lambda c: c.update(req))
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)

@bp.route(route="kitties/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete_kitten_claws(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete kitties processed a request.")
    try:
        await _run("kitty_cats", _build_kitten_claws_controller, lambda c: c.soft_delete(req))
        return func.HttpResponse(
            body="KittenClaws deleted.",
            status_code=200
        )
    except Exception as error:
        return detect_error(error)

