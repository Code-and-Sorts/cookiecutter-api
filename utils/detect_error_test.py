import json
import logging
import pytest
from errors import BaseError, MethodNotAllowedError, NotFoundError, ValidationError
from .detect_error import detect_error


def _parts(response):
    body, status, headers = response
    return status, headers["Content-Type"], json.loads(body)


def describe_detect_error():
    @pytest.mark.parametrize("error, status", [
        (ValidationError("name: Field required"), 400),
        (NotFoundError("Cat with id x was not found."), 404),
        (MethodNotAllowedError(), 405),
    ])
    def test_expected_errors_use_their_status_and_message(caplog, error, status):
        with caplog.at_level(logging.ERROR):
            response = detect_error(error)

        assert _parts(response) == (status, "application/json", {"errorMessage": str(error)})
        assert caplog.records == []

    def test_default_messages():
        assert str(NotFoundError()) == "Not found."
        assert str(MethodNotAllowedError()) == "Method not allowed."

    @pytest.mark.parametrize("error", [
        RuntimeError("secret connection string"),
        ModuleNotFoundError("No module named 'aiohttp'"),
        BaseError("internal detail"),
    ])
    def test_unexpected_errors_are_logged_and_hidden(caplog, error):
        try:
            raise error
        except Exception as raised:
            with caplog.at_level(logging.ERROR):
                response = detect_error(raised)

        assert _parts(response) == (500, "application/json", {"errorMessage": "An unexpected error occurred."})
        (record,) = caplog.records
        assert record.levelno == logging.ERROR
        assert record.exc_info[1] is error
        assert "Traceback" in caplog.text
