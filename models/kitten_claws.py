from pydantic import BaseModel, ConfigDict, Field
from .base import BaseResponse


class BaseKittenClaws(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)

    name: str = Field(min_length=1)


class KittenClawsUpdate(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)

    # Defaults skip validation: an absent name stays unset, an explicit null is rejected.
    name: str = Field(default=None, min_length=1)


class KittenClawsResponse(BaseResponse):
    name: str
