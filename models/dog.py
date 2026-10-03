from typing import Annotated
from pydantic import Field, StrictStr
from .base import BaseCreateRequest, BaseEntity, BaseReplaceRequest, BaseResponse


class DogEntity(BaseEntity):
    name: str | None = None


class DogCreateRequest(BaseCreateRequest):
    name: Annotated[StrictStr, Field(min_length=1)]


class DogReplaceRequest(BaseReplaceRequest):
    name: Annotated[StrictStr, Field(min_length=1)]


class DogResponse(BaseResponse):
    name: str | None = None
