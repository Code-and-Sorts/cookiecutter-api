"""Asserts every language renders base_model's field data, as shared/_fields.jinja derives it, into its base types.

Renders each language with the same answers, reads BaseEntity, the three base request types and BaseResponse back
out of the generated code, and compares every field's name, kind (date, date-time, uuid, enum and so on, or the
family a type stands for where the language's type is wider, such as a string) and, for request types, its
required and nullable flags with the macros' data. String literals are blanked before code is read, so a default
or a message can never look like a type.
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

import yaml

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tests/integration"))
from project import FIELDS  # noqa: E402  the integration suite's renderer of shared/_fields.jinja

FIXTURES = ROOT / ".github/actions/setup-copier-template/fixtures"
LANGUAGES = ["python", "typescript", "dotnet", "go"]
OPERATIONS = {"BaseCreateRequest": "create", "BaseReplaceRequest": "replace", "BaseUpdateRequest": "update"}
BASE_TYPES = ["BaseEntity", *OPERATIONS, "BaseResponse"]
STRINGS = frozenset({"string", "date", "date-time", "uuid", "enum"})
NUMBERS = frozenset({"integer", "number"})
STRING_LITERAL = re.compile(r"'(?:\\.|[^'\\])*'|\"(?:\\.|[^\"\\])*\"")

# A field as a language states it: the kinds its type allows, required and nullable (None where it does not say).
Field = tuple[frozenset[str], bool | None, bool | None]


def blank_strings(code: str) -> str:
    return STRING_LITERAL.sub('""', code)


def array_of(kinds: frozenset[str]) -> frozenset[str]:
    return frozenset(f"array of {kind}" for kind in kinds)


def expected(base_model: list[dict]) -> dict[str, dict[str, tuple[str, bool, bool]]]:
    """Each base type's fields as (kind, required, nullable), from the macros templates render."""
    fields = json.loads(str(FIELDS.base_fields_json(base_model)))
    kind = {f["name"]: f"array of {f['item_type']}" if f["type"] == "array" else f["type"] for f in fields}
    types = {"BaseEntity": {f["name"]: (kind[f["name"]], False, False) for f in fields}}
    for base_type, op in OPERATIONS.items():
        types[base_type] = {
            f["name"]: (kind[f["name"]], f["needs_value"] and op != "update", f["nullable"]) for f in fields if f[f"in_{op}"]
        }
    types["BaseResponse"] = {f["name"]: (kind[f["name"]], False, False) for f in fields if not f["hidden"]}
    return types


def render(language: str, cloud: str, fixture: Path, destination: Path) -> Path:
    subprocess.run(
        ["copier", "copy", "--defaults", "--trust", "--vcs-ref", "HEAD", "--quiet", "--data", "project_name=KittenClaws",
         "--data", f"language={language}", "--data", f"cloud_service={cloud}", "--data-file", str(fixture),
         str(ROOT), str(destination)],
        check=True, capture_output=True, cwd=ROOT,
    )
    return destination


PYTHON_KINDS = {
    "DateTimeStr": {"date-time"}, "DateStr": {"date"}, "UuidStr": {"uuid"}, "SafeInt": {"integer"}, "int": {"integer"},
    "Number": {"number"}, "float": {"number"}, "StrictBool": {"boolean"}, "bool": {"boolean"}, "StrictStr": {"string"},
    "str": STRINGS,
}


def python_kinds(node: ast.expr) -> frozenset[str]:
    if isinstance(node, ast.BinOp):
        return python_kinds(node.left) | python_kinds(node.right)
    if isinstance(node, ast.Constant):
        return frozenset()
    if isinstance(node, ast.Name):
        return frozenset(PYTHON_KINDS[node.id])
    generic = node.value.id
    args = node.slice.elts if isinstance(node.slice, ast.Tuple) else [node.slice]
    if generic == "Literal":
        return frozenset({"enum"})
    if generic in ("List", "list"):
        return array_of(python_kinds(args[0]))
    return python_kinds(args[0])  # Annotated[type, constraints...]


def python_types(project: Path) -> dict[str, dict[str, Field]]:
    types = {}
    for node in ast.parse((project / "models/base.py").read_text()).body:
        if isinstance(node, ast.ClassDef) and node.name in BASE_TYPES:
            request = node.name in OPERATIONS
            types[node.name] = {
                item.target.id: (
                    python_kinds(item.annotation),
                    item.value is None if request else None,
                    "None" in ast.unparse(item.annotation) if request else None,
                )
                for item in node.body
                if isinstance(item, ast.AnnAssign)
            }
    return types


