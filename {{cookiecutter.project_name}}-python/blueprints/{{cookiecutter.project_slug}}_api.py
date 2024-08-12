import os
import logging
import azure.functions as func
from controllers import {{ cookiecutter.project_class_name }}Controller
from services import {{ cookiecutter.project_class_name }}Service
from repositories import {{ cookiecutter.project_class_name }}Repository, Database
from utils import detect_error, response_generator

bp = func.Blueprint()

database = Database(
    endpoint=os.getenv("Cosmos_Db_Uri"),
    key=os.getenv("Cosmos_Db_Key"),
    name=os.getenv("Cosmos_Db_Database_Name"),
    container_name=os.getenv("Cosmos_Db_Container_Name")
)
repository = {{ cookiecutter.project_class_name }}Repository(database)
service = {{ cookiecutter.project_class_name }}Service(repository)
controller = {{ cookiecutter.project_class_name }}Controller(service)

@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get {{ cookiecutter.project_endpoint }} by ID processed a request.")

    try:
        item = controller.get_by_id(req)
        return response_generator(item)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}", methods=[func.HttpMethod.GET])
async def get_list(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get {{ cookiecutter.project_endpoint }} list processed a request.")

    try:
        items = controller.get_list()
        return response_generator(items)

    except Exception as error:
        print(str(error))
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}", methods=[func.HttpMethod.POST])
async def create(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create item processed a request.")

    try:
        created_item = controller.create(req)
        return response_generator(created_item, 201)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.PATCH])
async def update(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch item processed a request.")

    try:
        updated_item = controller.update(req)
        return response_generator(updated_item, 201)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete item processed a request.")

    try:
        controller.soft_delete(req)
        return func.HttpResponse(
            body="{{ cookiecutter.project_class_name }} deleted.",
            status_code=200
        )

    except Exception as error:
        return detect_error(error)
