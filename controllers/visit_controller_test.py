import asyncio
import pytest
from unittest.mock import AsyncMock
from errors import NotFoundError, ValidationError
from models import VisitCreateRequest, VisitUpdateRequest, VisitResponse
from controllers import VisitController
from services import VisitService
from conftest import ITEM_ID, encode, invalid_bodies

_response = VisitResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]})
_REJECTED = {
    "tenantId": [42, None, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
    "region": ["__invalid__", None, "EU"],
    "priority": ["1", -1, 1.5, 9007199254740992],
    "rank": ["1", None],
    "labels": ["not-a-list", None, ["item1", "item1"]],
    "reason": [42, None],
    "visitedOn": ["not-a-date", None, "2026-02-30", "2026-13-01", "0000-01-01"],
    "cost": ["1", None, -1],
    "paid": ["true", None],
    "checkedAt": ["not-a-list", None, [None]],
}
_BOUNDARY_VALUES = [
    ("tenantId", "s"),
    ("tenantId", "😺"),
    ("tenantId", "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss"),
    ("tenantId", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"),
    ("priority", 0),
    ("priority", 1.0),
    ("cost", 0),
]

_CREATE_BODY = {"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "reason": "sample", "visitedOn": "2026-01-01", "cost": 1.5}
_CREATE_INVALID = invalid_bodies(
    _CREATE_BODY,
    ["rank", "reason", "visitedOn"],
    {"paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]},
    _REJECTED,
)
_CREATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _CREATE_BODY]

_UPDATE_BODY = {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "visitedOn": "2026-01-01", "cost": 1.5, "paid": False, "checkedAt": ["2026-01-15T10:00:00.000Z"]}
_UPDATE_INVALID = invalid_bodies(
    _UPDATE_BODY,
    [],
    {"region": "eu", "reason": "sample"},
    _REJECTED,
)
_UPDATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _UPDATE_BODY]


def describe_visit_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock(VisitService)

    @pytest.fixture
    def controller(mock_service):
        return VisitController(service=mock_service)

    def describe_get_by_id():
        def test_returns_item(controller, mock_service):
            mock_service.get_by_id.return_value = _response

            assert asyncio.run(controller.get_by_id(ITEM_ID)) == _response
            mock_service.get_by_id.assert_awaited_once_with(ITEM_ID)

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError) as error:
                asyncio.run(controller.get_by_id("not-a-uuid"))

            assert str(error.value) == "Visit with id not-a-uuid was not found."
            mock_service.get_by_id.assert_not_called()

    def describe_create():
        def test_passes_validated_body(controller, mock_service):
            mock_service.create.return_value = _response

            assert asyncio.run(controller.create(encode(_CREATE_BODY), "user-1")) == _response
            sent = mock_service.create.call_args.args[-2]
            assert isinstance(sent, VisitCreateRequest)
            assert sent.model_dump(exclude_unset=True) == _CREATE_BODY
            assert mock_service.create.call_args.args[-1] == "user-1"

        def test_fields_left_out_get_their_defaults(controller, mock_service):
            mock_service.create.return_value = _response
            minimal = {"rank": 1.5, "reason": "sample", "visitedOn": "2026-01-01"}

            asyncio.run(controller.create(encode(minimal)))
            sent = mock_service.create.call_args.args[-2].model_dump()
            assert sent["tenantId"] == "public"
            assert sent["region"] == "eu"
            assert sent["priority"] is None
            assert sent["labels"] == []
            assert sent["cost"] is None

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
            assert isinstance(sent, VisitUpdateRequest)
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
