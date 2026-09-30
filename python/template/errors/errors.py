class BaseError(Exception):
    status_code: int = 500
    default_message: str = "An unexpected error occurred."

    def __init__(self, message: str | None = None):
        super().__init__(message or self.default_message)


class ValidationError(BaseError):
    status_code = 400
    default_message = "The request is invalid."


class NotFoundError(BaseError):
    status_code = 404
    default_message = "Not found."

    @classmethod
    def for_item(cls, resource: str, item_id: str | None) -> "NotFoundError":
        return cls(f"{resource} with id {item_id} was not found.")


class MethodNotAllowedError(BaseError):
    status_code = 405
    default_message = "Method not allowed."
