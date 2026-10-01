{% if cloud_service == 'Azure Function App' -%}
import azure.functions as func
{% endif -%}
{% if cloud_service == 'GCP Cloud Function' -%}
import flask
{% endif -%}
import pytest
from errors import ValidationError
from .user_id import MAX_USER_ID_LENGTH, USER_ID_TOO_LONG_MESSAGE, user_id_from
{%- if cloud_service == 'GCP Cloud Function' %}

_app = flask.Flask(__name__)
{%- endif %}


def _user_id(headers):
{%- if cloud_service == 'Azure Function App' %}
    return user_id_from(func.HttpRequest(method="POST", url="/api/items", body=b"", headers=headers))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    with _app.test_request_context("/items", method="POST", headers=headers):
        return user_id_from(flask.request)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    return user_id_from({"headers": headers})
{%- endif %}


def describe_user_id_from():
    @pytest.mark.parametrize("headers, expected", [
        ({"X-User-Id": "user-1"}, "user-1"),
        ({"x-user-id": "  user-1  "}, "user-1"),
        ({"X-USER-ID": "u" * MAX_USER_ID_LENGTH}, "u" * MAX_USER_ID_LENGTH),
        ({"X-User-Id": "   "}, None),
        ({}, None),
{%- if cloud_service == 'AWS Lambda' %}
        (None, None),
{%- endif %}
    ])
    def test_reads_trimmed_header(headers, expected):
        assert _user_id(headers) == expected

    def test_too_long_is_rejected():
        with pytest.raises(ValidationError, match=USER_ID_TOO_LONG_MESSAGE):
            _user_id({"X-User-Id": "u" * (MAX_USER_ID_LENGTH + 1)})
