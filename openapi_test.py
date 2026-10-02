import json
from pathlib import Path
import pytest
from blueprints import ROUTES
import models

_SPEC = json.loads(Path(__file__).with_name("openapi.json").read_text(encoding="utf-8"))

_REQUEST_MODELS = {
    "CatCreateRequest": models.BaseCat,
    "CatUpdateRequest": models.CatUpdate,
    "DogCreateRequest": models.BaseDog,
    "DogReplaceRequest": models.BaseDog,
}


def _spec_routes():
    return sorted(
        (method.upper(), path)
        for path, item in _SPEC["paths"].items()
        for method in item
        if method != "parameters"
    )


def _registered_routes():
    return sorted(
        (method, f"/{endpoint}/{{id}}" if has_id else f"/{endpoint}")
        for endpoint, routes in ROUTES.items()
        for method, has_id in routes
    )


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
