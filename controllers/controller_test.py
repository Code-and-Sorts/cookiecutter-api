import asyncio
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
from azure.functions import HttpRequest
from models import (
    KittenClawsResponse,
    KittenClawsIdValidation,
)
from controllers import (
    KittenClawsController,
)
from services import (
    KittenClawsService,
)
from repositories.repository import DEFAULT_LIST_LIMIT


def describe_kitten_claws_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock(KittenClawsService)

    @pytest.fixture
    def controller(mock_service):
        return KittenClawsController(service=mock_service)

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'ac1df01c-7ece-4a20-ab60-179829dad8f5'}
            expected_response = KittenClawsResponse(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mockName", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object(KittenClawsIdValidation, '__init__', return_value=None) as MockIdValidation:
                response = asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.route_params = {'item_id': 'mockInvalidId'}
            with patch.object(KittenClawsIdValidation, '__init__', side_effect=ValueError("Invalid ID")) as MockIdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.params = {}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_request = MagicMock(spec=HttpRequest)
            mock_request.params = {'limit': '5'}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(5)

