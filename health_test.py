import json
import asyncio
import azure.functions as func
from blueprints import health


def describe_health():
    def test_endpoint_is_configured_segment():
        assert health.ENDPOINT == "health"

    def test_route_is_anonymous_get():
        (builder,) = health.bp._function_builders
        trigger = builder._function.get_trigger()
        assert trigger.route == "health"
        assert [str(m) for m in trigger.methods] == ["GET"]
        assert trigger.auth_level == func.AuthLevel.ANONYMOUS

    def test_returns_ok():
        fn = health.bp._function_builders[0]._function.get_user_function()
        req = func.HttpRequest(method="GET", url="/api/health", body=b"")
        response = asyncio.run(fn(req))
        assert response.status_code == 200
        assert response.mimetype == "application/json"
        assert json.loads(response.get_body()) == {"status": "ok"}
