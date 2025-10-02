import os
import logging
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import azure.functions as func
from azure.cosmos import CosmosClient
from repositories import Database

bp = func.Blueprint()

database = Database(
    endpoint=os.getenv("Cosmos_Db_Uri"),
    key=os.getenv("Cosmos_Db_Key"),
    name=os.getenv("Cosmos_Db_Database_Name"),
    container_name=os.getenv("Cosmos_Db_Container_Name")
)
client = CosmosClient(database._endpoint, database._key)
database_client = client.get_database_client(database.name)
container_client = database_client.get_container_client(database.container_name)
repository = {{ cookiecutter.project_class_name }}Repository(container_client)
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
from google.cloud import firestore
from flask import Request

# Initialize Firestore client
db = firestore.Client(
    project=os.getenv("GCP_PROJECT_ID"),
    database=os.getenv("FIRESTORE_DATABASE", "(default)")
)
collection_name = os.getenv("FIRESTORE_COLLECTION", "{{ cookiecutter.project_slug }}")
collection = db.collection(collection_name)
repository = {{ cookiecutter.project_class_name }}Repository(collection)
{%- endif %}
from controllers import {{ cookiecutter.project_class_name }}Controller
from services import {{ cookiecutter.project_class_name }}Service
from repositories import {{ cookiecutter.project_class_name }}Repository
from utils import detect_error, response_generator

service = {{ cookiecutter.project_class_name }}Service(repository)
controller = {{ cookiecutter.project_class_name }}Controller(service)

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def get_by_id(request: Request):
    """HTTP Cloud Function to get item by ID."""
{%- endif %}
    logging.info("Get {{ cookiecutter.project_endpoint }} by ID processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        item = controller.get_by_id(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        item = controller.get_by_id(request)
{%- endif %}
        return response_generator(item)

    except Exception as error:
        return detect_error(error)


{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ cookiecutter.project_endpoint }}", methods=[func.HttpMethod.GET])
async def get_list(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def get_list(request: Request):
    """HTTP Cloud Function to get all items."""
{%- endif %}
    logging.info("Get {{ cookiecutter.project_endpoint }} list processed a request.")

    try:
        items = controller.get_list()
        return response_generator(items)

    except Exception as error:
        return detect_error(error)


{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ cookiecutter.project_endpoint }}", methods=[func.HttpMethod.POST])
async def create(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def create(request: Request):
    """HTTP Cloud Function to create a new item."""
{%- endif %}
    logging.info("Create item processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        created_item = controller.create(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        created_item = controller.create(request)
{%- endif %}
        return response_generator(created_item, 201)

    except Exception as error:
        return detect_error(error)


{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.PATCH])
async def update(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def update(request: Request):
    """HTTP Cloud Function to update an existing item."""
{%- endif %}
    logging.info("Patch item processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        updated_item = controller.update(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        updated_item = controller.update(request)
{%- endif %}
        return response_generator(updated_item, 201)

    except Exception as error:
        return detect_error(error)


{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def delete(request: Request):
    """HTTP Cloud Function to soft delete an item."""
{%- endif %}
    logging.info("Delete item processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        controller.soft_delete(req)
        return func.HttpResponse(
            body="{{ cookiecutter.project_class_name }} deleted.",
            status_code=200
        )
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        controller.soft_delete(request)
        return ("{{ cookiecutter.project_class_name }} deleted.", 200)
{%- endif %}

    except Exception as error:
        return detect_error(error)
