from typing import Annotated, List, Literal
from pydantic import AfterValidator, BaseModel, ConfigDict, Field, StrictStr
from .fields import Number, SafeInt, check_unique_items


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
    # Owning tenant
    tenantId: str | None = "public"
    region: str | None = "eu"
    priority: int | None = None
    rank: int | float | None = None
    labels: List[str] | None = Field(default_factory=lambda: [])

    @classmethod
    def client_fields(cls) -> list[str]:
        return [name for name in cls.model_fields if name not in SYSTEM_FIELDS]

    @classmethod
    def client_defaults(cls) -> dict:
        """Evaluated per call, so dynamic defaults are fresh on every write."""
        return {name: cls.model_fields[name].get_default(call_default_factory=True) for name in cls.client_fields()}


class BaseCreateRequest(RequestModel):
    tenantId: Annotated[StrictStr, Field(min_length=1, max_length=64)] = "public"
    region: Literal["eu", "us"] = "eu"
    priority: Annotated[SafeInt, Field(ge=0)] | None = None
    rank: Number
    labels: Annotated[List[StrictStr], AfterValidator(check_unique_items)] = Field(default_factory=lambda: [])


class BaseReplaceRequest(RequestModel):
    tenantId: Annotated[StrictStr, Field(min_length=1, max_length=64)] = "public"
    priority: Annotated[SafeInt, Field(ge=0)] | None = None
    rank: Number
    labels: Annotated[List[StrictStr], AfterValidator(check_unique_items)] = Field(default_factory=lambda: [])


class BaseUpdateRequest(RequestModel):
    # Defaults skip validation: an absent field stays unset, an explicit null is rejected unless nullable.
    tenantId: Annotated[StrictStr, Field(min_length=1, max_length=64)] = None
    priority: Annotated[SafeInt, Field(ge=0)] | None = None
    rank: Number = None
    labels: Annotated[List[StrictStr], AfterValidator(check_unique_items)] = None


class BaseResponse(BaseModel):
    id: str
    tenantId: str | None = "public"
    region: str | None = "eu"
    priority: int | None = None
    rank: int | float | None = None
    labels: List[str] | None = Field(default_factory=lambda: [])
