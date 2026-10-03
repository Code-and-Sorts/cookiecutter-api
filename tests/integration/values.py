"""Values for a resource's fields, read from its answers: valid bodies, invalid ones and the responses they yield.

The rules mirror shared/_fields.jinja, which derives the same samples and violations for the generated unit tests.
"""

import random
from datetime import UTC, date, datetime, timedelta
from typing import Any
from unittest.mock import ANY
from uuid import uuid4

from project import Field, Resource

MAX_SAFE_INTEGER = 9007199254740991
EPOCH = datetime(2026, 1, 1, tzinfo=UTC)


def timestamp(value: datetime) -> str:
    return value.astimezone(UTC).isoformat(timespec="milliseconds").replace("+00:00", "Z")


def _bounds(field: Field) -> tuple[float | None, float | None]:
    """The inclusive range a number may take."""
    rules, step = field.rule, 1 if field.type == "integer" else 0.5
    low = rules.get("minimum", rules["exclusive_minimum"] + step if "exclusive_minimum" in rules else None)
    high = rules.get("maximum", rules["exclusive_maximum"] - step if "exclusive_maximum" in rules else None)
    return low, high


def _item(item_type: str, index: int) -> Any:
    return {
        "string": f"item{index + 1}",
        "integer": index + 1,
        "number": index + 1.5,
        "boolean": index % 2 == 0,
        "date": f"2026-01-{index % 28 + 1:02d}",
        "date-time": f"2026-01-15T10:{index % 60:02d}:00.000Z",
        "uuid": f"6f1c2a3b-4d5e-4f60-8a7b-{index:012x}",
    }[item_type]


def _string(field: Field, text: str) -> str:
    rules = field.rule
    if "max_length" in rules:
        text = text[: rules["max_length"]]
    return text.ljust(rules.get("min_length", 0), "s")


