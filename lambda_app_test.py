import asyncio
import base64
import json
import logging
from unittest.mock import AsyncMock, MagicMock, patch
import pytest
from lambda_app import lambda_handler
import blueprints
from blueprints import database
from controllers import (CatController, DogController)
from repositories import (CatRepository, DogRepository)
from utils.user_id import MAX_USER_ID_LENGTH, USER_ID_TOO_LONG_MESSAGE
from conftest import ITEM_ID, TIMESTAMPS

_BODY = '{"name": "mockName"}'
_ITEM = {"id": ITEM_ID, "name": "mockName", **TIMESTAMPS, "createdBy": "user-1", "updatedBy": "user-1"}
_USER_ID = "user-1"
_USER_HEADERS = {"X-User-Id": f"  {_USER_ID}  "}
_TOO_LONG_HEADERS = {"X-User-Id": "u" * (MAX_USER_ID_LENGTH + 1)}


def _event(method, resource, item_id=None, body=None, query=None, **extra):
    """API Gateway REST API, proxy integration."""
    return {
        "httpMethod": method,
        "resource": resource,
        "path": resource.replace("{id}", item_id or ""),
        "pathParameters": {"id": item_id} if item_id is not None else None,
        "queryStringParameters": query,
        "body": body,
        "isBase64Encoded": False,
        **extra,
    }


def _call(event):
    response = lambda_handler(event, None)
    return response["statusCode"], response["headers"]["Content-Type"], json.loads(response["body"])


def _offline_controller(module):
    settings = MagicMock(aws_region="us-east-1")
    settings.tables = {module.CONTAINER: "table"}
    with patch.object(module, "get_settings", return_value=settings):
        return module._controller.__wrapped__()


_ROUTES = [
    (blueprints.cat, "GET", "/cats", None,
     None, {"limit": "5"}, "get_list", ("5",), 200),
    (blueprints.cat, "GET", "/cats/{id}", ITEM_ID,
     None, None, "get_by_id", (ITEM_ID,), 200),
    (blueprints.cat, "POST", "/cats", None,
     _BODY, None, "create", (_BODY, _USER_ID,), 201),
    (blueprints.cat, "PATCH", "/cats/{id}", ITEM_ID,
     _BODY, None, "update", (ITEM_ID, _BODY, _USER_ID,), 200),
    (blueprints.cat, "DELETE", "/cats/{id}", ITEM_ID,
     None, None, "delete", (ITEM_ID, _USER_ID,), 200),
    (blueprints.dog, "GET", "/dogs", None,
     None, {"limit": "5"}, "get_list", ("5",), 200),
    (blueprints.dog, "GET", "/dogs/{id}", ITEM_ID,
     None, None, "get_by_id", (ITEM_ID,), 200),
    (blueprints.dog, "POST", "/dogs", None,
     _BODY, None, "create", (_BODY, _USER_ID,), 201),
    (blueprints.dog, "PUT", "/dogs/{id}", ITEM_ID,
     _BODY, None, "replace", (ITEM_ID, _BODY, _USER_ID,), 200),
    (blueprints.dog, "DELETE", "/dogs/{id}", ITEM_ID,
     None, None, "delete", (ITEM_ID, _USER_ID,), 200),
]

_WRITE_ROUTES = [route for route in _ROUTES if route[6] in ['create', 'update', 'replace', 'delete']]

# Only reachable if API Gateway forwards them.
_NOT_ALLOWED = [
    ("POST", "/health"),
    ("DELETE", "/cats"),
    ("POST", "/cats/{id}"),
    ("DELETE", "/dogs"),
    ("POST", "/dogs/{id}"),
]

_ITEM_ROUTES = [
    (blueprints.cat, "GET", "/cats/{id}", "Cat"),
    (blueprints.cat, "PATCH", "/cats/{id}", "Cat"),
    (blueprints.cat, "DELETE", "/cats/{id}", "Cat"),
    (blueprints.dog, "GET", "/dogs/{id}", "Dog"),
    (blueprints.dog, "PUT", "/dogs/{id}", "Dog"),
    (blueprints.dog, "DELETE", "/dogs/{id}", "Dog"),
]

_BODY_ROUTES = [
    (blueprints.cat, "POST", "/cats", None),
    (blueprints.cat, "PATCH", "/cats/{id}", ITEM_ID),
    (blueprints.dog, "POST", "/dogs", None),
    (blueprints.dog, "PUT", "/dogs/{id}", ITEM_ID),
]


