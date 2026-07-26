import asyncio
import pytest
from unittest.mock import AsyncMock, MagicMock, patch

from models import (
    CatResponse,
    CatIdValidation,
    DogResponse,
    DogIdValidation,
)
from controllers import (
    CatController,
    DogController,
)
from services import (
    CatService,
    DogService,
)
from repositories.repository import DEFAULT_LIST_LIMIT


def describe_cat_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock(CatService)

    @pytest.fixture
    def controller(mock_service):
        return CatController(service=mock_service)

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
            mock_request = MagicMock()
            mock_request.path = '/cats/ac1df01c-7ece-4a20-ab60-179829dad8f5'
            expected_response = CatResponse(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mockName", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object(CatIdValidation, '__init__', return_value=None) as MockIdValidation:
                response = asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_request = MagicMock()
            mock_request.path = '/cats/mockInvalidId'
            with patch.object(CatIdValidation, '__init__', side_effect=ValueError("Invalid ID")) as MockIdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_request = MagicMock()
            mock_request.args = {}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_request = MagicMock()
            mock_request.args = {'limit': '5'}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(5)


def describe_dog_controller():
    @pytest.fixture
    def mock_service():
        return AsyncMock(DogService)

    @pytest.fixture
    def controller(mock_service):
        return DogController(service=mock_service)

    def describe_get_by_id():
        def test_ok_valid_uuid(controller, mock_service):
            mock_request = MagicMock()
            mock_request.path = '/dogs/ac1df01c-7ece-4a20-ab60-179829dad8f5'
            expected_response = DogResponse(id="ac1df01c-7ece-4a20-ab60-179829dad8f5", name="mockName", type="mockType")
            mock_service.get_by_id.return_value = expected_response

            with patch.object(DogIdValidation, '__init__', return_value=None) as MockIdValidation:
                response = asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='ac1df01c-7ece-4a20-ab60-179829dad8f5')
                mock_service.get_by_id.assert_called_once_with('ac1df01c-7ece-4a20-ab60-179829dad8f5')
                assert response == expected_response

        def test_value_error_invalid_uuid(controller, mock_service):
            mock_request = MagicMock()
            mock_request.path = '/dogs/mockInvalidId'
            with patch.object(DogIdValidation, '__init__', side_effect=ValueError("Invalid ID")) as MockIdValidation:
                with pytest.raises(ValueError):
                    asyncio.run(controller.get_by_id(mock_request))

                MockIdValidation.assert_called_once_with(id='mockInvalidId')
                mock_service.get_by_id.assert_not_called()

    def describe_get_list():
        def test_default_limit(controller, mock_service):
            mock_request = MagicMock()
            mock_request.args = {}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(DEFAULT_LIST_LIMIT)

        def test_honours_limit_query_param(controller, mock_service):
            mock_request = MagicMock()
            mock_request.args = {'limit': '5'}
            mock_service.get_list.return_value = []

            asyncio.run(controller.get_list(mock_request))
            mock_service.get_list.assert_called_once_with(5)

