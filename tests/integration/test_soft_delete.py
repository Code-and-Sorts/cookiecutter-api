import pytest


@pytest.mark.ops("delete")
def test_delete_soft_deletes_the_record(api, resource, make_record, store):
    record = make_record(resource)
    response = api.send("delete", resource, record["id"])
    assert response.status_code == 200
    assert response.json() == {"message": f"{resource.name} with id {record['id']} was deleted successfully."}
    assert store.get(resource.container, record["id"])["isDeleted"] is True


@pytest.mark.ops("delete")
@pytest.mark.each_operation("get_by_id", "update", "replace", "delete")
def test_a_deleted_record_is_not_found(api, resource, operation, make_record):
    record = make_record(resource)
    assert api.send("delete", resource, record["id"]).status_code == 200
    response = api.send(operation, resource, record["id"])
    assert response.status_code == 404
    assert isinstance(response.json()["errorMessage"], str)


@pytest.mark.ops("delete", "list")
def test_list_excludes_a_deleted_record(api, resource, make_record):
    record = make_record(resource)
    assert api.send("delete", resource, record["id"]).status_code == 200
    assert record["id"] not in api.list_ids(resource)
