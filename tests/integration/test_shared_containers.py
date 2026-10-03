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


def test_a_write_keeps_the_fields_only_a_sibling_resource_knows(api, store, writer, reader, make_record):
    writes = [op for op in ("update", "replace", "delete") if reader.has(op)]
    unknown = {f.name for f in writer.fields} - {f.name for f in reader.fields}
    if not writes or not unknown:
        pytest.skip(f"{reader} cannot write, or knows every field {writer} stores")
    for write in writes:
        record = make_record(writer)
        before = store.get(writer.container, record["id"])
        reply = api.send(write, reader, record["id"])
        assert reply.status_code == 200, f"{write}: {reply.text}"
        after = store.get(writer.container, record["id"])
        assert {name: after.get(name) for name in unknown} == {name: before.get(name) for name in unknown}, write
