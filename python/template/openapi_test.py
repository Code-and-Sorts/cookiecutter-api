{%- set request_ops = [('create', 'CreateRequest'), ('update', 'UpdateRequest'), ('replace', 'ReplaceRequest')] -%}
import json
from pathlib import Path
import pytest
{%- if cloud_service == 'Azure Function App' %}
import asyncio
import azure.functions as func
from function_app import app
{%- else %}
from blueprints import ROUTES
{%- endif %}
from blueprints import openapi
import models

_SPEC = json.loads(Path(__file__).with_name("openapi.json").read_text(encoding="utf-8"))

# Request schema in the spec -> the Pydantic model that validates that body.
_REQUEST_MODELS = {
{%- for resource in resources %}
{%- for op, suffix in request_ops if op in resource.operations %}
    "{{ resource.name }}{{ suffix }}": models.{% if op == 'update' %}{{ resource.name }}Update{% else %}Base{{ resource.name }}{% endif %},
{%- endfor %}
{%- endfor %}
}


def _spec_routes():
    return sorted(
        (method.upper(), path)
        for path, item in _SPEC["paths"].items()
        for method in item
        if method != "parameters"
    )


def _registered_routes():
{%- if cloud_service == 'Azure Function App' %}
    # app.get_functions() may only run once per app, and function_app_test already calls it.
    triggers = [builder._function.get_trigger() for builder in app._function_builders]
    return sorted((str(method), "/" + trigger.route) for trigger in triggers for method in trigger.methods)
{%- else %}
    return sorted(
        (method, f"/{endpoint}/{% raw %}{{id}}{% endraw %}" if has_id else f"/{endpoint}")
        for endpoint, routes in ROUTES.items()
        for method, has_id in routes
    )
{%- endif %}


def _json_schema(model):
    schema = model.model_json_schema()
    del schema["title"]
    for field in schema["properties"].values():
        field.pop("title", None)
        field.pop("default", None)
    return schema


def describe_openapi_route():
{%- if cloud_service == 'Azure Function App' %}
    def test_is_an_anonymous_get():
        (builder,) = openapi.bp._function_builders
        trigger = builder._function.get_trigger()
        assert trigger.route == "openapi.json"
        assert [str(m) for m in trigger.methods] == ["GET"]
        assert trigger.auth_level == func.AuthLevel.ANONYMOUS

    def test_serves_the_spec():
        fn = openapi.bp._function_builders[0]._function.get_user_function()
        response = asyncio.run(fn(func.HttpRequest(method="GET", url="/api/openapi.json", body=b"")))
        assert (response.status_code, response.mimetype) == (200, "application/json")
        assert json.loads(response.get_body()) == _SPEC
{%- else %}
    def test_serves_the_spec():
        assert openapi.openapi(None, None) == (200, _SPEC)

    def test_route_table_is_get_only():
        assert openapi.ROUTES == {("GET", False): openapi.openapi}
{%- endif %}


def describe_openapi_spec():
    def test_lists_exactly_the_registered_routes():
        assert _spec_routes() == _registered_routes()

    def test_has_a_request_schema_per_request_model():
        assert sorted(name for name in _SPEC["components"]["schemas"] if name.endswith("Request")) == sorted(_REQUEST_MODELS)

    @pytest.mark.parametrize("name, model", list(_REQUEST_MODELS.items()))
    def test_request_schema_matches_model(name, model):
        assert _SPEC["components"]["schemas"][name] == _json_schema(model)
