from pydantic import BaseModel, ConfigDict, Field


class BaseCat(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)

    name: str = Field(min_length=1)


class CatUpdate(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)

    # Defaults skip validation: an absent name stays unset, an explicit null is rejected.
    name: str = Field(default=None, min_length=1)


class CatResponse(BaseModel):
    id: str
    name: str
