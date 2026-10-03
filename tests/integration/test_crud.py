from uuid import UUID

import pytest

from values import response, valid_body, written


@pytest.mark.ops("create")
def test_create_returns_the_new_record(api, resource):
    body = valid_body(resource, "create")
    created = api.send("create", resource, json=body)
    assert created.status_code == 201, created.text
    item = created.json()
    UUID(item["id"])
    assert item == response(resource, item["id"], written(resource, "create", body, None))


@pytest.mark.ops("get_by_id")
def test_get_by_id_returns_the_record(api, resource, make_record):
    record = make_record(resource)
    got = api.send("get_by_id", resource, record["id"])
    assert got.status_code == 200
    assert got.json() == record


@pytest.mark.ops("list")
def test_list_contains_the_record(api, resource, make_record):
    record = make_record(resource)
    assert record["id"] in api.list_ids(resource)


@pytest.mark.ops("list")
def test_list_honours_limit(api, resource, make_record):
    make_record(resource)
    make_record(resource)
    listed = api.send("list", resource, params={"limit": 1})
    assert listed.status_code == 200
    assert len(listed.json()) == 1


@pytest.mark.ops("list")
@pytest.mark.parametrize("limit", ["0", "-1", "abc", "1000000"])
def test_list_falls_back_on_an_out_of_range_limit(api, resource, limit, make_record):
    make_record(resource)
    listed = api.send("list", resource, params={"limit": limit})
    assert listed.status_code == 200
    assert len(listed.json()) >= 1


@pytest.mark.each_operation("update", "replace")
def test_a_write_returns_the_new_values(api, store, resource, operation, make_record):
    record = make_record(resource)
    before = store.get(resource.container, record["id"])
    body = valid_body(resource, operation)
    reply = api.send(operation, resource, record["id"], json=body)
    assert reply.status_code == 200, reply.text
    assert reply.json() == response(resource, record["id"], written(resource, operation, body, before))


@pytest.mark.ops("get_by_id")
@pytest.mark.each_operation("update", "replace")
def test_a_write_is_visible_to_get_by_id(api, resource, operation, make_record):
    record = make_record(resource)
    reply = api.send(operation, resource, record["id"])
    assert reply.status_code == 200, reply.text
    assert api.send("get_by_id", resource, record["id"]).json() == reply.json()
