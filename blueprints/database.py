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
