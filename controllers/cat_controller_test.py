import asyncio
import pytest
from unittest.mock import AsyncMock
from errors import NotFoundError, ValidationError
from models import CatCreateRequest, CatReplaceRequest, CatUpdateRequest, CatResponse
from controllers import CatController
from services import CatService
from controllers.pagination import DEFAULT_LIST_LIMIT
from conftest import ITEM_ID, encode, invalid_bodies

_response = CatResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z"})
_REJECTED = {
    "tenantId": [42, None, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
    "region": ["__invalid__", None, "EU"],
    "priority": ["1", -1, 1.5, 9007199254740992],
    "rank": ["1", None],
    "labels": ["not-a-list", None, ["item1", "item1"]],
    "name": [42, None, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
    "breed": ["__invalid__", None, "SIAMESE"],
    "ageYears": ["1", None, -1, 41, 1.5, 9007199254740992],
    "weightKg": ["1", 0, 100],
    "indoor": ["true", None],
    "birthDate": ["not-a-date", None, "2026-02-30", "2026-13-01", "0000-01-01"],
    "microchipId": ["not-a-uuid", None],
    "ownerEmail": [42, None, "not-an-email"],
    "website": [42, "not a uri"],
    "tagCode": [42, None, "!"],
    "tags": ["not-a-list", None, ["item1", "item1"], ["item1", "item2", "item3", "item4"]],
    "scores": ["not-a-list", [None], []],
    "adoptedAt": ["not-a-date-time", "2026-01-31T09:30:00", "2026-01-31T24:00:00Z", "2026-01-31T23:59:60Z", "2026-01-31T09:30:00+14:60", "0000-12-31T23:00:00-01:00", "0001-01-01T00:00:00+01:00", "9999-12-31T23:59:59-01:00"],
    "lastVisit": ["not-a-date-time", None, "2026-01-31T09:30:00", "2026-01-31T24:00:00Z", "2026-01-31T23:59:60Z", "2026-01-31T09:30:00+14:60", "0000-12-31T23:00:00-01:00", "0001-01-01T00:00:00+01:00", "9999-12-31T23:59:59-01:00"],
    "notes": [42, None],
}
_BOUNDARY_VALUES = [
    ("tenantId", "s"),
    ("tenantId", "😺"),
    ("tenantId", "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss"),
    ("tenantId", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"),
    ("priority", 0),
    ("priority", 1.0),
    ("name", "s"),
    ("name", "😺"),
    ("name", "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss"),
    ("name", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"),
    ("ageYears", 0),
    ("ageYears", 40),
    ("ageYears", 0.0),
    ("tags", ["item1", "item2", "item3"]),
    ("scores", [1]),
    ("adoptedAt", "0001-01-01T00:00:00.000Z"),
    ("adoptedAt", "9999-12-31T23:59:59.999Z"),
    ("lastVisit", "0001-01-01T00:00:00.000Z"),
    ("lastVisit", "9999-12-31T23:59:59.999Z"),
]

_CREATE_BODY = {"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"}
_CREATE_INVALID = invalid_bodies(
    _CREATE_BODY,
    ["rank", "name"],
    {},
    _REJECTED,
)
_CREATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _CREATE_BODY]

_UPDATE_BODY = {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "ageYears": 0, "weightKg": 1.5, "indoor": True, "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tags": [], "adoptedAt": "2026-01-15T10:00:00.000Z", "notes": "$none"}
_UPDATE_INVALID = invalid_bodies(
    _UPDATE_BODY,
    [],
    {"region": "eu", "breed": "tabby", "birthDate": "2026-01-01", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000", "tagCode": "ABC-123", "scores": [1], "lastVisit": "2026-01-01T00:00:00.000Z"},
    _REJECTED,
)
_UPDATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _UPDATE_BODY]

_REPLACE_BODY = {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample", "breed": "tabby", "ageYears": 0, "weightKg": 1.5, "indoor": True, "birthDate": "2026-01-01", "ownerEmail": "unknown@example.com", "website": "https://example.com/items/1", "tagCode": "ABC-123", "tags": [], "scores": [1], "adoptedAt": "2026-01-15T10:00:00.000Z", "lastVisit": "2026-01-01T00:00:00.000Z", "notes": "$none"}
_REPLACE_INVALID = invalid_bodies(
    _REPLACE_BODY,
    ["rank", "name"],
    {"region": "eu", "microchipId": "6f1c2a3b-4d5e-4f60-8a7b-000000000000"},
    _REJECTED,
)
_REPLACE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _REPLACE_BODY]


def describe_cat_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock(CatService)

    @pytest.fixture
    def controller(mock_service):
        return CatController(service=mock_service)

    def describe_get_by_id():
        def test_returns_item(controller, mock_service):
            mock_service.get_by_id.return_value = _response

            assert asyncio.run(controller.get_by_id(ITEM_ID)) == _response
            mock_service.get_by_id.assert_awaited_once_with(ITEM_ID)

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError) as error:
                asyncio.run(controller.get_by_id("not-a-uuid"))

            assert str(error.value) == "Cat with id not-a-uuid was not found."
            mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_service.get_list.return_value = []

            assert asyncio.run(controller.get_list(None)) == []
            mock_service.get_list.assert_awaited_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_service.get_list.return_value = [_response]

            assert asyncio.run(controller.get_list("5")) == [_response]
            mock_service.get_list.assert_awaited_once_with(5)

        def test_invalid_limit_falls_back_to_default(controller, mock_service):
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list("abc"))
            mock_service.get_list.assert_awaited_once_with(DEFAULT_LIST_LIMIT)

    def describe_create():
        def test_passes_validated_body(controller, mock_service):
            mock_service.create.return_value = _response

            assert asyncio.run(controller.create(encode(_CREATE_BODY), "user-1")) == _response
            sent = mock_service.create.call_args.args[-2]
            assert isinstance(sent, CatCreateRequest)
            assert sent.model_dump(exclude_unset=True) == _CREATE_BODY
            assert mock_service.create.call_args.args[-1] == "user-1"

        def test_fields_left_out_get_their_defaults(controller, mock_service):
            mock_service.create.return_value = _response
            minimal = {"rank": 1.5, "name": "sample"}

            asyncio.run(controller.create(encode(minimal)))
            sent = mock_service.create.call_args.args[-2].model_dump()
            assert sent["tenantId"] == "public"
            assert sent["region"] == "eu"
            assert sent["priority"] is None
            assert sent["labels"] == []
            assert sent["breed"] == "tabby"
            assert sent["ageYears"] == 0
            assert sent["weightKg"] is None
            assert sent["indoor"] is True
            assert sent["birthDate"] is not None
            assert sent["microchipId"] is not None
            assert sent["ownerEmail"] == "unknown@example.com"
            assert sent["website"] is None
            assert sent["tagCode"] is None
            assert sent["tags"] == []
            assert sent["scores"] is None
            assert sent["adoptedAt"] is not None
            assert sent["lastVisit"] == "2026-01-01T00:00:00.000Z"
            assert sent["notes"] == "$none"

        @pytest.mark.parametrize("body", _CREATE_INVALID)
        def test_invalid_body_is_rejected(controller, mock_service, body):
            with pytest.raises(ValidationError):
                asyncio.run(controller.create(body))
            mock_service.create.assert_not_called()

        @pytest.mark.parametrize("name, value", _CREATE_BOUNDARIES)
        def test_edge_values_are_accepted(controller, mock_service, name, value):
            mock_service.create.return_value = _response

            asyncio.run(controller.create(encode({**_CREATE_BODY, name: value})))
            assert getattr(mock_service.create.call_args.args[-2], name) == value

    def describe_update():
        def test_passes_validated_body(controller, mock_service):
            mock_service.update.return_value = _response

            assert asyncio.run(controller.update(ITEM_ID, encode(_UPDATE_BODY), "user-1")) == _response
            sent = mock_service.update.call_args.args[-2]
            assert isinstance(sent, CatUpdateRequest)
            assert sent.model_dump(exclude_unset=True) == _UPDATE_BODY
            assert mock_service.update.call_args.args[-1] == "user-1"

        def test_empty_object_changes_nothing(controller, mock_service):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, b'{}'))
            assert mock_service.update.call_args.args[1].model_dump(exclude_unset=True) == {}

        def test_null_clears_priority(controller, mock_service):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, b'{"priority": null}'))
            assert mock_service.update.call_args.args[1].model_dump(exclude_unset=True) == {"priority": None}

        def test_null_clears_weight_kg(controller, mock_service):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, b'{"weightKg": null}'))
            assert mock_service.update.call_args.args[1].model_dump(exclude_unset=True) == {"weightKg": None}

        def test_null_clears_website(controller, mock_service):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, b'{"website": null}'))
            assert mock_service.update.call_args.args[1].model_dump(exclude_unset=True) == {"website": None}

        def test_null_clears_adopted_at(controller, mock_service):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, b'{"adoptedAt": null}'))
            assert mock_service.update.call_args.args[1].model_dump(exclude_unset=True) == {"adoptedAt": None}

        @pytest.mark.parametrize("body", _UPDATE_INVALID)
        def test_invalid_body_is_rejected(controller, mock_service, body):
            with pytest.raises(ValidationError):
                asyncio.run(controller.update(ITEM_ID, body))
            mock_service.update.assert_not_called()

        @pytest.mark.parametrize("name, value", _UPDATE_BOUNDARIES)
        def test_edge_values_are_accepted(controller, mock_service, name, value):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, encode({**_UPDATE_BODY, name: value})))
            assert getattr(mock_service.update.call_args.args[-2], name) == value

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError):
                asyncio.run(controller.update("not-a-uuid", encode(_UPDATE_BODY)))
            mock_service.update.assert_not_called()

    def describe_replace():
        def test_passes_validated_body(controller, mock_service):
            mock_service.replace.return_value = _response

            assert asyncio.run(controller.replace(ITEM_ID, encode(_REPLACE_BODY), "user-1")) == _response
            sent = mock_service.replace.call_args.args[-2]
            assert isinstance(sent, CatReplaceRequest)
            assert sent.model_dump(exclude_unset=True) == _REPLACE_BODY
            assert mock_service.replace.call_args.args[-1] == "user-1"

        def test_fields_left_out_get_their_defaults(controller, mock_service):
            mock_service.replace.return_value = _response
            minimal = {"rank": 1.5, "name": "sample"}

            asyncio.run(controller.replace(ITEM_ID, encode(minimal)))
            sent = mock_service.replace.call_args.args[-2].model_dump()
            assert sent["tenantId"] == "public"
            assert sent["priority"] is None
            assert sent["labels"] == []
            assert sent["breed"] == "tabby"
            assert sent["ageYears"] == 0
            assert sent["weightKg"] is None
            assert sent["indoor"] is True
            assert sent["birthDate"] is not None
            assert sent["ownerEmail"] == "unknown@example.com"
            assert sent["website"] is None
            assert sent["tagCode"] is None
            assert sent["tags"] == []
            assert sent["scores"] is None
            assert sent["adoptedAt"] is not None
            assert sent["lastVisit"] == "2026-01-01T00:00:00.000Z"
            assert sent["notes"] == "$none"

        @pytest.mark.parametrize("body", _REPLACE_INVALID)
        def test_invalid_body_is_rejected(controller, mock_service, body):
            with pytest.raises(ValidationError):
                asyncio.run(controller.replace(ITEM_ID, body))
            mock_service.replace.assert_not_called()

        @pytest.mark.parametrize("name, value", _REPLACE_BOUNDARIES)
        def test_edge_values_are_accepted(controller, mock_service, name, value):
            mock_service.replace.return_value = _response

            asyncio.run(controller.replace(ITEM_ID, encode({**_REPLACE_BODY, name: value})))
            assert getattr(mock_service.replace.call_args.args[-2], name) == value

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError):
                asyncio.run(controller.replace("not-a-uuid", encode(_REPLACE_BODY)))
            mock_service.replace.assert_not_called()

    def describe_delete():
        def test_returns_confirmation_message(controller, mock_service):
            result = asyncio.run(controller.delete(ITEM_ID, "user-1"))

            assert result == {"message": f"Cat with id {ITEM_ID} was deleted successfully."}
            mock_service.soft_delete.assert_awaited_once_with(ITEM_ID, "user-1")

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError):
                asyncio.run(controller.delete("not-a-uuid"))
            mock_service.soft_delete.assert_not_called()
