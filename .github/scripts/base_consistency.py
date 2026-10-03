"""Asserts every language renders the same base types from one base_model: field names, kinds and flags.

Renders each language with the same answers, reads BaseEntity, the three base request types and BaseResponse
back out of the generated code, and compares them. Required and nullable flags are compared for the languages
whose request types state them (Python, TypeScript, and Go through its request JSON Schemas).
"""

import argparse
import ast
import json
import re
import subprocess
import sys
import tempfile
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
FIXTURES = ROOT / ".github/actions/setup-copier-template/fixtures"
LANGUAGES = ["python", "typescript", "dotnet", "go"]
BASE_TYPES = ["BaseEntity", "BaseCreateRequest", "BaseReplaceRequest", "BaseUpdateRequest", "BaseResponse"]
OPERATIONS = {"BaseCreateRequest": "create", "BaseReplaceRequest": "replace", "BaseUpdateRequest": "update"}
# Checked in order: an array of integers is an array, and Python reads a number as int | float.
KINDS = [
    ("array", r"(^\*?\[\]|\bList\[|z\.array|\bList<)"),
    ("number", r"\b(Number|float64|z\.number|double|float)\b"),
    ("integer", r"\b(SafeInt|int64|z\.int\(\)|long|int)\b"),
    ("boolean", r"\b(StrictBool|bool|z\.boolean)\b"),
]

# A field as a language states it: (kind, required, nullable); a flag is None where the language does not say.
Field = tuple[str, bool | None, bool | None]


def kind(type_text: str) -> str:
    return next((name for name, pattern in KINDS if re.search(pattern, type_text.strip())), "string")


def render(language: str, cloud: str, fixture: str, destination: Path) -> Path:
    subprocess.run(
        ["copier", "copy", "--defaults", "--trust", "--vcs-ref", "HEAD", "--quiet", "--data", "project_name=KittenClaws",
         "--data", f"language={language}", "--data", f"cloud_service={cloud}",
         "--data-file", str(FIXTURES / f"{fixture}-resources.yml"), str(ROOT), str(destination)],
        check=True, capture_output=True, cwd=ROOT,
    )
    return destination


def python_types(project: Path) -> dict[str, dict[str, Field]]:
    types = {}
    for node in ast.parse((project / "models/base.py").read_text()).body:
        if isinstance(node, ast.ClassDef) and node.name in BASE_TYPES:
            request = node.name in OPERATIONS
            types[node.name] = {
                item.target.id: (
                    kind(ast.unparse(item.annotation)),
                    item.value is None if request else None,
                    "| None" in ast.unparse(item.annotation) if request else None,
                )
                for item in node.body
                if isinstance(item, ast.AnnAssign)
            }
    return types


def typescript_types(project: Path) -> dict[str, dict[str, Field]]:
    source = (project / "types/models/base.schema.ts").read_text()
    types = {}
    for name, body in re.findall(r"export const (Base\w+)Schema = z\.\w+\(\{(.*?)\n\}\);", source, re.S):
        request = name in OPERATIONS
        fields = {}
        for field, expression in re.findall(r"^\s+(\w+): (.+),$", body, re.M):
            optional = any(marker in expression for marker in (".optional()", ".default(", ".nullish()"))
            nullable = ".nullable()" in expression or ".nullish()" in expression
            fields[field] = (kind(expression), not optional if request else None, nullable if request else None)
        types[name] = fields
    return types


def go_types(project: Path) -> dict[str, dict[str, Field]]:
    source = (project / "models/base.go").read_text()
    types = {}
    for name, body in re.findall(r"^type (Base\w+) struct \{(.*?)\n\}", source, re.S | re.M):
        schema = _go_schema(project, OPERATIONS.get(name))
        fields = {}
        for type_text, field in re.findall(r"^\t\w+\s+(\S+)\s+`json:\"([^\",]+)", body, re.M):
            if field == "-":
                continue
            flags = (field in schema.get("required", []), "null" in schema["properties"][field]["type"]) if schema else (None, None)
            fields[field] = (kind(type_text), *flags)
        types[name] = fields
    return types


def _go_schema(project: Path, operation: str | None) -> dict:
    """A resource's request schema for the operation; base fields read the same in every resource's."""
    if not operation:
        return {}
    schemas = sorted((project / "controllers/schemas").glob(f"*_{operation}_request.json"))
    return json.loads(schemas[0].read_text()) if schemas else {}


def dotnet_types(project: Path) -> dict[str, dict[str, Field]]:
    api = next(project.glob("*.Api"))
    source = "\n".join(
        path.read_text() for path in [api / "Models/Entities/BaseEntity.cs", api / "Models/BaseRequests.cs", api / "Models/Dtos/BaseResponse.cs"]
        if path.exists()
    )
    types = {}
    for name, body in re.findall(r"public class (Base\w+)\b[^{]*\{(.*?)\n\}", source, re.S):
        fields, json_name, ignored = {}, None, False
        for line in body.splitlines():
            line = line.strip()
            if attribute := re.match(r'\[(?:JsonPropertyName|FirestoreProperty)\("(\w+)"\)\]', line):
                json_name = attribute[1]
            elif line == "[JsonIgnore]":
                ignored = True
            elif prop := re.match(r"public (\S+) (\w+) \{ get;", line):
                if not ignored:
                    fields[json_name or prop[2][0].lower() + prop[2][1:]] = (kind(prop[1]), None, None)
                json_name, ignored = None, False
        types[name] = fields
    return types


EXTRACTORS = {"python": python_types, "typescript": typescript_types, "dotnet": dotnet_types, "go": go_types}


def differences(found: dict[str, dict[str, dict[str, Field]]]) -> list[str]:
    """Each base type field where the languages disagree, comparing a flag only among languages that state it."""
    problems = []
    for base_type in BASE_TYPES:
        present = {language: types[base_type] for language, types in found.items() if base_type in types}
        names = set().union(*(fields.keys() for fields in present.values())) if present else set()
        for field in sorted(names):
            values = {language: fields.get(field) for language, fields in present.items()}
            if any(value is None for value in values.values()):
                problems.append(f"{base_type}.{field} is missing in {sorted(lang for lang, v in values.items() if v is None)}")
                continue
            for index, label in enumerate(("kind", "required", "nullable")):
                stated = {language: value[index] for language, value in values.items() if value[index] is not None}
                if len(set(stated.values())) > 1:
                    problems.append(f"{base_type}.{field} {label} differs: {stated}")
    return problems


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fixture", default="model")
    parser.add_argument("--cloud", default="GCP Cloud Function")
    args = parser.parse_args()
    with tempfile.TemporaryDirectory() as tmp:
        projects = list(ThreadPoolExecutor(max_workers=4).map(
            lambda language: render(language, args.cloud, args.fixture, Path(tmp) / language), LANGUAGES
        ))
        found = {language: EXTRACTORS[language](project) for language, project in zip(LANGUAGES, projects)}
    for language, types in found.items():
        print(f"{language}: " + ", ".join(f"{name} ({len(fields)})" for name, fields in sorted(types.items())))
    problems = differences(found)
    for problem in problems:
        print(f"FAIL {problem}")
    if not problems:
        print("Every language renders the same base fields, kinds and flags.")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
