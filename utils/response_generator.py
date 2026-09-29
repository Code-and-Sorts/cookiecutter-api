import json
from azure.functions import HttpResponse
from pydantic import BaseModel

def response_generator(items: BaseModel | list, status_code: int = 200):

    if isinstance(items, list):
        if not items:
            body = json.dumps([])
        else:
            body = json.dumps([item.model_dump() for item in items])
    else:
        body = json.dumps(items.model_dump())
    return HttpResponse(body=body, status_code=status_code)
