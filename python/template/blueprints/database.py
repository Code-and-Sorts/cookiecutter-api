{%- if cloud_service == 'Azure Function App' -%}
import asyncio
import azure.functions as func
from azure.cosmos.aio import ContainerProxy, CosmosClient
from config import get_settings
from utils import detect_error, response_generator
from utils.deadline import within_deadline
from utils.user_id import user_id_from

# One client per worker, as the SDK recommends; all functions share one event loop.
_client: CosmosClient | None = None
_client_lock = asyncio.Lock()

# Few, short retries so a failing database answers inside the request deadline.
CLIENT_OPTIONS = {
    "timeout": 5,
    "connection_timeout": 2,
    "read_timeout": 3,
    "retry_total": 2,
    "retry_connect": 1,
    "retry_read": 1,
    "retry_backoff_max": 1,
}


def cosmos_client_options(settings) -> dict:
    if not settings.cosmos_db_emulator:
        return CLIENT_OPTIONS
    # The emulator advertises its own address, which discovery would use instead of the configured one.
    options = {**CLIENT_OPTIONS, "enable_endpoint_discovery": False}
    if settings.cosmos_db_uri.lower().startswith("https://"):
        # Only an emulator serving HTTPS gets here; its certificate is self-signed.
        options["connection_verify"] = False
    return options


async def get_container(container_id: str) -> ContainerProxy:
    global _client
    settings = get_settings()
    async with _client_lock:
        if _client is None:
            client = CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key, **cosmos_client_options(settings))
            await client.__aenter__()
            _client = client
    database = _client.get_database_client(settings.cosmos_db_database_name)
    return database.get_container_client(settings.container_names[container_id])


async def handle(
    container_id, build_controller, operation, status_code: int = 200, request: func.HttpRequest | None = None
) -> func.HttpResponse:
    """Writes pass `request`; `operation` then also receives its user id, read before the database is touched."""
    async def run(*args):
        return await operation(build_controller(await get_container(container_id)), *args)

    try:
        args = () if request is None else (user_id_from(request),)
        return response_generator(await within_deadline(run(*args)), status_code)
    except Exception as error:
        return detect_error(error)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' -%}
# Requests run on worker threads but the async Firestore client is bound to its event
# loop, so one background loop owns it; a loop per request fails with "Event loop is closed".
import asyncio
import threading
from google.cloud import firestore
from config import get_settings
from utils.deadline import DATABASE_TIMEOUT_SECONDS, within_deadline

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
    # Runs on the background loop, so the client is created there.
    global _client
    settings = get_settings()
    if _client is None:
        _client = firestore.AsyncClient(project=settings.gcp_project_id, database=settings.firestore_database)
    return _client.collection(settings.collections[container_id])


def run(container_id, build_controller, operation):
    async def _run():
        return await within_deadline(operation(build_controller(_collection(container_id))))

    future = asyncio.run_coroutine_threadsafe(_run(), _event_loop())
    # The coroutine enforces the deadline itself; this only guards the thread.
    return future.result(timeout=DATABASE_TIMEOUT_SECONDS + 1)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' -%}
import asyncio
from collections.abc import Coroutine
import aioboto3
from utils.deadline import within_deadline

# Sessions are not tied to an event loop, so one serves every invocation.
session = aioboto3.Session()


def run[T](operation: Coroutine[object, object, T]) -> T:
    return asyncio.run(within_deadline(operation))
{%- endif %}
