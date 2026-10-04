import asyncio
import json
import logging
from unittest.mock import AsyncMock, MagicMock, patch
import flask
import pytest
import main
import blueprints
from blueprints import database
from controllers import (CatController, DogController)
from utils.user_id import MAX_USER_ID_LENGTH, USER_ID_TOO_LONG_MESSAGE
from conftest import ITEM_ID, TIMESTAMPS

_BODY = b'{"name": "mockName"}'
_ITEM = {"id": ITEM_ID, "name": "mockName", **TIMESTAMPS, "createdBy": "user-1", "updatedBy": "user-1"}
_USER_ID = "user-1"
_USER_HEADERS = {"X-User-Id": f"  {_USER_ID}  "}
_TOO_LONG_HEADERS = {"X-User-Id": "u" * (MAX_USER_ID_LENGTH + 1)}
_app = flask.Flask(__name__)


def _call(method, path, data=None, headers=None):
    with _app.test_request_context(path, method=method, data=data, headers=headers):
        body, status, headers = main.api(flask.request)
    return status, headers["Content-Type"], json.loads(body)


def _run_with(controller):
    return lambda container_id, build_controller, operation: asyncio.run(operation(controller))


def _run_offline(container_id, build_controller, operation):
    return asyncio.run(operation(build_controller(MagicMock())))


_ROUTES = [
    (blueprints.cat, "GET", "/cats?limit=5", None,
     "get_list", ("5",), 200),
    (blueprints.cat, "GET", f"/cats/{ITEM_ID}", None,
     "get_by_id", (ITEM_ID,), 200),
    (blueprints.cat, "POST", "/cats", _BODY,
     "create", (_BODY, _USER_ID,), 201),
    (blueprints.cat, "PATCH", f"/cats/{ITEM_ID}", _BODY,
     "update", (ITEM_ID, _BODY, _USER_ID,), 200),
    (blueprints.cat, "DELETE", f"/cats/{ITEM_ID}", None,
     "delete", (ITEM_ID, _USER_ID,), 200),
    (blueprints.dog, "GET", "/dogs?limit=5", None,
     "get_list", ("5",), 200),
    (blueprints.dog, "GET", f"/dogs/{ITEM_ID}", None,
     "get_by_id", (ITEM_ID,), 200),
    (blueprints.dog, "POST", "/dogs", _BODY,
     "create", (_BODY, _USER_ID,), 201),
    (blueprints.dog, "PUT", f"/dogs/{ITEM_ID}", _BODY,
     "replace", (ITEM_ID, _BODY, _USER_ID,), 200),
    (blueprints.dog, "DELETE", f"/dogs/{ITEM_ID}", None,
     "delete", (ITEM_ID, _USER_ID,), 200),
]

_WRITE_ROUTES = [route for route in _ROUTES if route[4] in ['create', 'update', 'replace', 'delete']]

_NOT_ALLOWED = [
    ("POST", "/health"),
    ("DELETE", "/cats"),
    ("POST", f"/cats/{ITEM_ID}"),
    ("DELETE", "/dogs"),
    ("POST", f"/dogs/{ITEM_ID}"),
]

_ITEM_ROUTES = [
    (blueprints.cat, "GET", "/cats", "Cat"),
    (blueprints.cat, "PATCH", "/cats", "Cat"),
    (blueprints.cat, "DELETE", "/cats", "Cat"),
    (blueprints.dog, "GET", "/dogs", "Dog"),
    (blueprints.dog, "PUT", "/dogs", "Dog"),
    (blueprints.dog, "DELETE", "/dogs", "Dog"),
]

_BODY_ROUTES = [
    (blueprints.cat, "POST", "/cats"),
    (blueprints.cat, "PATCH", f"/cats/{ITEM_ID}"),
    (blueprints.dog, "POST", "/dogs"),
    (blueprints.dog, "PUT", f"/dogs/{ITEM_ID}"),
]


