from itertools import permutations
from pathlib import Path

import pytest

from api import OPERATIONS, Api
from project import BODY_OPERATIONS, Project
from store import new_record, open_store
from values import response_from_record

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
        ids = ["-".join(_id(part) for part in case) if isinstance(case, tuple) else str(case) for case in cases]
        metafunc.parametrize(argnames, cases, ids=ids)
    else:
        metafunc.parametrize(
            argnames, [pytest.param(*[None] * len(argnames.split(",")), marks=pytest.mark.skip(reason=reason))]
        )


def _id(part) -> str:
    """Resources and fields by name, values by their repr, shortened."""
    text = part if isinstance(part, str) else getattr(part, "name", None) or repr(part)
    return text if len(text) <= 40 else text[:37] + "..."


def _field_cases(kind: str, resources, needs) -> tuple[str, list]:
    """The (resource, operation, field[, value]) cases a field-level test runs, per the fields marker."""
    ops = [(r, op) for r in resources if r.has(*needs) for op in BODY_OPERATIONS if r.has(op)]
    if kind == "invalid":
        return "resource,operation,field,value", [
            (r, op, f, value) for r, op in ops for f in r.accepted(op) for value in f.rejected
        ]
    if kind == "required":
        return "resource,operation,field", [(r, op, f) for r, op in ops for f in r.accepted(op) if op in f.needed_on]
    if kind == "refused":
        return "resource,operation,field", [(r, op, f) for r, op in ops for f in r.fields if not f.accepted(op)]
    if kind == "nullable":
        return "resource,operation,field", [(r, op, f) for r, op in ops if op == "update" for f in r.accepted(op) if f.nullable]
    if kind == "date_time":
        return "resource,operation,field", [
            (r, op, f) for r, op in ops if op == "create" for f in r.accepted(op) if f.type == "date-time"
        ]
    if kind == "hidden":
        return "resource,field", [(r, f) for r in resources if r.has(*needs) for f in r.fields if f.hidden]
    raise ValueError(f"unknown fields marker {kind}")


def pytest_generate_tests(metafunc):
    resources = metafunc.config.stash[PROJECT].resources
    marker = metafunc.definition.get_closest_marker("ops")
    needs = marker.args if marker else ()
    names = metafunc.fixturenames
    fields_marker = metafunc.definition.get_closest_marker("fields")

    if fields_marker:
        argnames, cases = _field_cases(fields_marker.args[0], resources, needs)
        _parametrize(metafunc, argnames, cases, f"no field fits {fields_marker.args[0]}")
    elif "disabled_operation" in names:
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
    """Creates a live record through the API, or seeds the store when the resource cannot create; returns its response."""

    def make(resource, *, user_id: str | None = None) -> dict:
        if resource.has("create"):
            response = api.send("create", resource, user_id=user_id)
            assert response.status_code == 201, response.text
            return response.json()
        record = new_record(resource)
        store.put(resource.container, record)
        return response_from_record(resource, record)

    return make
