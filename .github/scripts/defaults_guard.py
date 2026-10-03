"""Asserts that answers without base_model or fields render exactly like answers that spell out their defaults.

For each language, cloud and fixture it renders twice: once leaving out every default fields list and the
default base_model (as an older .copier-answers.yml does), once writing them out, and diffs the two projects.
"""

import argparse
import subprocess
import sys
import tempfile
from concurrent.futures import ThreadPoolExecutor
from itertools import product
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parents[2]
FIXTURES = ROOT / ".github/actions/setup-copier-template/fixtures"
LANGUAGES = ["python", "typescript", "dotnet", "go"]
CLOUDS = ["Azure Function App", "GCP Cloud Function", "AWS Lambda"]
FIXTURE_NAMES = ["single"] + sorted(p.name.removesuffix("-resources.yml") for p in FIXTURES.glob("*-resources.yml"))
ANSWERS = ".copier-answers.yml"


def copier(destination: Path, language: str, cloud: str, data_file: Path | None) -> None:
    command = ["copier", "copy", "--defaults", "--trust", "--vcs-ref", "HEAD", "--quiet",
               "--data", "project_name=KittenClaws", "--data", f"language={language}", "--data", f"cloud_service={cloud}"]
    if data_file:
        command += ["--data-file", str(data_file)]
    subprocess.run(command + [str(ROOT), str(destination)], check=True, capture_output=True, cwd=ROOT)


def answers_of(work: Path, language: str, cloud: str, fixture: str) -> dict:
    """The fixture's answers; for single, the defaults a render records."""
    if fixture != "single":
        return yaml.safe_load((FIXTURES / f"{fixture}-resources.yml").read_text())
    copier(work / "defaults", language, cloud, None)
    recorded = yaml.safe_load((work / "defaults" / ANSWERS).read_text())
    return {"resources": recorded["resources"], "base_model": recorded["base_model"]}


def variants(answers: dict, default_fields: list, default_base: list) -> tuple[dict, dict]:
    """The answers without the default fields and base_model, and with both written out."""
    implicit = {**answers, "resources": [
        {key: value for key, value in r.items() if not (key == "fields" and value == default_fields)}
        for r in answers["resources"]
    ]}
    if implicit.get("base_model") == default_base:
        del implicit["base_model"]
    explicit = {**answers, "base_model": answers.get("base_model", default_base),
                "resources": [{"fields": default_fields, **r} for r in answers["resources"]]}
    return implicit, explicit


def check(case: tuple[str, str, str], defaults: tuple[list, list]) -> str | None:
    """Returns the diff when the two renders differ, or None."""
    language, cloud, fixture = case
    with tempfile.TemporaryDirectory() as tmp:
        work = Path(tmp)
        for name, data in zip(("implicit", "explicit"), variants(answers_of(work, language, cloud, fixture), *defaults)):
            data_file = work / f"{name}.yml"
            data_file.write_text(yaml.safe_dump(data))
            copier(work / name, language, cloud, data_file)
        diff = subprocess.run(["diff", "-r", "-x", ANSWERS, str(work / "implicit"), str(work / "explicit")],
                              capture_output=True, text=True)
    return diff.stdout or None


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--language", action="append", choices=LANGUAGES)
    parser.add_argument("--fixture", action="append", choices=FIXTURE_NAMES)
    args = parser.parse_args()
    with tempfile.TemporaryDirectory() as tmp:
        copier(Path(tmp), "python", CLOUDS[0], None)
        recorded = yaml.safe_load((Path(tmp) / ANSWERS).read_text())
    defaults = (recorded["resources"][0]["fields"], recorded["base_model"])
    cases = list(product(args.language or LANGUAGES, CLOUDS, args.fixture or FIXTURE_NAMES))
    with ThreadPoolExecutor(max_workers=8) as pool:
        failures = [(case, diff) for case, diff in zip(cases, pool.map(lambda c: check(c, defaults), cases)) if diff]
    for (language, cloud, fixture), diff in failures:
        print(f"FAIL {language} / {cloud} / {fixture}: leaving the defaults out changes the project\n{diff[:4000]}")
    print(f"{len(cases) - len(failures)}/{len(cases)} renders match with and without the default answers.")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
