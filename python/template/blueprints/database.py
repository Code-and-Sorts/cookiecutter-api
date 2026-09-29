{%- if cloud_service == 'Azure Function App' -%}
"""Cosmos DB wiring shared by every resource's functions."""
import asyncio
import azure.functions as func
from azure.cosmos.aio import ContainerProxy, CosmosClient
from config import get_settings
from utils import detect_error, response_generator

# One client for the life of the worker, as the Cosmos DB SDK recommends: the
# Functions worker runs every async function on the same event loop.
_client: CosmosClient | None = None
_client_lock = asyncio.Lock()


async def get_container(container_id: str) -> ContainerProxy:
    """The Cosmos DB container for ``container_id``, opening the client on first use."""
    global _client
    settings = get_settings()
    async with _client_lock:
        if _client is None:
            client = CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key)
            await client.__aenter__()
            _client = client
    database = _client.get_database_client(settings.cosmos_db_database_name)
    return database.get_container_client(settings.container_names[container_id])


async def handle(container_id, build_controller, operation, status_code: int = 200) -> func.HttpResponse:
    """Build the controller for ``container_id``, run ``operation`` against it
    and answer with its result as JSON, or with the error it raised."""
    try:
        controller = build_controller(await get_container(container_id))
        return response_generator(await operation(controller), status_code)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' -%}
"""Firestore wiring shared by every resource's routes.

The Functions Framework serves each request on a worker thread, while the async
Firestore client (built on grpc.aio) is bound to the event loop it was created
on. So one background event loop runs for the life of the process and owns a
single ``AsyncClient``; each request submits its coroutine to that loop and
waits for the result. (Creating a client and an event loop per request leaves
gRPC channels to be cleaned up on a closed loop: "Event loop is closed".)
"""
import asyncio
import threading
from google.cloud import firestore
from config import get_settings

_lock = threading.Lock()
_loop: asyncio.AbstractEventLoop | None = None
_client: firestore.AsyncClient | None = None


def _event_loop() -> asyncio.AbstractEventLoop:
    global _loop
    with _lock:
        if _loop is None:
            loop = asyncio.new_event_loop()
            threading.Thread(target=loop.run_forever, name="firestore-event-loop", daemon=True).start()
            _loop = loop
    return _loop


def _collection(container_id: str) -> firestore.AsyncCollectionReference:
    """The Firestore collection for ``container_id``. Runs on the background
    loop, which creates the client on first use."""
    global _client
    settings = get_settings()
    if _client is None:
        _client = firestore.AsyncClient(project=settings.gcp_project_id, database=settings.firestore_database)
    return _client.collection(settings.collections[container_id])


def run(container_id, build_controller, operation):
    """Build the controller for ``container_id`` and run ``operation`` against it
    on the Firestore event loop, returning its result."""
    async def _run():
        return await operation(build_controller(_collection(container_id)))

    return asyncio.run_coroutine_threadsafe(_run(), _event_loop()).result()
{%- endif %}
{%- if cloud_service == 'AWS Lambda' -%}
"""DynamoDB wiring shared by every resource's handlers."""
import aioboto3

# Sessions are not tied to an event loop, so one serves every invocation.
session = aioboto3.Session()
{%- endif %}
