from uuid import UUID

import pytest

from api import unique_name


@pytest.mark.ops("create")
def test_create_returns_the_new_record(api, resource):
    name = unique_name(resource)
    response = api.send("create", resource, json={"name": name})
    assert response.status_code == 201
    body = response.json()
    assert body == {"id": body["id"], "name": name}
    UUID(body["id"])


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
def test_a_write_returns_the_new_name(api, resource, operation, make_record):
    record = make_record(resource)
    name = unique_name(resource)
    response = api.send(operation, resource, record["id"], json={"name": name})
    assert response.status_code == 200
    assert response.json() == {"id": record["id"], "name": name}


@pytest.mark.ops("get_by_id")
@pytest.mark.each_operation("update", "replace")
def test_a_write_is_visible_to_get_by_id(api, resource, operation, make_record):
    record = make_record(resource)
    name = unique_name(resource)
    assert api.send(operation, resource, record["id"], json={"name": name}).status_code == 200
    assert api.send("get_by_id", resource, record["id"]).json() == {"id": record["id"], "name": name}