ZOD_KINDS = [
    ("z.string()", {"string"}), ("z.int()", {"integer"}), ("z.number()", {"number"}), ("z.boolean()", {"boolean"}),
    ("dateSchema", {"date"}), ("dateTimeSchema", {"date-time"}), ("uuidSchema", {"uuid"}), ("z.enum(", {"enum"}),
]
TS_KINDS = {"string": STRINGS, "number": NUMBERS, "boolean": frozenset({"boolean"})}


def zod_kinds(expression: str) -> frozenset[str]:
    if expression.startswith("z.array("):
        return array_of(zod_kinds(expression.removeprefix("z.array(")))
    return frozenset(next(kinds for prefix, kinds in ZOD_KINDS if expression.startswith(prefix)))


def ts_kinds(type_text: str) -> frozenset[str]:
    type_text = type_text.removesuffix(" | null")
    if type_text.endswith("[]"):
        return array_of(ts_kinds(type_text.removesuffix("[]")))
    return frozenset({"enum"}) if type_text.startswith('""') else TS_KINDS[type_text]


def typescript_types(project: Path) -> dict[str, dict[str, Field]]:
    source = blank_strings((project / "types/models/base.schema.ts").read_text())
    types = {}
    for name, body in re.findall(r"^export type (Base\w+) = \{(.*?)\n\};", source, re.S | re.M):
        types[name] = {field: (ts_kinds(type_text), None, None) for field, type_text in re.findall(r"^\s+(\w+)\??: (.+);$", body, re.M)}
    for name, body in re.findall(r"^export const (Base\w+)Schema = z\.strictObject\(\{(.*?)\n\}\);", source, re.S | re.M):
        fields = {}
        for field, expression in re.findall(r"^\s+(\w+): (.+),$", body, re.M):
            optional = any(marker in expression for marker in (".optional()", ".default("))
            fields[field] = (zod_kinds(expression), not optional, ".nullable()" in expression)
        types[name] = fields
    return types


GO_KINDS = {"string": STRINGS, "DateTime": frozenset({"date-time"}), "int64": frozenset({"integer"}),
            "float64": frozenset({"number"}), "bool": frozenset({"boolean"})}


def go_kinds(type_text: str) -> frozenset[str]:
    type_text = type_text.lstrip("*")
    if type_text == "UniqueDateTimes":
        return array_of(frozenset({"date-time"}))
    if type_text.startswith("[]"):
        return array_of(go_kinds(type_text[2:]))
    return GO_KINDS[type_text]


def schema_kinds(schema: dict) -> frozenset[str]:
    """A JSON Schema's kind; uuid is the shared uuid pattern, which no other format uses."""
    types = schema["type"] if isinstance(schema["type"], list) else [schema["type"]]
    kind = next(t for t in types if t != "null")
    if kind == "array":
        return array_of(schema_kinds(schema["items"]))
    if "enum" in schema:
        return frozenset({"enum"})
    if "format" in schema:
        return frozenset({schema["format"]})
    if schema.get("pattern") == FIELDS.PATTERNS["uuid"]:
        return frozenset({"uuid"})
    return frozenset({kind})


def go_types(project: Path) -> dict[str, dict[str, Field]]:
    source = (project / "models/base.go").read_text()
    types = {}
    for name, body in re.findall(r"^type (Base\w+) struct \{(.*?)\n\}", source, re.S | re.M):
        schema = _go_schema(project, OPERATIONS.get(name))
        fields = {}
        for type_text, field in re.findall(r"^\t\w+\s+(\S+)\s+`json:\"([^\",]+)", body, re.M):
            if field == "-":
                continue
            if schema:
                prop = schema["properties"][field]
                fields[field] = (schema_kinds(prop), field in schema.get("required", []), "null" in prop["type"])
            else:
                fields[field] = (go_kinds(type_text), None, None)
        types[name] = fields
    return types


def _go_schema(project: Path, operation: str | None) -> dict:
    """A resource's request schema for the operation; base fields read the same in every resource's."""
    if not operation:
        return {}
    schemas = sorted((project / "controllers/schemas").glob(f"*_{operation}_request.json"))
    return json.loads(schemas[0].read_text()) if schemas else {}


DOTNET_KINDS = {"string": STRINGS, "long": frozenset({"integer"}), "double": frozenset({"number"}), "bool": frozenset({"boolean"})}
DOTNET_CHECKS = [("Fields.IsDateTime", "date-time"), ("Fields.IsDate", "date"), ("Fields.IsUuid", "uuid"), ("value is null or", "enum")]


def dotnet_kinds(type_text: str) -> frozenset[str]:
    type_text = type_text.removesuffix("?")
    if type_text.startswith("List<"):
        return array_of(dotnet_kinds(type_text[5:-1]))
    return DOTNET_KINDS[type_text]


