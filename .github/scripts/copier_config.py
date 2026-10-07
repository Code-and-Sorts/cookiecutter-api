import os
import subprocess
import sys
from pathlib import Path

try:
    import yaml
except ImportError:
    subprocess.run(
        [sys.executable, "-m", "pip", "install", "--quiet", "pyyaml"], check=True
    )
    import yaml

REPOSITORY = Path(__file__).resolve().parents[2]


def default(question: str):
    with (REPOSITORY / "copier.yml").open(encoding="utf-8") as copier:
        return yaml.safe_load(copier)[question]["default"]


def runtime_versions(cloud: str) -> dict:
    versions = dict(default("runtime_versions"))
    versions.update(versions.pop("clouds").get(cloud, {}))
    return versions


def write(variable: str, values: dict) -> None:
    with open(os.environ[variable], "a", encoding="utf-8") as file:
        file.writelines(f"{name}={value}\n" for name, value in values.items())
