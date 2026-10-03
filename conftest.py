import json

ITEM_ID = "ac1df01c-7ece-4a20-ab60-179829dad8f5"
SYSTEM_FIELDS = {"id": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "isDeleted": False, "createdTimestamp": "2026-01-15T10:00:00.000Z", "updatedTimestamp": "2026-01-15T10:00:00.000Z", "createdBy": "sample", "updatedBy": "sample"}


def encode(body) -> bytes:
    return json.dumps(body).encode()


def invalid_bodies(valid: dict, required: list[str], refused: dict, rejected: dict[str, list]) -> list[bytes]:
    """Bodies an operation must reject: system, unknown and refused fields, rejected values and missing required ones."""
    bodies = [b"[]", b"{not json", encode({**valid, "not_a_field": 1})]
    bodies += [encode({**valid, name: value}) for name, value in {**SYSTEM_FIELDS, **refused}.items()]
    bodies += [encode({**valid, name: value}) for name in valid for value in rejected[name]]
    bodies += [encode({key: value for key, value in valid.items() if key != name}) for name in required]
    return bodies
