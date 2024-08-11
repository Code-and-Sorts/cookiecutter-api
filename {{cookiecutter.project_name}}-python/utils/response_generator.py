import json
from azure.functions import HttpResponse
from models import ItemResponse

def response_generator(items: ItemResponse | list, status_code: int = 200) -> HttpResponse:
    if isinstance(items, list):
        if not items:
            return HttpResponse(
                body=json.dumps([]),
                status_code=status_code
            )
        return HttpResponse(
            body=json.dumps([item.model_dump() for item in items]),
            status_code=status_code
        )
    return HttpResponse(
        body=json.dumps(items.model_dump()),
        status_code=status_code
    )
