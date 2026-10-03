from pydantic import BaseModel, ConfigDict


SYSTEM_FIELDS = ("id", "isDeleted", "createdTimestamp", "updatedTimestamp", "createdBy", "updatedBy")


class RequestModel(BaseModel):
    """Request bodies reject unknown and server-managed fields and never coerce types."""

    model_config = ConfigDict(extra="forbid", strict=True)


class BaseEntity(BaseModel):
    id: str
    isDeleted: bool = False
    createdTimestamp: str
    updatedTimestamp: str
    createdBy: str | None = None
    updatedBy: str | None = None

    @classmethod
    def client_fields(cls) -> list[str]:
        return [name for name in cls.model_fields if name not in SYSTEM_FIELDS]

    @classmethod
    def client_defaults(cls) -> dict:
        """Evaluated per call, so dynamic defaults are fresh on every write."""
        return {name: cls.model_fields[name].get_default(call_default_factory=True) for name in cls.client_fields()}


class BaseCreateRequest(RequestModel):
    pass


class BaseReplaceRequest(RequestModel):
    pass


class BaseUpdateRequest(RequestModel):
    pass


class BaseResponse(BaseModel):
    id: str
