from uuid import uuid4

# API Gateway answers routes it does not know with 403 before the function runs.
NOT_ROUTED = {"azure": {404, 405}, "gcp": {404, 405}, "aws": {403, 404, 405}}


def test_a_disabled_operation_is_refused(api, project, resource, disabled_operation):
    response = api.send(disabled_operation, resource)
    assert response.status_code in NOT_ROUTED[project.cloud], response.text


def test_an_unknown_path_is_not_found(api, project):
    response = api.http.get(f"/no-such-route-{uuid4().hex[:8]}")
    assert response.status_code in NOT_ROUTED[project.cloud] - {405}, response.text
