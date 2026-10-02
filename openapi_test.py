import json
from pathlib import Path
import pytest
from function_app import app
import models

_SPEC = json.loads(Path(__file__).with_name("openapi.json").read_text(encoding="utf-8"))

_REQUEST_MODELS = {
    "KittenClawsCreateRequest": models.BaseKittenClaws,
    "KittenClawsUpdateRequest": models.KittenClawsUpdate,
}


def _spec_routes():
    return sorted(
        (method.upper(), path)
        for path, item in _SPEC["paths"].items()
        for method in item
        if method != "parameters"
    )


def _registered_routes():
    # app.get_functions() may only run once per app, and function_app_test already calls it.
    triggers = [builder._function.get_trigger() for builder in app._function_builders]
    return sorted((str(method), "/" + trigger.route) for trigger in triggers for method in trigger.methods)


def _json_schema(model):
    schema = model.model_json_schema()
    del schema["title"]
    for field in schema["properties"].values():
        field.pop("title", None)
        field.pop("default", None)
    return schema


def describe_openapi_spec():
    def test_lists_exactly_the_registered_routes():
        assert _spec_routes() == _registered_routes()

    def test_has_a_request_schema_per_request_model():
        assert sorted(name for name in _SPEC["components"]["schemas"] if name.endswith("Request")) == sorted(_REQUEST_MODELS)

    @pytest.mark.parametrize("name, model", list(_REQUEST_MODELS.items()))
    def test_request_schema_matches_model(name, model):
        assert _SPEC["components"]["schemas"][name] == _json_schema(model)
