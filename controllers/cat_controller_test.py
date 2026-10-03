import asyncio
import pytest
from unittest.mock import AsyncMock
from errors import NotFoundError, ValidationError
from models import CatCreateRequest, CatUpdateRequest, CatResponse
from controllers import CatController
from services import CatService
from controllers.pagination import DEFAULT_LIST_LIMIT
from conftest import ITEM_ID, encode, invalid_bodies

_response = CatResponse(id=ITEM_ID, **{"name": "sample"})
_REJECTED = {
    "name": [42, None, "", ""],
}
_BOUNDARY_VALUES = [
    ("name", "s"),
    ("name", "😺"),
]

_CREATE_BODY = {"name": "sample"}
_CREATE_INVALID = invalid_bodies(
    _CREATE_BODY,
    ["name"],
    {},
    _REJECTED,
)
_CREATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _CREATE_BODY]

_UPDATE_BODY = {"name": "sample"}
_UPDATE_INVALID = invalid_bodies(
    _UPDATE_BODY,
    [],
    {},
    _REJECTED,
)
_UPDATE_BOUNDARIES = [(name, value) for name, value in _BOUNDARY_VALUES if name in _UPDATE_BODY]


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

    def describe_delete():
        def test_returns_confirmation_message(controller, mock_service):
            result = asyncio.run(controller.delete(ITEM_ID, "user-1"))

            assert result == {"message": f"Cat with id {ITEM_ID} was deleted successfully."}
            mock_service.soft_delete.assert_awaited_once_with(ITEM_ID, "user-1")

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError):
                asyncio.run(controller.delete("not-a-uuid"))
            mock_service.soft_delete.assert_not_called()
