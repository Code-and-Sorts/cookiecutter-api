from typing import Annotated
from pydantic import Field, StrictStr
from .base import BaseCreateRequest, BaseEntity, BaseResponse, BaseUpdateRequest


class CatEntity(BaseEntity):
    name: str | None = None


class CatCreateRequest(BaseCreateRequest):
    name: Annotated[StrictStr, Field(min_length=1)]


class CatUpdateRequest(BaseUpdateRequest):
    # Defaults skip validation: an absent field stays unset, an explicit null is rejected unless nullable.
    name: Annotated[StrictStr, Field(min_length=1)] = None


class CatResponse(BaseResponse):
    name: str | None = None
