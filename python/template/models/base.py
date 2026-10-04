from datetime import datetime, timezone
from pydantic import BaseModel, Field


def generate_utc_timestamp() -> str:
    now = datetime.now(timezone.utc)
    return now.isoformat(timespec="milliseconds").replace("+00:00", "Z")


def _unset(value) -> bool:
    return value is None


class BaseResponse(BaseModel):
    """A stored record as clients see it; isDeleted and database metadata are dropped."""

    id: str
    name: str
    createdTimestamp: str
    createdBy: str | None = Field(default=None, exclude_if=_unset)
    updatedTimestamp: str
    updatedBy: str | None = Field(default=None, exclude_if=_unset)
