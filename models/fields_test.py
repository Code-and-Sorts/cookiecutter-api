import re
from datetime import datetime, timezone
from unittest.mock import patch
import pytest
from pydantic import BaseModel, ConfigDict, ValidationError
from .fields import (
    MAX_SAFE_INTEGER,
    DateStr,
    DateTimeStr,
    Number,
    SafeInt,
    UuidStr,
    check_email,
    check_unique_items,
    check_uri,
    generate_utc_timestamp,
    new_uuid,
    utc_today,
)


class _Values(BaseModel):
    model_config = ConfigDict(strict=True)

    date: DateStr | None = None
    dateTime: DateTimeStr | None = None
    id: UuidStr | None = None
    count: SafeInt | None = None
    amount: Number | None = None


def describe_clock():
    def test_timestamp_is_utc_with_milliseconds():
        assert re.fullmatch(r"\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{3}Z", generate_utc_timestamp())

    def test_formats_current_time():
        fixed = datetime(2026, 9, 29, 22, 49, 26, 625999, tzinfo=timezone.utc)
        with patch("models.fields.datetime") as mock_datetime:
            mock_datetime.now.return_value = fixed
            assert generate_utc_timestamp() == "2026-09-29T22:49:26.625Z"
            assert utc_today() == "2026-09-29"

    def test_new_uuid_is_unique():
        assert new_uuid() != new_uuid()


def describe_value_types():
    @pytest.mark.parametrize("raw, stored", [
        ("2026-01-31T11:30:00.1239+02:00", "2026-01-31T09:30:00.123Z"),
        ("2026-01-31T00:30:00.123456789+23:59", "2026-01-30T00:31:00.123Z"),
        ("2026-01-31T23:30:00-23:59", "2026-02-01T23:29:00.000Z"),
        ("0001-01-01T00:00:00-00:00", "0001-01-01T00:00:00.000Z"),
        ("9999-12-31T23:59:59.999Z", "9999-12-31T23:59:59.999Z"),
    ])
    def test_date_time_is_stored_in_utc_with_milliseconds(raw, stored):
        assert _Values(dateTime=raw).dateTime == stored

    @pytest.mark.parametrize("values", [
        {"dateTime": "2026-01-31 09:30:00Z"},
        {"dateTime": "2026-02-30T09:30:00Z"},
        {"dateTime": "2026-01-31T09:30:00"},
        {"dateTime": "2026-01-31T24:00:00Z"},
        {"dateTime": "2026-01-31T23:59:60Z"},
        {"dateTime": "2026-01-31T09:30:00+14:60"},
        {"dateTime": "0000-12-31T23:00:00-01:00"},
        {"dateTime": "0001-01-01T00:00:00+01:00"},
        {"dateTime": "9999-12-31T23:59:59-01:00"},
        {"date": "2026-1-31"},
        {"date": "2026-02-30"},
        {"date": "2026-13-01"},
        {"date": "0000-01-01"},
        {"id": "not-a-uuid"},
        {"count": MAX_SAFE_INTEGER + 1},
        {"count": True},
        {"count": 1.5},
        {"amount": float("inf")},
        {"amount": "1"},
    ])
    def test_rejects_invalid_values(values):
        with pytest.raises(ValidationError):
            _Values(**values)

    def test_accepts_valid_values():
        values = _Values(date="2024-02-29", id="AC1DF01C-7ECE-4A20-AB60-179829DAD8F5", count=-MAX_SAFE_INTEGER, amount=3)
        assert (values.date, values.count, values.amount) == ("2024-02-29", -MAX_SAFE_INTEGER, 3)
        assert _Values(amount=2.5).amount == 2.5

    @pytest.mark.parametrize("raw, stored", [(2.0, 2), (1e3, 1000)])
    def test_integer_accepts_a_whole_float(raw, stored):
        count = _Values(count=raw).count
        assert (count, type(count)) == (stored, int)

    @pytest.mark.parametrize("check, valid, invalid", [
        (check_email, "a@example.com", "not-an-email"),
        (check_uri, "https://example.com/x", "not a uri"),
        (check_unique_items, ["a", "b"], ["a", "a"]),
    ])
    def test_checks(check, valid, invalid):
        assert check(valid) == valid
        with pytest.raises(ValueError):
            check(invalid)
