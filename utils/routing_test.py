
import pytest
from errors import MethodNotAllowedError, NotFoundError
from .routing import resolve


def _list(*args):
    return "list"


def _get(*args):
    return "get"


def _health(*args):
    return "health"


def _delete(*args):
    return "delete"


_TABLE = {
    "health": {("GET", False): _health},
    "cats": {("GET", False): _list, ("GET", True): _get},
    "owls": {("DELETE", True): _delete},
}


def describe_resolve():
    @pytest.mark.parametrize("method, path, expected", [
        ("GET", "/cats", (_list, None)),
        ("get", "/cats/", (_list, None)),
        ("GET", "/cats/abc", (_get, "abc")),
        ("GET", "/health", (_health, None)),
        ("DELETE", "/owls/abc", (_delete, "abc")),
    ])
    def test_matches_route(method, path, expected):
        assert resolve(_TABLE, method, path) == expected

    @pytest.mark.parametrize("path", [
        "/", "", "/unknown", "/cats/abc/extra", "/health/abc", "/owls", "/api/cats", "/xcats",
    ])
    def test_unknown_path_is_not_found(path):
        with pytest.raises(NotFoundError) as error:
            resolve(_TABLE, "GET", path)
        assert str(error.value) == "Not found."

    @pytest.mark.parametrize("method, path", [
        ("POST", "/cats"), ("PUT", "/cats/abc"), ("PATCH", "/cats/abc"), ("DELETE", "/cats/abc"),
        ("POST", "/health"), ("GET", "/owls/abc"),
    ])
    def test_known_path_with_other_method_is_not_allowed(method, path):
        with pytest.raises(MethodNotAllowedError) as error:
            resolve(_TABLE, method, path)
        assert str(error.value) == "Method not allowed."
