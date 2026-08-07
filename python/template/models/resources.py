import uuid
from datetime import datetime, timezone
from pydantic import BaseModel, Field
from typing import Optional
from uuid import UUID

def generate_utc_timestamp():
    return str(datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"))
{% for resource in resources %}
class Base{{ resource.name }}(BaseModel):
    name: str
    type: Optional[str] = None

class {{ resource.name }}IdValidation(BaseModel):
    id: UUID

class {{ resource.name }}(Base{{ resource.name }}):
    id: str = Field(default_factory=lambda: str(uuid.uuid4()))
    isDeleted: bool = Field(default=False)
    createdDate: str = Field(default_factory=lambda: generate_utc_timestamp())
    updatedDate: str = generate_utc_timestamp()

class {{ resource.name }}Response(Base{{ resource.name }}):
    id: str
{% endfor %}