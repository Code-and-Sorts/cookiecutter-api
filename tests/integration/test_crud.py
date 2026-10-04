from uuid import UUID

import pytest

from api import LIST_MAX, RESPONSE_FIELDS, response_of, unique_name


@pytest.mark.ops("create")
def test_create_returns_the_new_record(api, resource, store):
    name = unique_name(resource)
    response = api.send("create", resource, json={"name": name})
    assert response.status_code == 201
    body = response.json()
    UUID(body["id"])
    assert body.keys() == RESPONSE_FIELDS
    assert body["name"] == name
    assert body["createdTimestamp"] == body["updatedTimestamp"]
    assert body == response_of(store.get(resource.container, body["id"]))


@pytest.mark.ops("create")
def test_create_returns_its_user(api, resource):
    body = api.send("create", resource, user_id="alice").json()
    assert (body["createdBy"], body["updatedBy"]) == ("alice", "alice")


@pytest.mark.ops("get_by_id")
def test_get_by_id_returns_the_record(api, resource, make_record):
    record = make_record(resource)
    response = api.send("get_by_id", resource, record["id"])
    assert response.status_code == 200
    assert response.json() == record


@pytest.mark.ops("list")
def test_list_contains_the_record(api, resource, make_record):
    record = make_record(resource)
    assert record["id"] in api.list_ids(resource)


@pytest.mark.ops("list")
def test_list_returns_each_record_as_get_by_id_does(api, resource, make_record):
    record = make_record(resource, user_id="alice")
    items = api.send("list", resource, params={"limit": LIST_MAX}).json()
    assert next(item for item in items if item["id"] == record["id"]) == record


@pytest.mark.ops("list")
def test_list_honours_limit(api, resource, make_record):
    make_record(resource)
    make_record(resource)
    response = api.send("list", resource, params={"limit": 1})
    assert response.status_code == 200
    assert len(response.json()) == 1


@pytest.mark.ops("list")
@pytest.mark.parametrize("limit", ["0", "-1", "abc", "1000000"])
def test_list_falls_back_on_an_out_of_range_limit(api, resource, limit, make_record):
    make_record(resource)
    response = api.send("list", resource, params={"limit": limit})
    assert response.status_code == 200
    assert len(response.json()) >= 1


@pytest.mark.each_operation("update", "replace")
def test_a_write_returns_the_new_record(api, resource, operation, make_record, store):
    record = make_record(resource)
    name = unique_name(resource)
    response = api.send(operation, resource, record["id"], json={"name": name})
    assert response.status_code == 200
    body = response.json()
    assert body["name"] == name
    assert body["createdTimestamp"] == record["createdTimestamp"]
    assert body["updatedTimestamp"] >= record["updatedTimestamp"]
    assert body == response_of(store.get(resource.container, record["id"]))


@pytest.mark.ops("create")
@pytest.mark.each_operation("update", "replace")
def test_a_write_returns_its_user(api, resource, operation, make_record):
    record = make_record(resource, user_id="alice")
    body = api.send(operation, resource, record["id"], user_id="bob").json()
    assert (body["createdBy"], body["updatedBy"]) == ("alice", "bob")
    assert "updatedBy" not in api.send(operation, resource, record["id"]).json()


@pytest.mark.ops("get_by_id")
@pytest.mark.each_operation("update", "replace")
def test_a_write_is_visible_to_get_by_id(api, resource, operation, make_record):
    record = make_record(resource)
    response = api.send(operation, resource, record["id"], json={"name": unique_name(resource)})
    assert response.status_code == 200
    assert api.send("get_by_id", resource, record["id"]).json() == response.json()
