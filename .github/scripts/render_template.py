import os
import subprocess
import tempfile
from pathlib import Path

from copier_config import REPOSITORY, default

FIXTURES = REPOSITORY / ".github" / "actions" / "setup-copier-template" / "fixtures"


def data_file(fixture: str, cloud: str, infrastructure: bool) -> list[str]:
    paths = []
    if fixture != "single":
        paths.append(FIXTURES / f"{fixture}-resources.yml")
    if infrastructure:
        paths.append(FIXTURES / f"{default('infra_clouds')[cloud]['slug']}-infra-environments.yml")
    if not paths:
        return []
    merged = Path(tempfile.mkdtemp()) / "copier-data.yml"
    merged.write_text("".join(path.read_text(encoding="utf-8") for path in paths), encoding="utf-8")
    return ["--data-file", str(merged)]


def main() -> None:
    env = os.environ
    infrastructure = env["INFRASTRUCTURE"] == "true"
    answers = {
        "project_name": env["PROJECT_NAME"],
        "project_description": env["PROJECT_DESCRIPTION"],
        "author": env["AUTHOR"],
        "language": env["LANGUAGE"],
        "cloud_service": env["CLOUD"],
        "open_source_license": env["LICENSE"],
    }
    if infrastructure:
        answers["include_infrastructure"] = "true"
    if env["PROJECT_ENDPOINT"]:
        answers["project_endpoint"] = env["PROJECT_ENDPOINT"]
    command = ["copier", "copy", "--defaults", "--trust", "--vcs-ref", "HEAD"]
    for name, value in answers.items():
        command += ["--data", f"{name}={value}"]
    command += data_file(env["FIXTURE"], env["CLOUD"], infrastructure)
    command += [str(REPOSITORY), env["OUTPUT"]]
    subprocess.run(command, check=True)


if __name__ == "__main__":
    main()
