import logging
from controllers import (
    CatController,
    DogController,
)
from services import (
    CatService,
    DogService,
)
from repositories import (
    CatRepository,
    DogRepository,
)
from config import get_settings
from utils import detect_error, response_generator
import azure.functions as func
from azure.cosmos.aio import CosmosClient

bp = func.Blueprint()

settings = get_settings()

def _build_cat_controller(container_client):
    return CatController(CatService(CatRepository(container_client)))


def _build_dog_controller(container_client):
    return DogController(DogService(DogRepository(container_client)))


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



@bp.route(route="cats/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get cats by ID processed a request.")
    try:
        item = await _run("animals", _build_cat_controller, lambda c: c.get_by_id(req))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)

@bp.route(route="cats", methods=[func.HttpMethod.GET])
async def get_list_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get cats list processed a request.")
    try:
        items = await _run("animals", _build_cat_controller, lambda c: c.get_list(req))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)

@bp.route(route="cats", methods=[func.HttpMethod.POST])
async def create_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create cats processed a request.")
    try:
        created_item = await _run("animals", _build_cat_controller, lambda c: c.create(req))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)

@bp.route(route="cats/{item_id}", methods=[func.HttpMethod.PATCH])
async def update_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch cats processed a request.")
    try:
        updated_item = await _run("animals", _build_cat_controller, lambda c: c.update(req))
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)

@bp.route(route="cats/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete_cat(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete cats processed a request.")
    try:
        await _run("animals", _build_cat_controller, lambda c: c.soft_delete(req))
        return func.HttpResponse(
            body="Cat deleted.",
            status_code=200
        )
    except Exception as error:
        return detect_error(error)


@bp.route(route="dogs/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get dogs by ID processed a request.")
    try:
        item = await _run("animals", _build_dog_controller, lambda c: c.get_by_id(req))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)

@bp.route(route="dogs", methods=[func.HttpMethod.GET])
async def get_list_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get dogs list processed a request.")
    try:
        items = await _run("animals", _build_dog_controller, lambda c: c.get_list(req))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)

@bp.route(route="dogs", methods=[func.HttpMethod.POST])
async def create_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create dogs processed a request.")
    try:
        created_item = await _run("animals", _build_dog_controller, lambda c: c.create(req))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)

@bp.route(route="dogs/{item_id}", methods=[func.HttpMethod.PUT])
async def replace_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Replace dogs processed a request.")
    try:
        replaced_item = await _run("animals", _build_dog_controller, lambda c: c.replace(req))
        return response_generator(replaced_item, 200)
    except Exception as error:
        return detect_error(error)

@bp.route(route="dogs/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete_dog(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete dogs processed a request.")
    try:
        await _run("animals", _build_dog_controller, lambda c: c.soft_delete(req))
        return func.HttpResponse(
            body="Dog deleted.",
            status_code=200
        )
    except Exception as error:
        return detect_error(error)

