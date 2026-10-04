from datetime import datetime, timezone
from pydantic import BaseModel, Field, model_serializer


def generate_utc_timestamp() -> str:
    now = datetime.now(timezone.utc)
    return now.isoformat(timespec="milliseconds").replace("+00:00", "Z")


_AUDIT_FIELDS = ("createdTimestamp", "createdBy", "updatedTimestamp", "updatedBy")


def _unset(value) -> bool:
    return value is None


class BaseResponse(BaseModel):
    """The id and audit fields every response carries; isDeleted and database metadata are dropped."""

    id: str
    createdTimestamp: str
    createdBy: str | None = Field(default=None, exclude_if=_unset)
    updatedTimestamp: str
    updatedBy: str | None = Field(default=None, exclude_if=_unset)

    @model_serializer(mode="wrap")
    def _resource_fields_before_audit_fields(self, handler):
        data = handler(self)
        audit = {key: data.pop(key) for key in _AUDIT_FIELDS if key in data}
        return {**data, **audit}
