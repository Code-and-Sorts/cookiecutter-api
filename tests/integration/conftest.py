from itertools import permutations
from pathlib import Path

import pytest

from api import OPERATIONS, Api, unique_name
from project import Project
from store import new_record, open_store

PROJECT = pytest.StashKey[Project]()


def pytest_addoption(parser):
    group = parser.getgroup("integration")
    group.addoption("--project-dir", type=Path, default=Path("KittenClaws"), help="Generated project to test.")
    group.addoption(
        "--base-url", required=True, help="Base URL including the route prefix, e.g. http://localhost:7071/api."
    )
    group.addoption("--ready-timeout", type=float, default=180, help="Seconds to wait for the API to answer.")


def pytest_configure(config):
    if not config.option.help:
        config.stash[PROJECT] = Project.load(config.option.project_dir)


def pytest_report_header(config):
    project = config.stash[PROJECT]
    names = ", ".join(r.name for r in project.resources)
    return f"project: {project.cloud}, resources: {names}, health: {project.health_endpoint or '(none)'}"


def _parametrize(metafunc, argnames: str, cases: list, reason: str):
    """Parametrizes over cases, or skips the test when the project has none."""
    if cases:
        ids = ["-".join(map(str, case)) if isinstance(case, tuple) else str(case) for case in cases]
        metafunc.parametrize(argnames, cases, ids=ids)
    else:
        metafunc.parametrize(
            argnames, [pytest.param(*[None] * len(argnames.split(",")), marks=pytest.mark.skip(reason=reason))]
        )


def pytest_generate_tests(metafunc):
    resources = metafunc.config.stash[PROJECT].resources
    marker = metafunc.definition.get_closest_marker("ops")
    needs = marker.args if marker else ()
    names = metafunc.fixturenames

    if "disabled_operation" in names:
        cases = [(r, op) for r in resources for op in OPERATIONS if op not in r.operations]
        _parametrize(metafunc, "resource,disabled_operation", cases, "every resource enables every operation")
    elif "operation" in names:
        each = metafunc.definition.get_closest_marker("each_operation").args
        cases = [(r, op) for r in resources if r.has(*needs) for op in each if r.has(op)]
        _parametrize(metafunc, "resource,operation", cases, f"no resource enables any of {', '.join(each)}")
    elif "resource" in names:
        cases = [r for r in resources if r.has(*needs)]
        _parametrize(metafunc, "resource", cases, f"no resource enables {', '.join(needs)}")
    for argname, same_container in (("reader", True), ("outsider", False)):
        if argname in names:
            cases = [
                (writer, other)
                for writer, other in permutations(resources, 2)
                if writer.has("create")
                and other.has(*needs)
                and (writer.container == other.container) == same_container
            ]
            _parametrize(metafunc, f"writer,{argname}", cases, "no pair of resources fits")


def _probe_path(project: Project) -> str | None:
    """A route that answers 200 once app code runs; it also starts a sam local container before the tests."""
    if project.health_endpoint:
        return f"/{project.health_endpoint}"
    lister = next((r for r in project.resources if r.has("list")), None)
    return f"/{lister.endpoint}" if lister else None


@pytest.fixture(scope="session")
def project(pytestconfig) -> Project:
    return pytestconfig.stash[PROJECT]


@pytest.fixture(scope="session")
def api(pytestconfig, project):
    api = Api(pytestconfig.option.base_url)
    if probe := _probe_path(project):
        api.wait_until_ready(probe, pytestconfig.option.ready_timeout)
    yield api
    api.http.close()


@pytest.fixture(scope="session")
def store(project):
    return open_store(project)


@pytest.fixture
def make_record(api, store):
    """Creates a live record through the API, or seeds the store when the resource cannot create."""

    def make(resource, *, user_id: str | None = None) -> dict:
        if resource.has("create"):
            response = api.send("create", resource, user_id=user_id)
            assert response.status_code == 201, response.text
            return response.json()
        record = new_record(unique_name(resource))
        store.put(resource.container, record)
        return {"id": record["id"], "name": record["name"]}

    return make
