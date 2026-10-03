"""The generated project under test, described by its Copier answers and emulator settings."""

from dataclasses import dataclass
from pathlib import Path
from typing import Any

import yaml

CLOUDS = {"Azure Function App": "azure", "GCP Cloud Function": "gcp", "AWS Lambda": "aws"}

# The pre-populated answers an older answers file may lack, as copier.yml and shared/_fields.jinja define them.
DEFAULT_BASE_MODEL = [
    {"name": "id", "type": "uuid", "required": True, "managed": "create", "default": "$uuid"},
    {"name": "isDeleted", "type": "boolean", "required": True, "managed": "create", "default": False, "hidden": True},
    {"name": "createdTimestamp", "type": "date-time", "required": True, "managed": "create", "default": "$now", "hidden": True},
    {"name": "updatedTimestamp", "type": "date-time", "required": True, "managed": "write", "default": "$now", "hidden": True},
    {"name": "createdBy", "type": "string", "managed": "create", "default": "$user", "hidden": True},
    {"name": "updatedBy", "type": "string", "managed": "write", "default": "$user", "hidden": True},
]
DEFAULT_FIELDS = [{"name": "name", "type": "string", "required": True, "rules": {"min_length": 1}}]
BODY_OPERATIONS = ("create", "replace", "update")


@dataclass(frozen=True)
class Field:
    name: str
    type: str
    item_type: str = ""
    enum_values: tuple[str, ...] = ()
    required: bool = False
    nullable: bool = False
    immutable: bool = False
    hidden: bool = False
    system: bool = False
    rules: tuple[tuple[str, Any], ...] = ()
    has_default: bool = False
    default: Any = None
    dynamic: str = ""
    example: Any = None
    accepted_by: frozenset[str] = frozenset()

    @classmethod
    def parse(cls, raw: dict, accepted_by: frozenset[str]) -> "Field":
        default = raw.get("default")
        dynamic = ""
        if isinstance(default, str) and default.startswith("$"):
            if default.startswith("$$"):
                default = default[1:]
            else:
                dynamic, default = default[1:], None
        return cls(
            name=raw["name"],
            type=raw["type"],
            item_type=raw.get("items", ""),
            enum_values=tuple(raw.get("values", ())),
            required=raw.get("required", False),
            nullable=raw.get("nullable", False),
            immutable=raw.get("immutable", False),
            hidden=raw.get("hidden", False),
            system="managed" in raw,
            rules=tuple(sorted((raw.get("rules") or {}).items())),
            has_default="default" in raw,
            default=default,
            dynamic=dynamic,
            example=raw.get("example"),
            accepted_by=accepted_by,
        )

    @property
    def rule(self) -> dict[str, Any]:
        return dict(self.rules)

    def accepted(self, operation: str) -> bool:
        return operation in self.accepted_by

    @property
    def read_default(self) -> Any:
        """What a response holds for a record without the field: its static default, or null."""
        return self.default if self.has_default and not self.dynamic and not self.nullable else None

    @property
    def needed_on(self) -> frozenset[str]:
        """The bodies that must send the field: it is required and has no default."""
        return self.accepted_by - {"update"} if self.required and not self.has_default else frozenset()


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
        base_model = answers.get("base_model") or DEFAULT_BASE_MODEL
        system = tuple(Field.parse(raw, frozenset()) for raw in base_model if "managed" in raw)
        base_fields = [raw for raw in base_model if "managed" not in raw]
        resources = tuple(_resource(r, base_fields) for r in answers["resources"])
        return cls(
            language=answers["language"],
            cloud=CLOUDS[answers["cloud_service"]],
            health_endpoint=answers.get("health_endpoint") or "",
            resources=resources,
            system_fields=system,
            env=_read_env(directory / ".env.emulator"),
        )


def _resource(r: dict, base_fields: list[dict]) -> Resource:
    requests = r.get("requests") or {}
    fields = [
        Field.parse(raw, frozenset(op for op in BODY_OPERATIONS if op == "create" or not raw.get("immutable")))
        for raw in base_fields
    ]
    for raw in r.get("fields", DEFAULT_FIELDS):
        accepted = frozenset(
            op
            for op in BODY_OPERATIONS
            if (raw["name"] in requests[op] if op in requests else op == "create" or not raw.get("immutable"))
        )
        fields.append(Field.parse(raw, accepted))
    # YAML loads a numeric container id as an int.
    return Resource(r["name"], r["endpoint"], str(r["container"]), frozenset(r["operations"]), tuple(fields))


def _read_env(path: Path) -> dict[str, str]:
    env = {}
    for line in path.read_text().splitlines():
        key, sep, value = line.partition("=")
        if sep and not key.lstrip().startswith("#"):
            env[key.strip()] = value.strip().strip('"')
    return env
