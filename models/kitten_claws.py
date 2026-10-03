from typing import Annotated
from pydantic import Field, StrictStr
from .base import BaseCreateRequest, BaseEntity, BaseResponse, BaseUpdateRequest


class KittenClawsEntity(BaseEntity):
    name: str | None = None


class KittenClawsCreateRequest(BaseCreateRequest):
    name: Annotated[StrictStr, Field(min_length=1)]


class KittenClawsUpdateRequest(BaseUpdateRequest):
    # Defaults skip validation: an absent field stays unset, an explicit null is rejected unless nullable.
    name: Annotated[StrictStr, Field(min_length=1)] = None


class KittenClawsResponse(BaseResponse):
    name: str | None = None
