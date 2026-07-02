import logging
from controllers import (
{%- for resource in resources %}
    {{ resource.name }}Controller,
{%- endfor %}
)
from services import (
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
)
from repositories import (
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
)
from config import get_settings
from utils import detect_error, response_generator
{% if cloud_service == 'Azure Function App' -%}
import azure.functions as func
from azure.cosmos.aio import CosmosClient

bp = func.Blueprint()

settings = get_settings()

{% for resource in resources -%}
def _build_{{ resource.name | to_snake }}_controller(container_client):
    return {{ resource.name }}Controller({{ resource.name }}Service({{ resource.name }}Repository(container_client)))


{% endfor -%}
async def _run(container_id, build_controller, operation):
    # Scope the async Cosmos client to the request via `async with` so its
    # aiohttp session is always closed, avoiding leaked/unclosed sessions.
    async with CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key) as client:
        database_client = client.get_database_client(settings.cosmos_db_database_name)
        container_client = database_client.get_container_client(settings.container_names[container_id])
        return await operation(build_controller(container_client))
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
import asyncio
from google.cloud import firestore
from flask import Request

settings = get_settings()

{% for resource in resources -%}
def _build_{{ resource.name | to_snake }}_controller(collection):
    return {{ resource.name }}Controller({{ resource.name }}Service({{ resource.name }}Repository(collection)))


{% endfor -%}
async def _run(container_id, build_controller, operation):
    # Build the async Firestore client inside the request's event loop so its
    # gRPC transport binds to the loop that drives it. Each invocation runs on
    # a fresh asyncio.run() loop, so a module-level client would be bound to an
    # already-closed loop on subsequent requests.
    db = firestore.AsyncClient(
        project=settings.gcp_project_id,
        database=settings.firestore_database
    )
    try:
        collection = db.collection(settings.collections[container_id])
        return await operation(build_controller(collection))
    finally:
        db.close()
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
import asyncio
import aioboto3

settings = get_settings()
# A single aioboto3 session is reused; each call opens a short-lived async
# resource context so DynamoDB I/O is non-blocking. One controller is wired
# per resource, its repository bound to that resource's storage table.
session = aioboto3.Session()
{%- for resource in resources %}
_{{ resource.name | to_snake }}_controller = {{ resource.name }}Controller({{ resource.name }}Service({{ resource.name }}Repository(
    session, settings.tables["{{ resource.container }}"], settings.aws_region
)))
{%- endfor %}
{%- endif %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="health", methods=[func.HttpMethod.GET])
async def health(req: func.HttpRequest) -> func.HttpResponse:
    return func.HttpResponse(
        body='{"status": "ok"}',
        status_code=200,
        mimetype="application/json"
    )
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def health(request: Request):
    """HTTP Cloud Function liveness probe."""
    return ('{"status": "ok"}', 200, {'Content-Type': 'application/json'})
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def health(event):
    """Lambda handler liveness probe."""
    return {
        "statusCode": 200,
        "headers": {"Content-Type": "application/json"},
        "body": '{"status": "ok"}'
    }
{%- endif %}

{% for resource in resources %}
{%- set slug = resource.name | to_snake %}
{%- if "get_by_id" in resource.operations %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ resource.endpoint }}/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id_{{ slug }}(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def get_by_id_{{ slug }}(request: Request):
    """HTTP Cloud Function to get a {{ resource.name }} by ID."""
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def get_by_id_{{ slug }}(event):
    """Lambda handler to get a {{ resource.name }} by ID."""
{%- endif %}
    logging.info("Get {{ resource.endpoint }} by ID processed a request.")
    try:
{%- if cloud_service == 'Azure Function App' %}
        item = await _run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.get_by_id(req))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        item = asyncio.run(_run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.get_by_id(request)))
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        item = asyncio.run(_{{ slug }}_controller.get_by_id(event))
{%- endif %}
        return response_generator(item)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if "list" in resource.operations %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ resource.endpoint }}", methods=[func.HttpMethod.GET])
async def get_list_{{ slug }}(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def get_list_{{ slug }}(request: Request):
    """HTTP Cloud Function to list {{ resource.name }} items."""
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def get_list_{{ slug }}(event):
    """Lambda handler to list {{ resource.name }} items."""
{%- endif %}
    logging.info("Get {{ resource.endpoint }} list processed a request.")
    try:
{%- if cloud_service == 'Azure Function App' %}
        items = await _run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.get_list(req))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        items = asyncio.run(_run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.get_list(request)))
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        items = asyncio.run(_{{ slug }}_controller.get_list(event))
{%- endif %}
        return response_generator(items)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if "create" in resource.operations %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ resource.endpoint }}", methods=[func.HttpMethod.POST])
