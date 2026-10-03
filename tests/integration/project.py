"""The generated project under test: its Copier answers, the field data shared/_fields.jinja derives from them, and
its emulator settings."""

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Any

import jinja2
import yaml

CLOUDS = {"Azure Function App": "azure", "GCP Cloud Function": "gcp", "AWS Lambda": "aws"}
BODY_OPERATIONS = ("create", "replace", "update")

# The templates render every model and unit test from these macros, so the suite reads the same field data.
_TEMPLATE_ROOT = Path(__file__).resolve().parents[2]
_JINJA = jinja2.Environment(
    loader=jinja2.FileSystemLoader(_TEMPLATE_ROOT),
    extensions=["jinja2_ansible_filters.AnsibleCoreFiltersExtension", "jinja2_strcase.StrcaseExtension"],
)
FIELDS = _JINJA.get_template("shared/_fields.jinja").module


@dataclass(frozen=True, eq=False)
class Field:
    """A field as shared/_fields.jinja normalizes it, with the answer it came from."""

    name: str
    type: str
    item_type: str
    enum_values: tuple[str, ...]
    nullable: bool
    hidden: bool
    system: bool
    dynamic: str
    default: Any
    read_default: Any
    needs_value: bool
    sample: Any
    rejected: tuple[Any, ...]
    boundaries: tuple[Any, ...]
    accepted_by: frozenset[str]
    answer: dict

    @classmethod
    def from_data(cls, data: dict, answer: dict) -> "Field":
        return cls(
            name=data["name"],
            type=data["type"],
            item_type=data["item_type"],
            enum_values=tuple(data["enum_values"]),
            nullable=data["nullable"],
            hidden=data["hidden"],
            system=data["system"],
            dynamic=data["dynamic"],
            default=data["default"],
            read_default=data["read_default"],
            needs_value=data["needs_value"],
            sample=data["sample"],
            rejected=tuple(data["rejected"]),
            boundaries=tuple(data["boundaries"]),
            accepted_by=frozenset(op for op in BODY_OPERATIONS if data[f"in_{op}"]),
            answer=answer,
        )

    def accepted(self, operation: str) -> bool:
        return operation in self.accepted_by

    def is_valid(self, value: Any) -> bool:
        return not str(FIELDS.value_error(self.answer, value)).strip()

    @property
    def needed_on(self) -> frozenset[str]:
        """The bodies that must send the field: it is required and has no default."""
        return self.accepted_by - {"update"} if self.needs_value else frozenset()


@dataclass(frozen=True)
class Resource:
    name: str
    endpoint: str
    container: str
    operations: frozenset[str]
    # Every field a client may set: the base_model fields it adds, then the resource's own.
    fields: tuple[Field, ...] = ()

    def has(self, *operations: str) -> bool:
        return set(operations) <= self.operations

    def accepted(self, operation: str) -> tuple[Field, ...]:
        return tuple(f for f in self.fields if f.accepted(operation)) if operation in self.operations else ()

    @property
    def shown(self) -> tuple[Field, ...]:
        return tuple(f for f in self.fields if not f.hidden)

    def __str__(self) -> str:
        return self.name


@dataclass(frozen=True)
class Project:
    language: str
    cloud: str
    health_endpoint: str
    resources: tuple[Resource, ...]
    system_fields: tuple[Field, ...]
    env: dict[str, str]

    @classmethod
    def load(cls, directory: Path) -> "Project":
        answers = yaml.safe_load((directory / ".copier-answers.yml").read_text())
        base_model = answers.get("base_model") or FIELDS.LOCKED_FIELDS
        base = _fields(FIELDS.base_fields_json(base_model), base_model)
        client_base = tuple(f for f in base if not f.system)
        return cls(
            language=answers["language"],
            cloud=CLOUDS[answers["cloud_service"]],
            health_endpoint=answers.get("health_endpoint") or "",
            resources=tuple(_resource(r, client_base) for r in answers["resources"]),
            system_fields=tuple(f for f in base if f.system),
            env=_read_env(directory / ".env.emulator"),
        )


def _fields(rendered: str, answers: list[dict]) -> tuple[Field, ...]:
    return tuple(Field.from_data(data, answer) for data, answer in zip(json.loads(str(rendered)), answers, strict=True))


def _resource(r: dict, client_base: tuple[Field, ...]) -> Resource:
    own = _fields(FIELDS.resource_fields_json(r), r.get("fields", FIELDS.DEFAULT_FIELDS))
    # YAML loads a numeric container id as an int.
    return Resource(r["name"], r["endpoint"], str(r["container"]), frozenset(r["operations"]), client_base + own)


def _read_env(path: Path) -> dict[str, str]:
    env = {}
    for line in path.read_text().splitlines():
        key, sep, value = line.partition("=")
        if sep and not key.lstrip().startswith("#"):
            env[key.strip()] = value.strip().strip('"')
    return env
