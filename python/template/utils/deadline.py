import asyncio
from collections.abc import Awaitable

# Bounds the whole request, however many calls it makes, so a failing database
# ends in a 500 well inside the platform's own timeout.
DATABASE_TIMEOUT_SECONDS = 8.0


async def within_deadline[T](awaitable: Awaitable[T], seconds: float = DATABASE_TIMEOUT_SECONDS) -> T:
    try:
        return await asyncio.wait_for(awaitable, seconds)
    except TimeoutError as error:
        raise TimeoutError(f"The database did not answer within {seconds:g} seconds.") from error
