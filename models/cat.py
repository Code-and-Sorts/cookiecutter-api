from typing import Annotated, List, Literal
from pydantic import AfterValidator, Field, StrictBool, StrictStr
from .fields import DateStr, DateTimeStr, Number, SafeInt, UuidStr, check_email, check_unique_items, check_uri, generate_utc_timestamp, new_uuid, utc_today
from .base import BaseCreateRequest, BaseEntity, BaseReplaceRequest, BaseResponse, BaseUpdateRequest


class CatEntity(BaseEntity):
    name: str | None = None
    breed: str | None = "tabby"
    ageYears: int | None = 0
    weightKg: int | float | None = None
    indoor: bool | None = True
    birthDate: str | None = Field(default_factory=utc_today)
    microchipId: str | None = Field(default_factory=new_uuid)
    ownerEmail: str | None = "unknown@example.com"
    website: str | None = None
    tagCode: str | None = None
    tags: List[str] | None = Field(default_factory=lambda: [])
    scores: List[int] | None = None
    adoptedAt: str | None = Field(default_factory=generate_utc_timestamp)
    lastVisit: str | None = "2026-01-01T00:00:00.000Z"
    notes: str | None = "$none"


class CatCreateRequest(BaseCreateRequest):
    name: Annotated[StrictStr, Field(min_length=1, max_length=100)]
    breed: Literal["siamese", "persian", "tabby"] = "tabby"
    ageYears: Annotated[SafeInt, Field(ge=0, le=40)] = 0
    weightKg: Annotated[Number, Field(gt=0, lt=100)] | None = None
    indoor: StrictBool = True
    birthDate: DateStr = Field(default_factory=utc_today)
    microchipId: UuidStr = Field(default_factory=new_uuid)
    ownerEmail: Annotated[StrictStr, AfterValidator(check_email)] = "unknown@example.com"
    website: Annotated[StrictStr, AfterValidator(check_uri)] | None = None
    tagCode: Annotated[StrictStr, Field(pattern="^[A-Z]{3}-[0-9]{3}$")] = None
    tags: Annotated[List[StrictStr], Field(max_length=3), AfterValidator(check_unique_items)] = Field(default_factory=lambda: [])
    scores: Annotated[List[SafeInt], Field(min_length=1)] | None = None
    adoptedAt: DateTimeStr | None = Field(default_factory=generate_utc_timestamp)
    lastVisit: DateTimeStr = "2026-01-01T00:00:00.000Z"
    notes: StrictStr = "$none"


class CatReplaceRequest(BaseReplaceRequest):
    name: Annotated[StrictStr, Field(min_length=1, max_length=100)]
    breed: Literal["siamese", "persian", "tabby"] = "tabby"
    ageYears: Annotated[SafeInt, Field(ge=0, le=40)] = 0
    weightKg: Annotated[Number, Field(gt=0, lt=100)] | None = None
    indoor: StrictBool = True
    birthDate: DateStr = Field(default_factory=utc_today)
    ownerEmail: Annotated[StrictStr, AfterValidator(check_email)] = "unknown@example.com"
    website: Annotated[StrictStr, AfterValidator(check_uri)] | None = None
    tagCode: Annotated[StrictStr, Field(pattern="^[A-Z]{3}-[0-9]{3}$")] = None
    tags: Annotated[List[StrictStr], Field(max_length=3), AfterValidator(check_unique_items)] = Field(default_factory=lambda: [])
    scores: Annotated[List[SafeInt], Field(min_length=1)] | None = None
    adoptedAt: DateTimeStr | None = Field(default_factory=generate_utc_timestamp)
    lastVisit: DateTimeStr = "2026-01-01T00:00:00.000Z"
    notes: StrictStr = "$none"


class CatUpdateRequest(BaseUpdateRequest):
    # Defaults skip validation: an absent field stays unset, an explicit null is rejected unless nullable.
    name: Annotated[StrictStr, Field(min_length=1, max_length=100)] = None
    ageYears: Annotated[SafeInt, Field(ge=0, le=40)] = None
    weightKg: Annotated[Number, Field(gt=0, lt=100)] | None = None
    indoor: StrictBool = None
    ownerEmail: Annotated[StrictStr, AfterValidator(check_email)] = None
    website: Annotated[StrictStr, AfterValidator(check_uri)] | None = None
    tags: Annotated[List[StrictStr], Field(max_length=3), AfterValidator(check_unique_items)] = None
    adoptedAt: DateTimeStr | None = None
    notes: StrictStr = None


class CatResponse(BaseResponse):
    name: str | None = None
    breed: str | None = "tabby"
    ageYears: int | None = 0
    weightKg: int | float | None = None
    indoor: bool | None = True
    birthDate: str | None = None
    microchipId: str | None = None
    ownerEmail: str | None = "unknown@example.com"
    website: str | None = None
    tagCode: str | None = None
    tags: List[str] | None = Field(default_factory=lambda: [])
    scores: List[int] | None = None
    adoptedAt: str | None = None
    lastVisit: str | None = "2026-01-01T00:00:00.000Z"
