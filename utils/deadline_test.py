import asyncio
import pytest
from .deadline import DATABASE_TIMEOUT_SECONDS, within_deadline


def describe_within_deadline():
    def test_default_leaves_room_under_ten_seconds():
        assert 0 < DATABASE_TIMEOUT_SECONDS < 10

    def test_returns_result():
        async def quick():
            return "ok"

        assert asyncio.run(within_deadline(quick())) == "ok"

    def test_cancels_slow_work_and_raises_timeout():
        cancelled = []

        async def slow():
            try:
                await asyncio.sleep(60)
            except asyncio.CancelledError:
                cancelled.append(True)
                raise

        with pytest.raises(TimeoutError, match="did not answer within 0.05 seconds"):
            asyncio.run(within_deadline(slow(), 0.05))
        assert cancelled == [True]

    def test_propagates_errors():
        async def failing():
            raise RuntimeError("boom")

        with pytest.raises(RuntimeError, match="boom"):
            asyncio.run(within_deadline(failing()))
