import json

from pydantic import BaseModel

def response_generator(items: BaseModel | list, status_code: int = 200):
    headers = {'Content-Type': 'application/json'}

    if isinstance(items, list):
        if not items:
            body = json.dumps([])
        else:
            body = json.dumps([item.model_dump() for item in items])
    else:
        body = json.dumps(items.model_dump())
    return (body, status_code, headers)
