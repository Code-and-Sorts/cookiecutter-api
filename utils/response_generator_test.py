import json
from pydantic import BaseModel
from .response_generator import response_generator

_ID = "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"


class _ItemResponse(BaseModel):
    id: str
    label: str | None = None


_item = _ItemResponse(id=_ID, label="mockLabel")


def _parts(response):
    return response.status_code, response.mimetype, response.get_body().decode()


def describe_response_generator():
    def test_item_is_its_response_fields():
        status, content_type, body = _parts(response_generator(_item))

        assert status == 200
        assert content_type == "application/json"
        assert json.loads(body) == {"id": _ID, "label": "mockLabel"}

    def test_list_of_items():
        status, _, body = _parts(response_generator([_item, _item]))

        assert status == 200
        assert json.loads(body) == [{"id": _ID, "label": "mockLabel"}] * 2

    def test_empty_list():
        status, content_type, body = _parts(response_generator([]))

        assert (status, content_type, body) == (200, "application/json", "[]")

    def test_plain_payload_and_status():
        status, content_type, body = _parts(response_generator({"errorMessage": "Nope."}, 404))

        assert (status, content_type) == (404, "application/json")
        assert json.loads(body) == {"errorMessage": "Nope."}
