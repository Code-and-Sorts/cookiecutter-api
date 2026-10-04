import re
from datetime import datetime, timezone
from unittest.mock import patch
from .base import BaseResponse, generate_utc_timestamp
from conftest import ITEM_ID

_CREATED = "2024-08-10T20:41:30.123Z"
_UPDATED = "2026-01-01T00:00:00.000Z"
_stored = {
    "id": ITEM_ID,
    "name": "mockName",
    "isDeleted": False,
    "createdTimestamp": _CREATED,
    "updatedTimestamp": _UPDATED,
}


def describe_generate_utc_timestamp():
    def test_iso_8601_utc_with_milliseconds_and_z_suffix():
        assert re.fullmatch(r"\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{3}Z", generate_utc_timestamp())

    def test_formats_current_time():
        fixed = datetime(2026, 9, 29, 22, 49, 26, 625999, tzinfo=timezone.utc)
        with patch("models.base.datetime") as mock_datetime:
            mock_datetime.now.return_value = fixed
            assert generate_utc_timestamp() == "2026-09-29T22:49:26.625Z"


class _ItemResponse(BaseResponse):
    name: str


def describe_base_response():
    def test_returns_stored_fields_in_order_with_users():
        stored = {**_stored, "updatedBy": "editor", "createdBy": "creator"}

        assert list(_ItemResponse.model_validate(stored).model_dump().items()) == [
            ("id", ITEM_ID),
            ("name", "mockName"),
            ("createdTimestamp", _CREATED),
            ("createdBy", "creator"),
            ("updatedTimestamp", _UPDATED),
            ("updatedBy", "editor"),
        ]

    def test_omits_unset_users():
        assert _ItemResponse.model_validate(_stored).model_dump() == {
            "id": ITEM_ID,
            "name": "mockName",
            "createdTimestamp": _CREATED,
            "updatedTimestamp": _UPDATED,
        }

    def test_drops_is_deleted_and_database_metadata():
        stored = {**_stored, "_etag": "etag-1", "_rid": "rid", "_ts": 1, "extra": "kept in storage"}

        assert set(_ItemResponse.model_validate(stored).model_dump()) == {
            "id", "name", "createdTimestamp", "updatedTimestamp",
        }

    def test_keeps_resource_fields_between_id_and_audit_fields():
        class _Pet(BaseResponse):
            name: str
            age: int

        stored = {**_stored, "age": 3}

        assert list(_Pet.model_validate(stored).model_dump()) == [
            "id", "name", "age", "createdTimestamp", "updatedTimestamp",
        ]
