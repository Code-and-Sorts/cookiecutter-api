import re
from datetime import datetime, timezone
from unittest.mock import patch
from .base import generate_utc_timestamp


def describe_generate_utc_timestamp():
    def test_iso_8601_utc_with_milliseconds_and_z_suffix():
        assert re.fullmatch(r"\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{3}Z", generate_utc_timestamp())

    def test_formats_current_time():
        fixed = datetime(2026, 9, 29, 22, 49, 26, 625999, tzinfo=timezone.utc)
        with patch("models.base.datetime") as mock_datetime:
            mock_datetime.now.return_value = fixed
            assert generate_utc_timestamp() == "2026-09-29T22:49:26.625Z"
