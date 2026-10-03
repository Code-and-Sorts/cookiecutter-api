"""The generated project under test, described by its Copier answers and emulator settings."""

from dataclasses import dataclass
from pathlib import Path

import yaml

CLOUDS = {"Azure Function App": "azure", "GCP Cloud Function": "gcp", "AWS Lambda": "aws"}


@dataclass(frozen=True)
class Resource:
    name: str
    endpoint: str
    container: str
    operations: frozenset[str]

    def has(self, *operations: str) -> bool:
        return set(operations) <= self.operations

    def __str__(self) -> str:
        return self.name


@dataclass(frozen=True)
class Project:
    language: str
    cloud: str
    health_endpoint: str
    resources: tuple[Resource, ...]
    env: dict[str, str]

    @classmethod
    def load(cls, directory: Path) -> "Project":
        answers = yaml.safe_load((directory / ".copier-answers.yml").read_text())
        resources = tuple(
            # YAML loads a numeric container id as an int.
            Resource(r["name"], r["endpoint"], str(r["container"]), frozenset(r["operations"]))
            for r in answers["resources"]
        )
        return cls(
            language=answers["language"],
            cloud=CLOUDS[answers["cloud_service"]],
            health_endpoint=answers.get("health_endpoint") or "",
            resources=resources,
            env=_read_env(directory / ".env.emulator"),
        )


def _read_env(path: Path) -> dict[str, str]:
    env = {}
    for line in path.read_text().splitlines():
        key, sep, value = line.partition("=")
        if sep and not key.lstrip().startswith("#"):
            env[key.strip()] = value.strip().strip('"')
    return env
