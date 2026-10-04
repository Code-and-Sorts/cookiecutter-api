import asyncio
from collections.abc import Coroutine
import aioboto3
from utils.deadline import within_deadline

# Sessions are not tied to an event loop, so one serves every invocation.
session = aioboto3.Session()


def run[T](operation: Coroutine[object, object, T]) -> T:
    return asyncio.run(within_deadline(operation))