def describe_api():
    @pytest.mark.parametrize("module, method, path, data, action, args, status", _ROUTES)
    def test_routes_to_controller(module, method, path, data, action, args, status):
        controller = AsyncMock()
        payload = [_ITEM] if action == "get_list" else _ITEM
        getattr(controller, action).return_value = payload
        with patch.object(module, "run", side_effect=_run_with(controller)):
            response = _call(method, path, data, _USER_HEADERS)

        assert response == (status, "application/json", payload)
        getattr(controller, action).assert_awaited_once_with(*args)

    @pytest.mark.parametrize("module, method, path, data, action, args, status", _WRITE_ROUTES)
    def test_too_long_user_id_is_rejected_before_the_database(module, method, path, data, action, args, status):
        with patch.object(module, "run") as run:
            response = _call(method, path, data, _TOO_LONG_HEADERS)

        assert response == (400, "application/json", {"errorMessage": USER_ID_TOO_LONG_MESSAGE})
        run.assert_not_called()

    def test_health():
        assert _call("GET", "/health") == (200, "application/json", {"status": "ok"})

    def test_health_is_exact_path_match():
        assert _call("GET", "/health/x")[0] == 404
        assert _call("GET", "/xhealth")[0] == 404

    @pytest.mark.parametrize("path", ["/", "/__unknown__", f"/__unknown__/{ITEM_ID}", f"/cats/{ITEM_ID}/extra"])
    def test_unknown_path_is_not_found(path):
        assert _call("GET", path) == (404, "application/json", {"errorMessage": "Not found."})

    @pytest.mark.parametrize("method, path", _NOT_ALLOWED)
    def test_disabled_method_is_not_allowed(method, path):
        assert _call(method, path) == (405, "application/json", {"errorMessage": "Method not allowed."})

    @pytest.mark.parametrize("module, method, path, name", _ITEM_ROUTES)
    def test_non_uuid_id_is_not_found(module, method, path, name):
        with patch.object(module, "run", side_effect=_run_offline):
            response = _call(method, path + "/not-a-uuid", _BODY)

        assert response == (404, "application/json", {"errorMessage": f"{name} with id not-a-uuid was not found."})

    @pytest.mark.parametrize("module, method, path", _BODY_ROUTES)
    @pytest.mark.parametrize("data", [
        b"{not json",
        b"[]",
        b'{"name": 42}',
        b'{"name": "mockName", "id": "' + ITEM_ID.encode() + b'"}',
        b'{"name": "mockName", "unknown": 1}',
    ])
    def test_invalid_body_is_bad_request(module, method, path, data):
        with patch.object(module, "run", side_effect=_run_offline):
            status, content_type, body = _call(method, path, data)

        assert (status, content_type) == (400, "application/json")
        assert set(body) == {"errorMessage"}

    def test_unexpected_error_is_logged_and_hidden(caplog):
        module, method, path, data, *_ = _ROUTES[0]
        with patch.object(module, "run", side_effect=RuntimeError("secret detail")):
            with caplog.at_level(logging.ERROR):
                response = _call(method, path, data)

        assert response == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        assert "secret detail" in caplog.text


def describe_build_controller():
    def test_builds_cat_controller():
        assert isinstance(blueprints.cat._build_controller(MagicMock()), CatController)

    def test_builds_dog_controller():
        assert isinstance(blueprints.dog._build_controller(MagicMock()), DogController)


def describe_database_run():
    @pytest.fixture(autouse=True)
    def settings():
        settings = MagicMock(gcp_project_id="project", firestore_database="(default)")
        settings.collections = {"container": "collection-name"}
        with patch.object(database, "get_settings", return_value=settings), \
                patch.object(database, "_client", None):
            yield settings

    def test_reuses_one_client_on_one_event_loop():
        with patch.object(database.firestore, "AsyncClient") as client_class:
            async def operation(controller):
                return controller, asyncio.get_running_loop()

            first = database.run("container", lambda collection: ("controller", collection), operation)
            second = database.run("container", lambda collection: ("controller", collection), operation)

        client_class.assert_called_once_with(project="project", database="(default)")
        client_class.return_value.collection.assert_called_with("collection-name")
        assert first[0] == ("controller", client_class.return_value.collection.return_value)
        assert first[1] is second[1]
        assert first[1].is_running()

    def test_slow_database_times_out(caplog):
        real_within_deadline = database.within_deadline

        async def hang(controller):
            await asyncio.sleep(60)

        with patch.object(database.firestore, "AsyncClient"), \
                patch.object(database, "within_deadline", lambda work: real_within_deadline(work, 0.05)):
            with pytest.raises(TimeoutError, match="did not answer within"):
                database.run("container", lambda collection: collection, hang)

    def test_slow_database_ends_in_500(caplog):
        module, method, path, data, *_ = _ROUTES[0]
        with patch.object(module, "run", side_effect=TimeoutError("The database did not answer within 8 seconds.")):
            with caplog.at_level(logging.ERROR):
                response = _call(method, path, data)

        assert response == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        assert "did not answer within" in caplog.text

    def test_propagates_errors():
        async def operation(controller):
            raise RuntimeError("boom")

        with patch.object(database.firestore, "AsyncClient"):
            with pytest.raises(RuntimeError, match="boom"):
                database.run("container", lambda collection: collection, operation)
