import json
from blueprints import health


def describe_health():
    def test_endpoint_is_configured_segment():
        assert health.ENDPOINT == "health"

    def test_returns_ok():
        status, body = health.health(None, None)
        assert status == 200
        assert json.dumps(body) == '{"status": "ok"}'

    def test_route_table_is_get_only():
        assert health.ROUTES == {("GET", False): health.health}