def dotnet_rules(api: Path) -> dict[str, tuple[set[str], set[str], dict[str, frozenset[str]]]]:
    """Per base request type: the fields it requires, the properties it refuses null for, and the kinds checks pin down."""
    path = api / "Models/Schemas/BaseValidation.cs"
    rules = {}
    for name, body in re.findall(r"public class (Base\w+)Validator\b.*?\{(.*?)\n\}", path.read_text() if path.exists() else "", re.S):
        required = set(re.findall(r'sent\.Contains\("(\w+)"\)\)', body))
        not_null, kinds = set(), {}
        for prop, check in re.findall(r"RuleFor\(x => x\.(\w+)\)\.(.*?)\.WithMessage\(", blank_strings(body)):
            if check.startswith("NotNull()"):
                not_null.add(prop)
            if kind := next((kind for marker, kind in DOTNET_CHECKS if marker in check), None):
                kinds[prop] = frozenset({f"array of {kind}" if "TrueForAll" in check else kind})
        rules[name] = (required, not_null, kinds)
    return rules


def dotnet_types(project: Path) -> dict[str, dict[str, Field]]:
    api = next(project.glob("*.Api"))
    paths = [api / "Models/Entities/BaseEntity.cs", api / "Models/BaseRequests.cs", api / "Models/Dtos/BaseResponse.cs"]
    source = "\n".join(path.read_text() for path in paths if path.exists())
    rules = dotnet_rules(api)
    types = {}
    for name, body in re.findall(r"public class (Base\w+)\b[^{]*\{(.*?)\n\}", source, re.S):
        required, not_null, kinds = rules.get(name, (set(), set(), {}))
        fields, json_name, ignored = {}, None, False
        for line in body.splitlines():
            line = line.strip()
            if attribute := re.match(r'\[JsonPropertyName\("(\w+)"\)\]', line):
                json_name = attribute[1]
            elif line == "[JsonIgnore]":
                ignored = True
            elif prop := re.match(r"public (\S+) (\w+) \{ get;", line):
                field = json_name or prop[2][0].lower() + prop[2][1:]
                if not ignored and name in OPERATIONS:
                    fields[field] = (kinds.get(prop[2], dotnet_kinds(prop[1])), field in required, prop[2] not in not_null)
                elif not ignored:
                    fields[field] = (dotnet_kinds(prop[1]), None, None)
                json_name, ignored = None, False
        types[name] = fields
    return types


EXTRACTORS = {"python": python_types, "typescript": typescript_types, "dotnet": dotnet_types, "go": go_types}


def problems(language: str, found: dict[str, dict[str, Field]], wanted: dict[str, dict[str, tuple]]) -> list[str]:
    """Where a language's base types differ from the macros' field data."""
    issues = []
    for base_type, fields in wanted.items():
        stated = found.get(base_type, {})
        if stated.keys() != fields.keys():
            issues.append(f"{language} {base_type} has fields {sorted(stated)}, expected {sorted(fields)}")
            continue
        for field, (kind, required, nullable) in fields.items():
            kinds, stated_required, stated_nullable = stated[field]
            if kind not in kinds:
                issues.append(f"{language} {base_type}.{field} is {sorted(kinds)}, expected {kind}")
            if stated_required is not None and stated_required != required:
                issues.append(f"{language} {base_type}.{field} required is {stated_required}, expected {required}")
            if stated_nullable is not None and stated_nullable != nullable:
                issues.append(f"{language} {base_type}.{field} nullable is {stated_nullable}, expected {nullable}")
    return issues


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fixture", default="edge")
    parser.add_argument("--cloud", default="GCP Cloud Function")
    args = parser.parse_args()
    fixture = FIXTURES / f"{args.fixture}-resources.yml"
    wanted = expected(yaml.safe_load(fixture.read_text()).get("base_model") or FIELDS.LOCKED_FIELDS)
    with tempfile.TemporaryDirectory() as tmp:
        projects = list(ThreadPoolExecutor(max_workers=4).map(
            lambda language: render(language, args.cloud, fixture, Path(tmp) / language), LANGUAGES
        ))
        found = {language: EXTRACTORS[language](project) for language, project in zip(LANGUAGES, projects)}
    issues = [issue for language, types in found.items() for issue in problems(language, types, wanted)]
    for language, types in found.items():
        print(f"{language}: " + ", ".join(f"{name} ({len(fields)})" for name, fields in sorted(types.items())))
    for issue in issues:
        print(f"FAIL {issue}")
    if not issues:
        print("Every language renders the base fields' names, kinds and flags as shared/_fields.jinja derives them.")
    return 1 if issues else 0


if __name__ == "__main__":
    sys.exit(main())
