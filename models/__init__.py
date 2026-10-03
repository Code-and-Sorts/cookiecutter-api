from .base import BaseCreateRequest, BaseEntity, BaseReplaceRequest, BaseResponse, BaseUpdateRequest
from .fields import generate_utc_timestamp
from .cat import CatEntity, CatCreateRequest, CatUpdateRequest, CatResponse
from .dog import DogEntity, DogCreateRequest, DogReplaceRequest, DogResponse

__all__ = [
    "BaseCreateRequest",
    "BaseEntity",
    "BaseReplaceRequest",
    "BaseResponse",
    "BaseUpdateRequest",
    "generate_utc_timestamp",
    "CatEntity",
    "CatCreateRequest",
    "CatUpdateRequest",
    "CatResponse",
    "DogEntity",
    "DogCreateRequest",
    "DogReplaceRequest",
    "DogResponse",
]
