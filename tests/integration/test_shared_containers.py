import pytest

from values import response_from_record


@pytest.mark.ops("get_by_id")
def test_a_record_is_visible_through_a_resource_sharing_its_container(api, store, writer, reader, make_record):
    record = make_record(writer)
    got = api.send("get_by_id", reader, record["id"])
    assert got.status_code == 200
    # The reader sees the stored record through its own model: its fields, with read defaults for the rest.
    assert got.json() == response_from_record(reader, store.get(writer.container, record["id"]))


@pytest.mark.ops("list")
def test_a_record_is_listed_by_a_resource_sharing_its_container(api, writer, reader, make_record):
    assert make_record(writer)["id"] in api.list_ids(reader)


@pytest.mark.ops("get_by_id")
def test_a_record_is_not_visible_through_another_container(api, writer, outsider, make_record):
    record = make_record(writer)
    assert api.send("get_by_id", outsider, record["id"]).status_code == 404


@pytest.mark.ops("list")
def test_a_record_is_not_listed_by_another_container(api, writer, outsider, make_record):
    assert make_record(writer)["id"] not in api.list_ids(outsider)
