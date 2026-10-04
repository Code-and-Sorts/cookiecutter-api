import asyncio
import json
import pytest
from unittest.mock import AsyncMock
from errors import NotFoundError, ValidationError
from models import BaseCat, CatResponse
from controllers import CatController
from services import CatService
from controllers.pagination import DEFAULT_LIST_LIMIT
from conftest import ITEM_ID, TIMESTAMPS, SERVER_FIELDS

_response = CatResponse(id=ITEM_ID, name="mockName", **TIMESTAMPS)


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

            assert asyncio.run(controller.create(b'{"name": "mockName"}', "user-1")) == _response
            mock_service.create.assert_awaited_once_with(BaseCat(name="mockName"), "user-1")

        @pytest.mark.parametrize("body", [
            b'{"name": 1}',
            b'{}',
            b'[]',
            b'{not json',
        ])
        def test_invalid_body_is_rejected(controller, mock_service, body):
            with pytest.raises(ValidationError):
                asyncio.run(controller.create(body))
            mock_service.create.assert_not_called()

    def describe_update():
        def test_passes_only_the_fields_sent(controller, mock_service):
            mock_service.update.return_value = _response

            assert asyncio.run(controller.update(ITEM_ID, b'{"name": "mockName"}', "user-1")) == _response
            item_id, changes, user_id = mock_service.update.call_args.args
            assert (item_id, user_id) == (ITEM_ID, "user-1")
            assert changes.model_dump(exclude_unset=True) == {"name": "mockName"}

        def test_empty_object_changes_nothing(controller, mock_service):
            mock_service.update.return_value = _response

            asyncio.run(controller.update(ITEM_ID, b'{}'))
            assert mock_service.update.call_args.args[1].model_dump(exclude_unset=True) == {}

        @pytest.mark.parametrize("body", [
            b'{"name": null}',
            b'{"name": ""}',
            b'"mockName"',
            b'',
        ])
        def test_invalid_body_is_rejected(controller, mock_service, body):
            with pytest.raises(ValidationError):
                asyncio.run(controller.update(ITEM_ID, body))
            mock_service.update.assert_not_called()

        def test_non_uuid_id_is_not_found(controller, mock_service):
            with pytest.raises(NotFoundError):
                asyncio.run(controller.update("not-a-uuid", b'{"name": "mockName"}'))
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

    @pytest.mark.parametrize("operation", ['create', 'update'])
    @pytest.mark.parametrize("field", [*SERVER_FIELDS, "unknown"])
    def test_body_rejects_server_and_unknown_fields(controller, mock_service, operation, field):
        body = json.dumps({"name": "mockName", field: "value"})
        args = (body,) if operation == "create" else (ITEM_ID, body)
        with pytest.raises(ValidationError, match=f"'{field}' is not an allowed field"):
            asyncio.run(getattr(controller, operation)(*args))
        getattr(mock_service, operation).assert_not_called()
