import asyncio
import json
import logging
from unittest.mock import AsyncMock, MagicMock, patch
import azure.functions as func
import pytest
from function_app import app
from blueprints import database
import blueprints
from controllers import (CatController, DogController)
from utils.user_id import MAX_USER_ID_LENGTH, USER_ID_TOO_LONG_MESSAGE
from conftest import ITEM_ID

# Routing tests pass the body through a mocked controller, so its content does not matter.
_BODY = b'{"any": "body"}'
_ITEM = {"id": ITEM_ID}
_USER_ID = "user-1"
_USER_HEADERS = {"X-User-Id": f"  {_USER_ID}  "}
_TOO_LONG_HEADERS = {"X-User-Id": "u" * (MAX_USER_ID_LENGTH + 1)}
_FUNCTIONS = {function.get_function_name(): function for function in app.get_functions()}


def _request(method, route, item_id=None, body=b"", params=None, headers=None):
    return func.HttpRequest(
        method=method,
        url="/api/" + route.replace("{id}", item_id or ""),
        body=body,
        headers=headers,
        params=params or {},
        route_params={"id": item_id} if item_id is not None else {},
    )


def _call(function_name, request):
    response = asyncio.run(_FUNCTIONS[function_name].get_user_function()(request))
    return response.status_code, response.mimetype, json.loads(response.get_body())


_ROUTES = [
    (blueprints.cat, "get_list_cat", "GET", "cats", None,
     b"", {"limit": "5"}, "get_list", ("5",), 200),
    (blueprints.cat, "get_by_id_cat", "GET", "cats/{id}", ITEM_ID,
     b"", None, "get_by_id", (ITEM_ID,), 200),
    (blueprints.cat, "create_cat", "POST", "cats", None,
     _BODY, None, "create", (_BODY, _USER_ID,), 201),
    (blueprints.cat, "update_cat", "PATCH", "cats/{id}", ITEM_ID,
     _BODY, None, "update", (ITEM_ID, _BODY, _USER_ID,), 200),
    (blueprints.cat, "delete_cat", "DELETE", "cats/{id}", ITEM_ID,
     b"", None, "delete", (ITEM_ID, _USER_ID,), 200),
    (blueprints.dog, "get_list_dog", "GET", "dogs", None,
     b"", {"limit": "5"}, "get_list", ("5",), 200),
    (blueprints.dog, "get_by_id_dog", "GET", "dogs/{id}", ITEM_ID,
     b"", None, "get_by_id", (ITEM_ID,), 200),
    (blueprints.dog, "create_dog", "POST", "dogs", None,
     _BODY, None, "create", (_BODY, _USER_ID,), 201),
    (blueprints.dog, "replace_dog", "PUT", "dogs/{id}", ITEM_ID,
     _BODY, None, "replace", (ITEM_ID, _BODY, _USER_ID,), 200),
    (blueprints.dog, "delete_dog", "DELETE", "dogs/{id}", ITEM_ID,
     b"", None, "delete", (ITEM_ID, _USER_ID,), 200),
]
_ITEM_ROUTES = [route for route in _ROUTES if route[4] is not None]
_BODY_ROUTES = [route for route in _ROUTES if route[5]]
_WRITE_ROUTES = [route for route in _ROUTES if route[7] in ['create', 'update', 'replace', 'delete']]


