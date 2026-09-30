import pytest
from pydantic import BaseModel, ConfigDict, Field
from errors import NotFoundError, ValidationError
from .validation import parse_body, require_uuid
from conftest import ITEM_ID


class _Body(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)

    name: str = Field(min_length=1)


def describe_require_uuid():
    def test_returns_valid_uuid():
        assert require_uuid(ITEM_ID, "Thing") == ITEM_ID

    @pytest.mark.parametrize("item_id", ["not-a-uuid", "", None, "123"])
    def test_raises_not_found_for_non_uuid(item_id):
        with pytest.raises(NotFoundError) as error:
            require_uuid(item_id, "Thing")
        assert error.value.status_code == 404
        assert str(error.value) == f"Thing with id {item_id} was not found."


def describe_parse_body():
    def test_parses_valid_object():
        assert parse_body(b'{"name": "Tom"}', _Body) == _Body(name="Tom")
        assert parse_body('{"name": "Tom"}', _Body) == _Body(name="Tom")

    @pytest.mark.parametrize("raw", [None, b"", b"{", b"not json", b"\xff\xfe"])
    def test_rejects_malformed_json(raw):
        with pytest.raises(ValidationError) as error:
            parse_body(raw, _Body)
        assert error.value.status_code == 400
        assert str(error.value) == "Request body must be valid JSON."

    @pytest.mark.parametrize("raw", [b"[]", b'"name"', b"42", b"null", b"true"])
    def test_rejects_non_object(raw):
        with pytest.raises(ValidationError) as error:
            parse_body(raw, _Body)
        assert str(error.value) == "Request body must be a JSON object."

    @pytest.mark.parametrize("raw, problem", [
        (b'{"name": "Tom", "id": "' + ITEM_ID.encode() + b'"}', "'id' is not an allowed field"),
        (b'{"name": "Tom", "isDeleted": true}', "'isDeleted' is not an allowed field"),
        (b'{"name": "Tom", "createdTimestamp": "2026-01-01T00:00:00.000Z"}', "'createdTimestamp' is not an allowed field"),
        (b'{"name": 42}', "'name' must be a string"),
        (b'{"name": true}', "'name' must be a string"),
        (b'{"name": null}', "'name' must be a string"),
        (b'{"name": ""}', "'name' must not be empty"),
        (b'{}', "'name' is required"),
    ])
    def test_rejects_invalid_fields(raw, problem):
        with pytest.raises(ValidationError) as error:
            parse_body(raw, _Body)
        assert error.value.status_code == 400
        assert str(error.value) == f"Invalid request body: {problem}."

    def test_lists_every_problem():
        with pytest.raises(ValidationError) as error:
            parse_body(b'{"id": "x", "name": 1}', _Body)
        assert str(error.value) == "Invalid request body: 'name' must be a string; 'id' is not an allowed field."
