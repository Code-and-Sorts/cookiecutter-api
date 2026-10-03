"""The stored record contract, read straight from the emulator."""

import re

import pytest

from api import unique_name

TIMESTAMP = re.compile(r"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$")
REQUIRED_FIELDS = {"id", "name", "isDeleted", "createdTimestamp", "updatedTimestamp"}
USER_FIELDS = {"createdBy", "updatedBy"}


def stored(store, resource, item_id: str) -> dict:
    record = store.get(resource.container, item_id)
    assert record is not None, f"{item_id} is not in {resource.container}"
    assert REQUIRED_FIELDS <= record.keys() <= REQUIRED_FIELDS | USER_FIELDS
    assert TIMESTAMP.match(record["createdTimestamp"]), record
    assert TIMESTAMP.match(record["updatedTimestamp"]), record
    return record


@pytest.mark.ops("create")
def test_create_stores_the_contract_fields(api, resource, make_record, store):
    record = make_record(resource)
    saved = stored(store, resource, record["id"])
    assert saved["name"] == record["name"]
    assert saved["isDeleted"] is False
    assert saved["createdTimestamp"] == saved["updatedTimestamp"]
    assert not USER_FIELDS & saved.keys()


@pytest.mark.ops("create")
def test_create_records_the_user_id(api, resource, make_record, store):
    saved = stored(store, resource, make_record(resource, user_id="alice")["id"])
    assert (saved["createdBy"], saved["updatedBy"]) == ("alice", "alice")


@pytest.mark.ops("create")
@pytest.mark.each_operation("update", "replace", "delete")
def test_a_write_keeps_the_created_fields_and_records_its_user(api, resource, operation, make_record, store):
    record = make_record(resource, user_id="alice")
    before = stored(store, resource, record["id"])
    assert api.send(operation, resource, record["id"], user_id="bob").status_code == 200
    after = stored(store, resource, record["id"])
    assert (after["createdTimestamp"], after["createdBy"]) == (before["createdTimestamp"], "alice")
    assert after["updatedTimestamp"] >= before["updatedTimestamp"]
    assert after["updatedBy"] == "bob"


@pytest.mark.ops("create")
@pytest.mark.each_operation("update", "replace", "delete")
def test_a_write_without_a_user_id_clears_updated_by(api, resource, operation, make_record, store):
    record = make_record(resource, user_id="alice")
    assert api.send(operation, resource, record["id"]).status_code == 200
    saved = stored(store, resource, record["id"])
    assert saved["createdBy"] == "alice"
    assert "updatedBy" not in saved


@pytest.mark.each_operation("update", "replace")
def test_a_write_stores_the_new_name(api, resource, operation, make_record, store):
    record = make_record(resource)
    name = unique_name(resource)
    assert api.send(operation, resource, record["id"], json={"name": name}).status_code == 200
    saved = stored(store, resource, record["id"])
    assert (saved["name"], saved["isDeleted"]) == (name, False)
