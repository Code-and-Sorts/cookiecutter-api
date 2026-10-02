import pytest

from api import BODY_OPERATIONS, ITEM_OPERATIONS

WRITE_OPERATIONS = ("create", "update", "replace", "delete")

INVALID_BODIES = {
    "malformed": b'{"name": ',
    "empty": b"",
    "array": b"[]",
    "string": b'"name"',
    "null": b"null",
    "empty-name": b'{"name": ""}',
    "number-name": b'{"name": 42}',
    "null-name": b'{"name": null}',
    "unknown-field": b'{"name": "Tom", "color": "grey"}',
    "id": b'{"name": "Tom", "id": "00000000-0000-4000-8000-000000000000"}',
    "isDeleted": b'{"name": "Tom", "isDeleted": true}',
    "createdTimestamp": b'{"name": "Tom", "createdTimestamp": "2020-01-01T00:00:00.000Z"}',
    "updatedTimestamp": b'{"name": "Tom", "updatedTimestamp": "2020-01-01T00:00:00.000Z"}',
    "createdBy": b'{"name": "Tom", "createdBy": "mallory"}',
    "updatedBy": b'{"name": "Tom", "updatedBy": "mallory"}',
}


def assert_error(response, status: int):
    assert response.status_code == status, response.text
    assert isinstance(response.json()["errorMessage"], str)


def target_id(operation: str, make_record, resource) -> str | None:
    """An existing record for item operations, so a 404 cannot hide the status under test."""
    return make_record(resource)["id"] if operation != "create" else None


@pytest.mark.each_operation(*BODY_OPERATIONS)
@pytest.mark.parametrize("body", INVALID_BODIES.values(), ids=INVALID_BODIES.keys())
def test_an_invalid_body_is_rejected(api, resource, operation, body, make_record):
    item_id = target_id(operation, make_record, resource)
    response = api.send(operation, resource, item_id, content=body, headers={"Content-Type": "application/json"})
    assert_error(response, 400)


@pytest.mark.each_operation("create", "replace")
def test_a_body_without_a_name_is_rejected(api, resource, operation, make_record):
    assert_error(api.send(operation, resource, target_id(operation, make_record, resource), json={}), 400)


@pytest.mark.ops("update")
def test_an_update_without_a_name_keeps_the_record(api, resource, make_record):
    record = make_record(resource)
    response = api.send("update", resource, record["id"], json={})
    assert response.status_code == 200, response.text
    assert response.json() == record


@pytest.mark.each_operation(*ITEM_OPERATIONS)
@pytest.mark.parametrize("item_id", ["not-a-uuid", "12345"])
def test_a_non_uuid_id_is_not_found(api, resource, operation, item_id):
    assert_error(api.send(operation, resource, item_id), 404)


@pytest.mark.each_operation(*ITEM_OPERATIONS)
def test_an_unknown_id_is_not_found(api, resource, operation):
    assert_error(api.send(operation, resource), 404)


@pytest.mark.each_operation(*WRITE_OPERATIONS)
def test_an_overlong_user_id_is_rejected(api, resource, operation, make_record):
    item_id = target_id(operation, make_record, resource)
    assert_error(api.send(operation, resource, item_id, user_id="u" * 257), 400)


@pytest.mark.ops("create")
def test_a_user_id_of_256_characters_is_accepted(api, resource):
    response = api.send("create", resource, user_id="u" * 256)
    assert response.status_code == 201, response.text
