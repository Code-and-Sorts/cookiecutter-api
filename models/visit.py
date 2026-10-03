from typing import Annotated, List
from pydantic import Field, StrictBool, StrictStr
from .fields import DateStr, DateTimeStr, Number
from .base import BaseCreateRequest, BaseEntity, BaseResponse, BaseUpdateRequest


class VisitEntity(BaseEntity):
    reason: str | None = None
    visitedOn: str | None = None
    cost: int | float | None = None
    paid: bool | None = False
    checkedAt: List[str] | None = None


class VisitCreateRequest(BaseCreateRequest):
    reason: StrictStr
    visitedOn: DateStr
    cost: Annotated[Number, Field(ge=0)] = None


class VisitUpdateRequest(BaseUpdateRequest):
    # Defaults skip validation: an absent field stays unset, an explicit null is rejected unless nullable.
    visitedOn: DateStr = None
    cost: Annotated[Number, Field(ge=0)] = None
    paid: StrictBool = None
    checkedAt: List[DateTimeStr] = None


class VisitResponse(BaseResponse):
    reason: str | None = None
    visitedOn: str | None = None
    cost: int | float | None = None
    paid: bool | None = False
    checkedAt: List[str] | None = None
