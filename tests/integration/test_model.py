"""Defaults, nulls, date-times, hidden fields and read defaults, for every field the answers declare."""

import pytest

from project import FIELDS
from store import new_record
from values import assert_stored, minimal_body, response, written


@pytest.mark.ops("create")
def test_create_gives_fields_left_out_their_defaults(api, store, resource):
    body = minimal_body(resource, "create")
    created = api.send("create", resource, json=body)
    assert created.status_code == 201, created.text
    item = created.json()
    values = written(resource, "create", body, None)
    assert item == response(resource, item["id"], values)
    assert_stored(resource, store.get(resource.container, item["id"]), values)


@pytest.mark.ops("replace")
def test_replace_defaults_accepted_fields_left_out_and_keeps_the_rest(api, store, resource, make_record):
    record = make_record(resource)
    before = store.get(resource.container, record["id"])
    body = minimal_body(resource, "replace")
    replaced = api.send("replace", resource, record["id"], json=body)
    assert replaced.status_code == 200, replaced.text
    assert replaced.json() == response(resource, record["id"], written(resource, "replace", body, before))


@pytest.mark.ops("update")
def test_an_empty_update_keeps_the_record(api, resource, make_record):
    record = make_record(resource)
    updated = api.send("update", resource, record["id"], json={})
    assert updated.status_code == 200, updated.text
    assert updated.json() == record


@pytest.mark.fields("nullable")
def test_an_update_with_null_clears_a_nullable_field(api, store, resource, operation, field, make_record):
    record = make_record(resource)
    updated = api.send(operation, resource, record["id"], json={field.name: None})
    assert updated.status_code == 200, updated.text
    if not field.hidden:
        assert updated.json()[field.name] is None
    assert field.name not in store.get(resource.container, record["id"])


@pytest.mark.fields("date_time")
def test_a_date_time_is_stored_in_utc_with_milliseconds(api, store, resource, operation, field):
    example = FIELDS.DATE_TIME_EXAMPLE
    body = {**minimal_body(resource, operation), field.name: example["sent"]}
    created = api.send(operation, resource, json=body)
    assert created.status_code == 201, created.text
    item = created.json()
    if not field.hidden:
        assert item[field.name] == example["stored"]
    assert store.get(resource.container, item["id"])[field.name] == example["stored"]


@pytest.mark.ops("create")
@pytest.mark.fields("hidden")
def test_a_hidden_field_is_stored_but_never_returned(api, store, resource, field):
    created = api.send("create", resource)
    assert created.status_code == 201, created.text
    item = created.json()
    assert field.name not in item
    if field.accepted("create"):
        assert field.name in store.get(resource.container, item["id"])


@pytest.mark.ops("get_by_id")
def test_a_record_without_a_field_reads_its_static_default(api, store, resource):
    system = ("id", "isDeleted", "createdTimestamp", "updatedTimestamp")
    record = {key: value for key, value in new_record(resource).items() if key in system}
    store.put(resource.container, record)
    got = api.send("get_by_id", resource, record["id"])
    assert got.status_code == 200, got.text
    assert got.json() == response(resource, record["id"], {})


@pytest.mark.ops("create")
def test_dynamic_uuid_defaults_are_fresh_on_every_write(api, resource):
    fields = [f for f in resource.shown if f.dynamic == "uuid"]
    if not fields:
        pytest.skip("the resource has no $uuid default")
    body = minimal_body(resource, "create")
    first, second = (api.send("create", resource, json=body).json() for _ in range(2))
    for field in fields:
        assert first[field.name] != second[field.name], field.name
