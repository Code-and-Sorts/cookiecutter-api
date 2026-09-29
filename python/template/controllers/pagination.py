from typing import Optional
from repositories import DEFAULT_LIST_LIMIT

MAX_LIST_LIMIT = 1000


def coerce_limit(raw: Optional[str]) -> int:
    """Parse the ``limit`` query parameter for list endpoints.

    Missing, non-numeric or non-positive values fall back to the default,
    and larger values are capped at ``MAX_LIST_LIMIT``.
    """
    try:
        limit = int(raw)
    except (TypeError, ValueError):
        return DEFAULT_LIST_LIMIT
    if limit < 1:
        return DEFAULT_LIST_LIMIT
    return min(limit, MAX_LIST_LIMIT)
