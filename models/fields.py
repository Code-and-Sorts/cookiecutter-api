import re
import uuid
from datetime import date, datetime, timezone
from typing import Annotated, Union
from pydantic import AfterValidator, BeforeValidator, Field, StrictFloat, StrictInt, StrictStr

# JavaScript numbers hold integers exactly only up to here, so every language caps integers at it.
MAX_SAFE_INTEGER = 9007199254740991

_DATE = re.compile("^(000[1-9]|00[1-9][0-9]|0[1-9][0-9]{2}|[1-9][0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$")
_DATE_TIME = re.compile("^(000[1-9]|00[1-9][0-9]|0[1-9][0-9]{2}|[1-9][0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\\.[0-9]{1,9})?(Z|[+-]([01][0-9]|2[0-3]):[0-5][0-9])$")
_UUID = re.compile("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
_EMAIL = re.compile("^[^@ \\t\\n]+@[^@ \\t\\n]+\\.[^@ \\t\\n]+$")
_URI = re.compile("^[A-Za-z][A-Za-z0-9+.-]*:[^ \\t\\n]+$")
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
    try:
        return format_timestamp(datetime.fromisoformat(_EXTRA_DIGITS.sub(r"\1", value)))
    except OverflowError:
        raise ValueError("must fall between the years 0001 and 9999 in UTC") from None


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


def whole_number(value: object) -> object:
    """JSON clients cannot always tell 1 from 1.0, so an integer may arrive as a whole float."""
    return int(value) if isinstance(value, float) and value.is_integer() else value


SafeInt = Annotated[StrictInt, Field(ge=-MAX_SAFE_INTEGER, le=MAX_SAFE_INTEGER), BeforeValidator(whole_number)]
# Integers stay integers, so a number round-trips as the client sent it.
Number = Union[SafeInt, Annotated[StrictFloat, Field(allow_inf_nan=False)]]
DateStr = Annotated[StrictStr, AfterValidator(check_date)]
DateTimeStr = Annotated[StrictStr, AfterValidator(to_utc_timestamp)]
UuidStr = Annotated[StrictStr, AfterValidator(check_uuid)]
