import pytest


def test_health_answers_ok(api, project):
    if not project.health_endpoint:
        pytest.skip("the project has no health check")
    response = api.http.get(f"/{project.health_endpoint}")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}
