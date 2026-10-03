{%- from 'shared/_fields.jinja' import MAX_SAFE_INTEGER, PATTERNS -%}
import re
import uuid
from datetime import date, datetime, timezone
from typing import Annotated, Union
from pydantic import AfterValidator, Field, StrictFloat, StrictInt, StrictStr

# JavaScript numbers hold integers exactly only up to here, so every language caps integers at it.
MAX_SAFE_INTEGER = {{ MAX_SAFE_INTEGER }}

_DATE = re.compile({{ PATTERNS.date | tojson }})
_DATE_TIME = re.compile({{ PATTERNS.date_time | tojson }})
_UUID = re.compile({{ PATTERNS.uuid | tojson }})
_EMAIL = re.compile({{ PATTERNS.email | tojson }})
_URI = re.compile({{ PATTERNS.uri | tojson }})
# datetime parses at most six fractional digits.
_EXTRA_DIGITS = re.compile(r"(\.[0-9]{6})[0-9]+")


def format_timestamp(value: datetime) -> str:
    return value.astimezone(timezone.utc).isoformat(timespec="milliseconds").replace("+00:00", "Z")


def generate_utc_timestamp() -> str:
    return format_timestamp(datetime.now(timezone.utc))


def utc_today() -> str:
    return datetime.now(timezone.utc).date().isoformat()


def new_uuid() -> str:
    return str(uuid.uuid4())


def check_date(value: str) -> str:
    if not _DATE.fullmatch(value):
        raise ValueError("must be a date such as 2026-01-31")
    date.fromisoformat(value)
    return value


def to_utc_timestamp(value: str) -> str:
    """Accepts RFC 3339 with any offset and stores UTC with milliseconds, like the system timestamps."""
    if not _DATE_TIME.fullmatch(value):
        raise ValueError("must be a date-time with a time zone, such as 2026-01-31T09:30:00Z")
    return format_timestamp(datetime.fromisoformat(_EXTRA_DIGITS.sub(r"\1", value)))


def check_uuid(value: str) -> str:
    if not _UUID.fullmatch(value):
        raise ValueError("must be a UUID")
    return value


def check_email(value: str) -> str:
    if not _EMAIL.fullmatch(value):
        raise ValueError("must be an email address")
    return value


def check_uri(value: str) -> str:
    if not _URI.fullmatch(value):
        raise ValueError("must be an absolute URI")
    return value


def check_unique_items(values: list) -> list:
    if len(set(values)) != len(values):
        raise ValueError("must not repeat an item")
    return values


SafeInt = Annotated[StrictInt, Field(ge=-MAX_SAFE_INTEGER, le=MAX_SAFE_INTEGER)]
# Integers stay integers, so a number round-trips as the client sent it.
Number = Union[SafeInt, Annotated[StrictFloat, Field(allow_inf_nan=False)]]
DateStr = Annotated[StrictStr, AfterValidator(check_date)]
DateTimeStr = Annotated[StrictStr, AfterValidator(to_utc_timestamp)]
UuidStr = Annotated[StrictStr, AfterValidator(check_uuid)]
