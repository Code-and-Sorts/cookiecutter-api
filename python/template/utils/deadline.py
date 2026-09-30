"""A per-request deadline for database work."""
import asyncio
from collections.abc import Awaitable

# Every request that touches the database must finish within this many seconds,
# so a failing or unreachable database ends in a 500 well inside the platform's
# own timeout. The SDK clients are also configured with short per-call timeouts
# and capped retries (see {% if cloud_service == 'Azure Function App' %}blueprints/database.py{% else %}repositories/base_repository.py{% endif %}); this bounds the
# request as a whole, however many calls it makes.
DATABASE_TIMEOUT_SECONDS = 8.0


async def within_deadline[T](awaitable: Awaitable[T], seconds: float = DATABASE_TIMEOUT_SECONDS) -> T:
    """Await ``awaitable``, cancelling it and raising ``TimeoutError`` after ``seconds``."""
    try:
        return await asyncio.wait_for(awaitable, seconds)
    except TimeoutError as error:
        raise TimeoutError(f"The database did not answer within {seconds:g} seconds.") from error
