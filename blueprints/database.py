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


async def get_container(container_id: str) -> ContainerProxy:
    global _client
    settings = get_settings()
    async with _client_lock:
        if _client is None:
            client = CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key, **CLIENT_OPTIONS)
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
