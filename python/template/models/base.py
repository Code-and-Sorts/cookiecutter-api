from datetime import datetime, timezone


def generate_utc_timestamp() -> str:
    """The current UTC time as ISO-8601 with millisecond precision and a ``Z``
    suffix, for example ``2026-09-29T22:49:26.625Z``."""
    now = datetime.now(timezone.utc)
    return now.isoformat(timespec="milliseconds").replace("+00:00", "Z")
