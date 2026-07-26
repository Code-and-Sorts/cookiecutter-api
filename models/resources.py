import uuid
from datetime import datetime, timezone
from pydantic import BaseModel, Field
from typing import Optional
from uuid import UUID

def generate_utc_timestamp():
    return str(datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"))

class BaseCat(BaseModel):
    name: str
    type: Optional[str] = None

class CatIdValidation(BaseModel):
    id: UUID

class Cat(BaseCat):
    id: str = Field(default_factory=lambda: str(uuid.uuid4()))
    isDeleted: bool = Field(default=False)
    createdDate: str = Field(default_factory=lambda: generate_utc_timestamp())
    updatedDate: str = generate_utc_timestamp()

class CatResponse(BaseCat):
    id: str

class BaseDog(BaseModel):
    name: str
    type: Optional[str] = None

class DogIdValidation(BaseModel):
    id: UUID

class Dog(BaseDog):
    id: str = Field(default_factory=lambda: str(uuid.uuid4()))
    isDeleted: bool = Field(default=False)
    createdDate: str = Field(default_factory=lambda: generate_utc_timestamp())
    updatedDate: str = generate_utc_timestamp()

class DogResponse(BaseDog):
    id: str
