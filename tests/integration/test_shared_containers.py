import pytest


@pytest.mark.ops("get_by_id")
def test_a_record_is_visible_through_a_resource_sharing_its_container(api, writer, reader, make_record):
    record = make_record(writer)
    response = api.send("get_by_id", reader, record["id"])
    assert response.status_code == 200
    assert response.json() == record


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