def sample(field: Field) -> Any:
    """A fixed valid value: the example, the static default or one derived from the rules."""
    if field.example is not None:
        return field.example
    if field.has_default and field.default is not None:
        return field.default
    rules = field.rule
    if field.type == "array":
        count = max(rules.get("min_items", 1), 1)
        count = min(count, rules.get("max_items", count))
        return [_item(field.item_type, index) for index in range(count)]
    if field.type == "string":
        return {"email": "user@example.com", "uri": "https://example.com/items/1"}.get(
            rules.get("format"), _string(field, "sample")
        )
    if field.type in ("integer", "number"):
        value = 1 if field.type == "integer" else 1.5
        low, high = _bounds(field)
        if low is not None and value < low:
            value = low
        if high is not None and value > high:
            value = high if low is None else ((low + high) // 2 if field.type == "integer" else (low + high) / 2)
        return value
    if field.type == "enum":
        return field.enum_values[0]
    return _item(field.type, 0)


def fresh(field: Field) -> Any:
    """A valid value that differs from call to call where the rules allow, so a write is visible."""
    rules = field.rule
    tag = uuid4().hex[:8]
    if field.type == "string":
        if "pattern" in rules:
            return sample(field)
        if rules.get("format") == "email":
            return f"user-{tag}@example.com"
        if rules.get("format") == "uri":
            return f"https://example.com/items/{tag}"
        return _string(field, f"{field.name}-{tag}")
    if field.type in ("integer", "number"):
        low, high = _bounds(field)
        low = -1000 if low is None else low
        high = low + 1000 if high is None else high
        if field.type == "integer":
            return random.randint(int(low), int(high))
        return round(random.uniform(low, high), 3) if low < high else low
    if field.type == "boolean":
        return random.choice([True, False])
    if field.type == "enum":
        return random.choice(field.enum_values)
    if field.type == "date":
        return (date(2026, 1, 1) + timedelta(days=random.randint(0, 3000))).isoformat()
    if field.type == "date-time":
        return timestamp(EPOCH + timedelta(milliseconds=random.randint(0, 10**11)))
    if field.type == "uuid":
        return str(uuid4())
    count = len(sample(field))
    if field.item_type == "boolean":
        return sample(field)
    offset = random.randint(0, 1000)
    return [_item(field.item_type, offset + index) if field.item_type != "string" else f"{tag}-{index}" for index in range(count)]


def violations(field: Field) -> list[Any]:
    """A wrongly typed value, null unless nullable, and values just outside each rule; every one is a 400."""
    rules = field.rule
    wrong = {"string": 42, "integer": "1", "number": "1", "boolean": "true", "enum": "__invalid__", "array": "not-a-list"}
    values: list[Any] = [wrong.get(field.type, f"not-a-{field.type}")]
    if not field.nullable:
        values.append(None)
    if field.type == "string":
        if rules.get("min_length"):
            values.append("s" * (rules["min_length"] - 1))
        if "max_length" in rules:
            values.append("s" * (rules["max_length"] + 1))
        if "pattern" in rules:
            values.append("!")
        values += {"email": ["not-an-email"], "uri": ["not a uri"]}.get(rules.get("format"), [])
    elif field.type in ("integer", "number"):
        for rule, shift in (("minimum", -1), ("exclusive_minimum", 0), ("maximum", 1), ("exclusive_maximum", 0)):
            if rule in rules:
                values.append(rules[rule] + shift)
        if field.type == "integer":
            values += [1.5, MAX_SAFE_INTEGER + 1]
    elif field.type == "array":
        item = _item(field.item_type, 0)
        values.append([item, item] if rules.get("unique_items") else [None])
        if rules.get("min_items"):
            values.append([item] * (rules["min_items"] - 1))
        if "max_items" in rules:
            values.append([_item(field.item_type, index) for index in range(rules["max_items"] + 1)])
    elif field.type == "date":
        values.append("2026-02-30")
    elif field.type == "date-time":
        values.append("2026-01-31T09:30:00")
    elif field.type == "enum" and field.enum_values[0].upper() not in field.enum_values:
        values.append(field.enum_values[0].upper())
    # A pattern can match "!"; such a value is no violation.
    return [value for value in values if not (value == "!" and _matches(field, value))]


def _matches(field: Field, value: str) -> bool:
    import re

    return re.search(field.rule["pattern"], value) is not None


def valid_body(resource: Resource, operation: str, *, fresh_values: bool = True) -> dict[str, Any]:
    """Every field the operation accepts, so its response is fully predictable."""
    make = fresh if fresh_values else sample
    return {f.name: make(f) for f in resource.accepted(operation)}


def minimal_body(resource: Resource, operation: str) -> dict[str, Any]:
    """Only the fields the operation needs: required ones without a default."""
    return {f.name: fresh(f) for f in resource.accepted(operation) if operation in f.needed_on}


def normalized(field: Field, value: Any) -> Any:
    """The value as stored and returned: date-times in UTC with milliseconds."""
    if value is None:
        return None
    if field.type == "date-time":
        return timestamp(datetime.fromisoformat(value))
    if field.type == "array" and field.item_type == "date-time":
        return [timestamp(datetime.fromisoformat(item)) for item in value]
    return value


def _default(field: Field) -> Any:
    if field.dynamic:
        return ANY
    return field.default if field.has_default else None


def written(resource: Resource, operation: str, body: dict[str, Any], before: dict[str, Any] | None) -> dict[str, Any]:
    """Every client field's value after the write, None for no value; ANY marks a dynamic default."""
    values = {}
    for f in resource.fields:
        if f.name in body:
            values[f.name] = normalized(f, body[f.name])
        elif operation == "update" or (operation == "replace" and not f.accepted("replace")):
            values[f.name] = (before or {}).get(f.name)
        else:
            values[f.name] = _default(f)
    return values


def response(resource: Resource, item_id: str, values: dict[str, Any]) -> dict[str, Any]:
    """The response for a record holding values: id and every shown field, read defaults filling gaps."""
    shown = {f.name: values.get(f.name) if values.get(f.name) is not None else f.read_default for f in resource.shown}
    return {"id": item_id, **shown}


def response_from_record(resource: Resource, record: dict[str, Any]) -> dict[str, Any]:
    return response(resource, record["id"], record)


def full_record(resource: Resource) -> dict[str, Any]:
    """Every client field with a fresh value, for seeding a container no resource can create in."""
    return {f.name: fresh(f) for f in resource.fields}
