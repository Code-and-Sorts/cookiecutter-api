class BaseError(Exception):
    """An expected failure, answered with ``status_code`` and its message as
    ``{"errorMessage": ...}``. Anything else is an unexpected 500."""

    status_code: int = 500
    default_message: str = "An unexpected error occurred."

    def __init__(self, message: str | None = None):
        super().__init__(message or self.default_message)


class ValidationError(BaseError):
    """The request is malformed: invalid JSON, not an object, or fields that are
    unknown, missing or of the wrong type."""

    status_code = 400
    default_message = "The request is invalid."


class NotFoundError(BaseError):
    """No route matches the path, or the requested item does not exist."""

    status_code = 404
    default_message = "Not found."

    @classmethod
    def for_item(cls, resource: str, item_id: str | None) -> "NotFoundError":
        return cls(f"{resource} with id {item_id} was not found.")


class MethodNotAllowedError(BaseError):
    """The path exists but does not accept the request's HTTP method."""

    status_code = 405
    default_message = "Method not allowed."