def describe_function_app():
    @pytest.mark.parametrize("module, name, method, route, item_id, body, query, action, args, status", _ROUTES)
    def test_registers_function_level_route(module, name, method, route, item_id, body, query, action, args, status):
        trigger = _FUNCTIONS[name].get_trigger()
        assert trigger.route == route
        assert [str(m) for m in trigger.methods] == [method]
        assert trigger.auth_level == func.AuthLevel.FUNCTION

    def test_health_is_anonymous():
        trigger = _FUNCTIONS["health"].get_trigger()
        assert trigger.route == "health"
        assert trigger.auth_level == func.AuthLevel.ANONYMOUS

    def test_registers_only_the_enabled_operations():
        assert set(_FUNCTIONS) == {route[1] for route in _ROUTES} | {"health"}

    @pytest.mark.parametrize("module, name, method, route, item_id, body, query, action, args, status", _ROUTES)
    def test_routes_to_controller(module, name, method, route, item_id, body, query, action, args, status):
        controller = AsyncMock()
        payload = [_ITEM] if action == "get_list" else _ITEM
        getattr(controller, action).return_value = payload
        with patch.object(database, "get_container", AsyncMock()), \
                patch.object(module, "_build_controller", return_value=controller):
            response = _call(name, _request(method, route, item_id, body, query, _USER_HEADERS))

        assert response == (status, "application/json", payload)
        getattr(controller, action).assert_awaited_once_with(*args)

    @pytest.mark.parametrize("module, name, method, route, item_id, body, query, action, args, status", _WRITE_ROUTES)
    def test_too_long_user_id_is_rejected_before_the_database(module, name, method, route, item_id, body, query, action, args, status):
        get_container = AsyncMock()
        with patch.object(database, "get_container", get_container):
            response = _call(name, _request(method, route, item_id, body, query, _TOO_LONG_HEADERS))

        assert response == (400, "application/json", {"errorMessage": USER_ID_TOO_LONG_MESSAGE})
        get_container.assert_not_called()

    @pytest.mark.parametrize("module, name, method, route, item_id, body, query, action, args, status", _ITEM_ROUTES)
    def test_non_uuid_id_is_not_found(module, name, method, route, item_id, body, query, action, args, status):
        with patch.object(database, "get_container", AsyncMock(return_value=MagicMock())):
            response = _call(name, _request(method, route, "not-a-uuid", _BODY))

        assert response[:2] == (404, "application/json")
        assert response[2]["errorMessage"].endswith(" with id not-a-uuid was not found.")

    @pytest.mark.parametrize("module, name, method, route, item_id, body, query, action, args, status", _BODY_ROUTES)
    @pytest.mark.parametrize("data", [
        b"{not json",
        b"[]",
        b'{"id": "' + ITEM_ID.encode() + b'"}',
        b'{"not_a_field": 1}',
        b"",
    ])
    def test_invalid_body_is_bad_request(module, name, method, route, item_id, body, query, action, args, status, data):
        with patch.object(database, "get_container", AsyncMock(return_value=MagicMock())):
            status_code, content_type, response = _call(name, _request(method, route, item_id, data))

        assert (status_code, content_type) == (400, "application/json")
        assert set(response) == {"errorMessage"}

    def test_unexpected_error_is_logged_and_hidden(caplog):
        _, name, method, route, item_id, body, query, *_ = _ROUTES[0]
        with patch.object(database, "get_container", AsyncMock(side_effect=ModuleNotFoundError("No module named 'aiohttp'"))):
            with caplog.at_level(logging.ERROR):
                response = _call(name, _request(method, route, item_id, body, query))

        assert response == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        assert "aiohttp" in caplog.text


    def test_slow_database_ends_in_500_within_the_deadline(caplog):
        _, name, method, route, item_id, body, query, *_ = _ROUTES[0]
        real_within_deadline = database.within_deadline

        async def hang(container_id):
            await asyncio.sleep(60)

        with patch.object(database, "get_container", side_effect=hang), \
                patch.object(database, "within_deadline", lambda work: real_within_deadline(work, 0.05)):
            with caplog.at_level(logging.ERROR):
                response = _call(name, _request(method, route, item_id, body, query))

        assert response == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        assert "did not answer within" in caplog.text

    def test_cancelled_request_is_not_logged_as_error(caplog):
        _, name, method, route, item_id, body, query, *_ = _ROUTES[0]
        with patch.object(database, "get_container", AsyncMock(side_effect=asyncio.CancelledError())):
            with caplog.at_level(logging.ERROR):
                with pytest.raises(asyncio.CancelledError):
                    _call(name, _request(method, route, item_id, body, query))

        assert caplog.records == []

    def test_client_bounds_each_call():
        assert database.CLIENT_OPTIONS["timeout"] <= 5
        assert database.CLIENT_OPTIONS["retry_total"] <= 2


def describe_build_controller():
    def test_builds_cat_controller():
        assert isinstance(blueprints.cat._build_controller(MagicMock()), CatController)

    def test_builds_dog_controller():
        assert isinstance(blueprints.dog._build_controller(MagicMock()), DogController)


def describe_get_container():
    @pytest.fixture(autouse=True)
    def settings():
        settings = MagicMock(cosmos_db_uri="https://cosmos", cosmos_db_key="key", cosmos_db_database_name="db", cosmos_db_emulator=False)
        settings.container_names = {"container": "container-name"}
        with patch.object(database, "get_settings", return_value=settings), \
                patch.object(database, "_client", None):
            yield settings

    def test_opens_one_client_and_reuses_it():
        with patch.object(database, "CosmosClient") as client_class:
            client_class.return_value.__aenter__ = AsyncMock()

            async def get_twice():
                return await database.get_container("container"), await database.get_container("container")

            first, second = asyncio.run(get_twice())

        client_class.assert_called_once_with("https://cosmos", "key", **database.CLIENT_OPTIONS)
        client_class.return_value.__aenter__.assert_awaited_once()
        client = client_class.return_value
        client.get_database_client.assert_called_with("db")
        client.get_database_client.return_value.get_container_client.assert_called_with("container-name")
        assert first is second


def describe_cosmos_client_options():
    def _options(uri, emulator):
        return database.cosmos_client_options(MagicMock(cosmos_db_uri=uri, cosmos_db_emulator=emulator))

    def test_leaves_the_client_unchanged_without_the_emulator_flag():
        assert _options("https://account.documents.azure.com:443/", False) == database.CLIENT_OPTIONS
        assert _options("http://localhost:8081/", False) == database.CLIENT_OPTIONS

    def test_emulator_keeps_the_configured_endpoint():
        result = _options("http://localhost:8081/", True)
        assert result["enable_endpoint_discovery"] is False
        assert "connection_verify" not in result
        assert result["retry_total"] == database.CLIENT_OPTIONS["retry_total"]

    def test_emulator_over_https_skips_certificate_checks():
        assert _options("https://localhost:8081/", True)["connection_verify"] is False
