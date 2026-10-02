from errors import ValidationError

USER_ID_HEADER = "X-User-Id"
MAX_USER_ID_LENGTH = 256
USER_ID_TOO_LONG_MESSAGE = f"{USER_ID_HEADER} must be at most {MAX_USER_ID_LENGTH} characters."


def user_id_from(event: dict) -> str | None:
    # API Gateway keeps the client's header casing.
    headers = event.get("headers") or {}
    value = next((value for name, value in headers.items() if name.lower() == USER_ID_HEADER.lower()), None)
    user_id = (value or "").strip()
    if len(user_id) > MAX_USER_ID_LENGTH:
        raise ValidationError(USER_ID_TOO_LONG_MESSAGE)
    return user_id or None
