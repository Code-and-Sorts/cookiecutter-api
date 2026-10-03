import json
from pydantic import BaseModel

JSON_CONTENT_TYPE = "application/json"


def _plain(value):
    return value.model_dump() if isinstance(value, BaseModel) else value


def to_json(payload) -> str:
    if isinstance(payload, list):
        return json.dumps([_plain(item) for item in payload])
    return json.dumps(_plain(payload))


def response_generator(payload, status_code: int = 200):
    body = to_json(payload)
    return {
        "statusCode": status_code,
        "headers": {"Content-Type": JSON_CONTENT_TYPE},
        "body": body
    }
