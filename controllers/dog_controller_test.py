import asyncio
import pytest
from unittest.mock import AsyncMock
from errors import NotFoundError, ValidationError
from models import DogCreateRequest, DogReplaceRequest, DogResponse
from controllers import DogController
from services import DogService
from controllers.pagination import DEFAULT_LIST_LIMIT
from conftest import ITEM_ID, encode, invalid_bodies

_response = DogResponse(id=ITEM_ID, **{"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample"})
_REJECTED = {
    "tenantId": [42, None, "", "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "", "😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺😺"],
    "region": ["__invalid__", None, "EU"],
    "priority": ["1", -1, 1.5, 9007199254740992],
    "rank": ["1", None],
    "labels": ["not-a-list", None, ["item1", "item1"]],
    "name": [42, None, "", ""],
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
]

_CREATE_BODY = {"tenantId": "public", "region": "eu", "priority": 1, "rank": 1.5, "labels": [], "name": "sample"}
_CREATE_INVALID = invalid_bodies(
    _CREATE_BODY,
    ["rank", "name"],
    {},
    _REJECTED,
)
_CREATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _CREATE_BODY]

_REPLACE_BODY = {"tenantId": "public", "priority": 1, "rank": 1.5, "labels": [], "name": "sample"}
_REPLACE_INVALID = invalid_bodies(
    _REPLACE_BODY,
    ["rank", "name"],
    {"region": "eu"},
    _REJECTED,
)
_REPLACE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _REPLACE_BODY]


def describe_dog_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock(DogService)

    @pytest.fixture
    def controller(mock_service):
        return DogController(service=mock_service)

    def describe_get_by_id():
        def test_returns_item(controller, mock_service):
            mock_service.get_by_id.return_value = _response

            assert asyncio.run(controller.get_by_id(ITEM_ID)) == _response
            mock_service.get_by_id.assert_awaited_once_with(ITEM_ID)

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError) as error:
                asyncio.run(controller.get_by_id("not-a-uuid"))

            assert str(error.value) == "Dog with id not-a-uuid was not found."
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
            assert isinstance(sent, DogCreateRequest)
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

    def describe_replace():
        def test_passes_validated_body(controller, mock_service):
            mock_service.replace.return_value = _response

            assert asyncio.run(controller.replace(ITEM_ID, encode(_REPLACE_BODY), "user-1")) == _response
            sent = mock_service.replace.call_args.args[-2]
            assert isinstance(sent, DogReplaceRequest)
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

            assert result == {"message": f"Dog with id {ITEM_ID} was deleted successfully."}
            mock_service.soft_delete.assert_awaited_once_with(ITEM_ID, "user-1")

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError):
                asyncio.run(controller.delete("not-a-uuid"))
            mock_service.soft_delete.assert_not_called()
