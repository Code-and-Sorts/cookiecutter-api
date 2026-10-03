{%- from 'python/_model.jinja' import sample_dict -%}
import json

ITEM_ID = "ac1df01c-7ece-4a20-ab60-179829dad8f5"
SYSTEM_FIELDS = {{ sample_dict(base_fields | selectattr("system")) }}


def encode(body) -> bytes:
    return json.dumps(body).encode()


def invalid_bodies(valid: dict, required: list[str], refused: dict, rejected: dict[str, list]) -> list[bytes]:
    """Bodies an operation must reject: system, unknown and refused fields, rejected values and missing required ones."""
    bodies = [b"[]", b"{not json", encode({**valid, "not_a_field": 1})]
    bodies += [encode({**valid, name: value}) for name, value in {**SYSTEM_FIELDS, **refused}.items()]
    bodies += [encode({**valid, name: value}) for name in valid for value in rejected[name]]
    bodies += [encode({key: value for key, value in valid.items() if key != name}) for name in required]
    return bodies
