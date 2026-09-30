import json
from uuid import UUID
from pydantic import BaseModel
from pydantic import ValidationError as PydanticValidationError
from errors import NotFoundError, ValidationError


def require_uuid(item_id: str | None, resource: str) -> str:
    """A non-UUID can never name an item, so it is a 404 rather than a 400."""
    try:
        UUID(str(item_id))
    except ValueError:
        raise NotFoundError.for_item(resource, item_id) from None
    return str(item_id)


_PROBLEMS = {
    "extra_forbidden": "is not an allowed field",
    "missing": "is required",
    "string_type": "must be a string",
    "string_too_short": "must not be empty",
}


def _describe(error: PydanticValidationError) -> str:
    problems = []
    for detail in error.errors():
        field = ".".join(str(part) for part in detail["loc"]) or "body"
        problems.append(f"'{field}' {_PROBLEMS.get(detail['type'], detail['msg'])}")
    return "; ".join(problems)


def parse_body[ModelT: BaseModel](raw: bytes | str | None, model: type[ModelT]) -> ModelT:
    try:
        data = json.loads(raw or b"")
    except ValueError:  # includes JSONDecodeError and UnicodeDecodeError
        raise ValidationError("Request body must be valid JSON.") from None
    if not isinstance(data, dict):
        raise ValidationError("Request body must be a JSON object.")
    try:
        return model.model_validate(data)
    except PydanticValidationError as error:
        raise ValidationError(f"Invalid request body: {_describe(error)}.") from None
