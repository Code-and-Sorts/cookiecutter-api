"""Values for a resource's fields: valid bodies, the records they store and the responses they yield.

Samples, invalid values and edge values come from shared/_fields.jinja through project.Field. Only values that change
from call to call, so a write is visible, are made here, and the macros check each one against the field's rules.
"""

import random
import string
from datetime import UTC, date, datetime, timedelta
from typing import Any
from unittest.mock import ANY
from uuid import uuid4

from project import Field, Resource

EPOCH = datetime(2026, 1, 1, tzinfo=UTC)
FRESH_ATTEMPTS = 5


def timestamp(value: datetime) -> str:
    return value.astimezone(UTC).isoformat(timespec="milliseconds").replace("+00:00", "Z")


def _random(kind: str, near: Any, enum_values: tuple[str, ...]) -> Any:
    """A random value of the kind, near the sample where that means anything."""
    letters = "".join(random.choices(string.ascii_lowercase, k=6))
    return {
        "string": lambda: random.choice([letters + near, near + letters, letters]),
        "integer": lambda: near + random.randint(-1000, 1000),
        "number": lambda: round(near + random.uniform(-1000, 1000), 3),
        "boolean": lambda: random.choice([True, False]),
        "enum": lambda: random.choice(enum_values),
        "date": lambda: (date(2026, 1, 1) + timedelta(days=random.randint(0, 3000))).isoformat(),
        "date-time": lambda: timestamp(EPOCH + timedelta(milliseconds=random.randint(0, 10**11))),
        "uuid": lambda: str(uuid4()),
    }[kind]()


def fresh(field: Field) -> Any:
    """A valid value that differs from call to call where the rules leave room, else the sample or an edge value."""
    for _ in range(FRESH_ATTEMPTS):
        if field.type == "array":
            value = [_random(field.item_type, item, ()) for item in field.sample]
        else:
            value = _random(field.type, field.sample, field.enum_values)
        if field.is_valid(value):
            return value
    return random.choice([value for value in (field.sample, *field.boundaries) if field.is_valid(value)])


def valid_body(resource: Resource, operation: str) -> dict[str, Any]:
    """Every field the operation accepts, so its response is fully predictable."""
    return {f.name: fresh(f) for f in resource.accepted(operation)}


def minimal_body(resource: Resource, operation: str) -> dict[str, Any]:
    """Only the fields the operation needs: required ones without a default."""
    return {f.name: fresh(f) for f in resource.accepted(operation) if operation in f.needed_on}


def written(resource: Resource, operation: str, body: dict[str, Any], before: dict[str, Any] | None) -> dict[str, Any]:
    """Every client field's value after the write, None for no value; ANY marks a dynamic default."""
    values = {}
    for f in resource.fields:
        if f.name in body:
            values[f.name] = body[f.name]
        elif operation == "update" or (operation == "replace" and not f.accepted("replace")):
            values[f.name] = (before or {}).get(f.name)
        else:
            values[f.name] = ANY if f.dynamic else f.default
    return values


def assert_stored(resource: Resource, saved: dict[str, Any], values: dict[str, Any]) -> None:
    """Each client field holds its value; a field without one is not stored at all."""
    for field in resource.fields:
        if values[field.name] is None:
            assert field.name not in saved, f"{field.name} has no value, so it must not be stored"
        else:
            assert saved.get(field.name) == values[field.name], field.name


def response(resource: Resource, item_id: str, values: dict[str, Any]) -> dict[str, Any]:
    """The response for a record holding values: id and every shown field, read defaults filling gaps."""
    shown = {f.name: values.get(f.name) if values.get(f.name) is not None else f.read_default for f in resource.shown}
    return {"id": item_id, **shown}


def response_from_record(resource: Resource, record: dict[str, Any]) -> dict[str, Any]:
    return response(resource, record["id"], record)


def full_record(resource: Resource) -> dict[str, Any]:
    """Every client field with a fresh value, for seeding a container no resource can create in."""
    return {f.name: fresh(f) for f in resource.fields}
