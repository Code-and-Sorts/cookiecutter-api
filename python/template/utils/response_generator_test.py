import json
from models import {{ resources[0].name }}Response
from .response_generator import response_generator

_ID = "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"
_item = {{ resources[0].name }}Response(id=_ID, name="mockName")


def _parts(response):
    """(status, content type, body) of this cloud's response."""
{%- if cloud_service == 'Azure Function App' %}
    return response.status_code, response.mimetype, response.get_body().decode()
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    body, status, headers = response
    return status, headers["Content-Type"], body
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    return response["statusCode"], response["headers"]["Content-Type"], response["body"]
{%- endif %}


def describe_response_generator():
    def test_item_is_exactly_id_and_name():
        status, content_type, body = _parts(response_generator(_item))

        assert status == 200
        assert content_type == "application/json"
        assert json.loads(body) == {"id": _ID, "name": "mockName"}

    def test_list_of_items():
        status, _, body = _parts(response_generator([_item, _item]))

        assert status == 200
        assert json.loads(body) == [{"id": _ID, "name": "mockName"}] * 2

    def test_empty_list():
        status, content_type, body = _parts(response_generator([]))

        assert (status, content_type, body) == (200, "application/json", "[]")

    def test_plain_payload_and_status():
        status, content_type, body = _parts(response_generator({"errorMessage": "Nope."}, 404))

        assert (status, content_type) == (404, "application/json")
        assert json.loads(body) == {"errorMessage": "Nope."}
