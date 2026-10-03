import pytest

from api import BODY_OPERATIONS, ITEM_OPERATIONS
from values import sample, valid_body

WRITE_OPERATIONS = ("create", "update", "replace", "delete")

MALFORMED_BODIES = {
    "malformed": b'{"a": ',
    "empty": b"",
    "array": b"[]",
    "string": b'"text"',
    "null": b"null",
}


def assert_error(response, status: int):
    assert response.status_code == status, response.text
    assert isinstance(response.json()["errorMessage"], str)


def target_id(operation: str, make_record, resource) -> str | None:
    """An existing record for item operations, so a 404 cannot hide the status under test."""
    return make_record(resource)["id"] if operation != "create" else None


@pytest.mark.each_operation(*BODY_OPERATIONS)
@pytest.mark.parametrize("body", MALFORMED_BODIES.values(), ids=MALFORMED_BODIES.keys())
def test_a_body_that_is_not_a_json_object_is_rejected(api, resource, operation, body, make_record):
    item_id = target_id(operation, make_record, resource)
    response = api.send(operation, resource, item_id, content=body, headers={"Content-Type": "application/json"})
    assert_error(response, 400)


@pytest.mark.each_operation(*BODY_OPERATIONS)
def test_an_unknown_field_is_rejected(api, resource, operation, make_record):
    body = {**valid_body(resource, operation), "not_a_field": 1}
    assert_error(api.send(operation, resource, target_id(operation, make_record, resource), json=body), 400)


@pytest.mark.each_operation(*BODY_OPERATIONS)
def test_a_server_managed_field_is_rejected(api, project, resource, operation, make_record):
    item_id = target_id(operation, make_record, resource)
    for field in project.system_fields:
        body = {**valid_body(resource, operation), field.name: sample(field)}
        assert_error(api.send(operation, resource, item_id, json=body), 400)


@pytest.mark.fields("invalid")
def test_an_invalid_value_is_rejected(api, resource, operation, field, value, make_record):
    body = {**valid_body(resource, operation), field.name: value}
    assert_error(api.send(operation, resource, target_id(operation, make_record, resource), json=body), 400)


@pytest.mark.fields("required")
def test_a_body_without_a_required_field_is_rejected(api, resource, operation, field, make_record):
    body = {key: value for key, value in valid_body(resource, operation).items() if key != field.name}
    assert_error(api.send(operation, resource, target_id(operation, make_record, resource), json=body), 400)


@pytest.mark.fields("refused")
def test_a_field_the_operation_does_not_accept_is_rejected(api, resource, operation, field, make_record):
    body = {**valid_body(resource, operation), field.name: sample(field)}
    assert_error(api.send(operation, resource, target_id(operation, make_record, resource), json=body), 400)


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
