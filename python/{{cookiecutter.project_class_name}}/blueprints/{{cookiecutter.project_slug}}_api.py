import logging
from controllers import {{ cookiecutter.project_class_name }}Controller
from services import {{ cookiecutter.project_class_name }}Service
from repositories import {{ cookiecutter.project_class_name }}Repository
from config import get_settings
from utils import detect_error, response_generator
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import azure.functions as func
from azure.cosmos.aio import CosmosClient

bp = func.Blueprint()

settings = get_settings()
client = CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key)
database_client = client.get_database_client(settings.cosmos_db_database_name)
container_client = database_client.get_container_client(settings.cosmos_db_container_name)
repository = {{ cookiecutter.project_class_name }}Repository(container_client)
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
import asyncio
from google.cloud import firestore
from flask import Request

settings = get_settings()


async def _run(operation):
    # Build the async Firestore client inside the request's event loop so its
    # gRPC transport binds to the loop that drives it. Each invocation runs on
    # a fresh asyncio.run() loop, so a module-level client would be bound to an
    # already-closed loop on subsequent requests.
    db = firestore.AsyncClient(
        project=settings.gcp_project_id,
        database=settings.firestore_database
    )
    try:
        collection = db.collection(settings.firestore_collection)
        repository = {{ cookiecutter.project_class_name }}Repository(collection)
        service = {{ cookiecutter.project_class_name }}Service(repository)
        controller = {{ cookiecutter.project_class_name }}Controller(service)
        return await operation(controller)
    finally:
        db.close()
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
import asyncio
import aioboto3

settings = get_settings()
# A single aioboto3 session is reused; each call opens a short-lived
# async resource context so DynamoDB I/O is non-blocking.
session = aioboto3.Session()
repository = {{ cookiecutter.project_class_name }}Repository(
    session,
    settings.dynamodb_table_name,
    settings.aws_region
)
{%- endif %}

{% if cookiecutter.cloud_service != 'GCP Cloud Function' -%}
service = {{ cookiecutter.project_class_name }}Service(repository)
controller = {{ cookiecutter.project_class_name }}Controller(service)
{%- endif %}

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="health", methods=[func.HttpMethod.GET])
async def health(req: func.HttpRequest) -> func.HttpResponse:
    return func.HttpResponse(
        body='{"status": "ok"}',
        status_code=200,
        mimetype="application/json"
    )
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def health(request: Request):
    """HTTP Cloud Function liveness probe."""
    return ('{"status": "ok"}', 200, {'Content-Type': 'application/json'})
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
def health(event):
    """Lambda handler liveness probe."""
    return {
        "statusCode": 200,
        "headers": {"Content-Type": "application/json"},
        "body": '{"status": "ok"}'
    }
{%- endif %}


{% if cookiecutter.cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
def get_by_id(request: Request):
    """HTTP Cloud Function to get item by ID."""
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
def get_by_id(event):
    """Lambda handler to get item by ID."""
{%- endif %}
    logging.info("Get {{ cookiecutter.project_endpoint }} by ID processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        item = await controller.get_by_id(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        item = asyncio.run(_run(lambda c: c.get_by_id(request)))
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        item = asyncio.run(controller.get_by_id(event))
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
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
def get_list(event):
    """Lambda handler to get all items."""
{%- endif %}
    logging.info("Get {{ cookiecutter.project_endpoint }} list processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        items = await controller.get_list(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        items = asyncio.run(_run(lambda c: c.get_list(request)))
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        items = asyncio.run(controller.get_list(event))
{%- endif %}
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
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
def create(event):
    """Lambda handler to create a new item."""
{%- endif %}
    logging.info("Create item processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        created_item = await controller.create(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        created_item = asyncio.run(_run(lambda c: c.create(request)))
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        created_item = asyncio.run(controller.create(event))
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
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
def update(event):
    """Lambda handler to update an existing item."""
{%- endif %}
    logging.info("Patch item processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        updated_item = await controller.update(req)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        updated_item = asyncio.run(_run(lambda c: c.update(request)))
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        updated_item = asyncio.run(controller.update(event))
{%- endif %}
        return response_generator(updated_item, 200)

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
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
def delete(event):
    """Lambda handler to soft delete an item."""
{%- endif %}
    logging.info("Delete item processed a request.")

    try:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        await controller.soft_delete(req)
        return func.HttpResponse(
            body="{{ cookiecutter.project_class_name }} deleted.",
            status_code=200
        )
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        asyncio.run(_run(lambda c: c.soft_delete(request)))
        return ("{{ cookiecutter.project_class_name }} deleted.", 200)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        asyncio.run(controller.soft_delete(event))
        return {
            "statusCode": 200,
            "headers": {"Content-Type": "application/json"},
            "body": '{"message": "{{ cookiecutter.project_class_name }} deleted."}'
        }
{%- endif %}

    except Exception as error:
        return detect_error(error)