def describe_lambda_handler():
    @pytest.mark.parametrize("module, method, resource, item_id, body, query, action, args, status", _ROUTES)
    def test_routes_to_controller(module, method, resource, item_id, body, query, action, args, status):
        controller = AsyncMock()
        payload = [_ITEM] if action == "get_list" else _ITEM
        getattr(controller, action).return_value = payload
        with patch.object(module, "_controller", return_value=controller):
            response = _call(_event(method, resource, item_id, body, query, headers=_USER_HEADERS))

        assert response == (status, "application/json", payload)
        getattr(controller, action).assert_awaited_once_with(*args)

    @pytest.mark.parametrize("module, method, resource, item_id, body, query, action, args, status", _WRITE_ROUTES)
    def test_too_long_user_id_is_rejected_before_the_database(module, method, resource, item_id, body, query, action, args, status):
        with patch.object(module, "_controller") as controller:
            response = _call(_event(method, resource, item_id, body, query, headers=_TOO_LONG_HEADERS))

        assert response == (400, "application/json", {"errorMessage": USER_ID_TOO_LONG_MESSAGE})
        controller.assert_not_called()

    def test_health():
        assert _call(_event("GET", "/health")) == (200, "application/json", {"status": "ok"})

    @pytest.mark.parametrize("resource", ["/", "/__unknown__", "/__unknown__/{id}"])
    def test_unknown_path_is_not_found(resource):
        assert _call(_event("GET", resource)) == (404, "application/json", {"errorMessage": "Not found."})

    def test_routes_by_path_without_resource():
        assert _call({"httpMethod": "GET", "path": "/__unknown__"})[0] == 404

    @pytest.mark.parametrize("method, resource", _NOT_ALLOWED)
    def test_disabled_method_is_not_allowed(method, resource):
        response = _call(_event(method, resource, ITEM_ID if "{id}" in resource else None))
        assert response == (405, "application/json", {"errorMessage": "Method not allowed."})

    @pytest.mark.parametrize("module, method, resource, name", _ITEM_ROUTES)
    def test_non_uuid_id_is_not_found(module, method, resource, name):
        with patch.object(module, "_controller", return_value=_offline_controller(module)):
            response = _call(_event(method, resource, "not-a-uuid", _BODY))

        assert response == (404, "application/json", {"errorMessage": f"{name} with id not-a-uuid was not found."})

    @pytest.mark.parametrize("module, method, resource, item_id", _BODY_ROUTES)
    @pytest.mark.parametrize("body", [
        "{not json",
        "[]",
        '{"name": 42}',
        '{"name": "mockName", "id": "' + ITEM_ID + '"}',
        '{"name": "mockName", "unknown": 1}',
        None,
    ])
    def test_invalid_body_is_bad_request(module, method, resource, item_id, body):
        with patch.object(module, "_controller", return_value=_offline_controller(module)):
            status, content_type, response = _call(_event(method, resource, item_id, body))

        assert (status, content_type) == (400, "application/json")
        assert set(response) == {"errorMessage"}

    @pytest.mark.parametrize("module, method, resource, item_id", _BODY_ROUTES)
    def test_base64_body_is_decoded(module, method, resource, item_id):
        controller = AsyncMock()
        encoded = base64.b64encode(_BODY.encode()).decode()
        with patch.object(module, "_controller", return_value=controller):
            _call(_event(method, resource, item_id, encoded, isBase64Encoded=True))

        (called,) = controller.method_calls
        assert called.args[-2] == _BODY.encode()

    def test_unexpected_error_is_logged_and_hidden(caplog):
        module, method, resource, item_id, body, query, *_ = _ROUTES[0]
        with patch.object(module, "_controller", side_effect=RuntimeError("secret detail")):
            with caplog.at_level(logging.ERROR):
                response = _call(_event(method, resource, item_id, body, query))

        assert response == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        assert "secret detail" in caplog.text


    def test_slow_database_ends_in_500_within_the_deadline(caplog):
        module, method, resource, item_id, body, query, action, *_ = _ROUTES[0]
        real_within_deadline = database.within_deadline

        async def hang(*args):
            await asyncio.sleep(60)

        controller = MagicMock()
        getattr(controller, action).side_effect = hang
        with patch.object(module, "_controller", return_value=controller), \
                patch.object(database, "within_deadline", lambda work: real_within_deadline(work, 0.05)):
            with caplog.at_level(logging.ERROR):
                response = _call(_event(method, resource, item_id, body, query))

        assert response == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        assert "did not answer within" in caplog.text


def describe_controllers():
    def test_builds_cat_controller_for_its_table():
        controller = _offline_controller(blueprints.cat)
        assert isinstance(controller, CatController)
        repository = controller.service.repository
        assert isinstance(repository, CatRepository)
        assert (repository.table_name, repository.region) == ("table", "us-east-1")

    def test_builds_dog_controller_for_its_table():
        controller = _offline_controller(blueprints.dog)
        assert isinstance(controller, DogController)
        repository = controller.service.repository
        assert isinstance(repository, DogRepository)
        assert (repository.table_name, repository.region) == ("table", "us-east-1")