async def create_{{ slug }}(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def create_{{ slug }}(request: Request):
    """HTTP Cloud Function to create a {{ resource.name }}."""
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def create_{{ slug }}(event):
    """Lambda handler to create a {{ resource.name }}."""
{%- endif %}
    logging.info("Create {{ resource.endpoint }} processed a request.")
    try:
{%- if cloud_service == 'Azure Function App' %}
        created_item = await _run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.create(req))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        created_item = asyncio.run(_run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.create(request)))
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        created_item = asyncio.run(_{{ slug }}_controller.create(event))
{%- endif %}
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if "update" in resource.operations %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ resource.endpoint }}/{item_id}", methods=[func.HttpMethod.PATCH])
async def update_{{ slug }}(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def update_{{ slug }}(request: Request):
    """HTTP Cloud Function to patch a {{ resource.name }}."""
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def update_{{ slug }}(event):
    """Lambda handler to patch a {{ resource.name }}."""
{%- endif %}
    logging.info("Patch {{ resource.endpoint }} processed a request.")
    try:
{%- if cloud_service == 'Azure Function App' %}
        updated_item = await _run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.update(req))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        updated_item = asyncio.run(_run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.update(request)))
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        updated_item = asyncio.run(_{{ slug }}_controller.update(event))
{%- endif %}
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if "replace" in resource.operations %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ resource.endpoint }}/{item_id}", methods=[func.HttpMethod.PUT])
async def replace_{{ slug }}(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def replace_{{ slug }}(request: Request):
    """HTTP Cloud Function to replace a {{ resource.name }}."""
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def replace_{{ slug }}(event):
    """Lambda handler to replace a {{ resource.name }}."""
{%- endif %}
    logging.info("Replace {{ resource.endpoint }} processed a request.")
    try:
{%- if cloud_service == 'Azure Function App' %}
        replaced_item = await _run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.replace(req))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        replaced_item = asyncio.run(_run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.replace(request)))
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        replaced_item = asyncio.run(_{{ slug }}_controller.replace(event))
{%- endif %}
        return response_generator(replaced_item, 200)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if "delete" in resource.operations %}

{% if cloud_service == 'Azure Function App' -%}
@bp.route(route="{{ resource.endpoint }}/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete_{{ slug }}(req: func.HttpRequest) -> func.HttpResponse:
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
def delete_{{ slug }}(request: Request):
    """HTTP Cloud Function to soft delete a {{ resource.name }}."""
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
def delete_{{ slug }}(event):
    """Lambda handler to soft delete a {{ resource.name }}."""
{%- endif %}
    logging.info("Delete {{ resource.endpoint }} processed a request.")
    try:
{%- if cloud_service == 'Azure Function App' %}
        await _run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.soft_delete(req))
        return func.HttpResponse(
            body="{{ resource.name }} deleted.",
            status_code=200
        )
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        asyncio.run(_run("{{ resource.container }}", _build_{{ slug }}_controller, lambda c: c.soft_delete(request)))
        return ("{{ resource.name }} deleted.", 200)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        asyncio.run(_{{ slug }}_controller.soft_delete(event))
        return {
            "statusCode": 200,
            "headers": {"Content-Type": "application/json"},
            "body": '{"message": "{{ resource.name }} deleted."}'
        }
{%- endif %}
    except Exception as error:
        return detect_error(error)
{%- endif %}
{% endfor %}
{%- if cloud_service == 'AWS Lambda' %}

# Dispatch table for the Lambda handler: endpoint -> {(method, needs_id): handler}.
ROUTES = {
{%- for resource in resources %}
{%- set slug = resource.name | to_snake %}
    "{{ resource.endpoint }}": {
{%- if "list" in resource.operations %}
        ("GET", False): get_list_{{ slug }},
{%- endif %}
{%- if "get_by_id" in resource.operations %}
        ("GET", True): get_by_id_{{ slug }},
{%- endif %}
{%- if "create" in resource.operations %}
        ("POST", False): create_{{ slug }},
{%- endif %}
{%- if "update" in resource.operations %}
        ("PATCH", True): update_{{ slug }},
{%- endif %}
{%- if "replace" in resource.operations %}
        ("PUT", True): replace_{{ slug }},
{%- endif %}
{%- if "delete" in resource.operations %}
        ("DELETE", True): delete_{{ slug }},
{%- endif %}
    },
{%- endfor %}
}
{%- endif %}
