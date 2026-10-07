import json
import subprocess
import sys
from pathlib import Path

SKIPPED = {"infra", ".github", "node_modules", ".git"}


def app_settings(project: Path) -> dict:
    infra = project / "infra"
    stack = subprocess.run(
        ["atmos", "list", "stacks"],
        cwd=infra,
        check=True,
        capture_output=True,
        text=True,
    ).stdout.split()[0]
    component = subprocess.run(
        ["atmos", "describe", "component", "api", "-s", stack, "--format", "json"],
        cwd=infra,
        check=True,
        capture_output=True,
        text=True,
    ).stdout
    return json.loads(component)["vars"]["compute"]["app_settings"]


def source_text(project: Path) -> str:
    return "\n".join(
        path.read_text(errors="ignore")
        for path in project.rglob("*")
        if path.is_file()
        and not SKIPPED & set(path.relative_to(project).parts)
        and path.suffix != ".md"
    )


def is_read(name: str, text: str) -> bool:
    if name in text:
        return True
    if name.startswith("ConnectionStrings__"):
        return f'"{name.removeprefix("ConnectionStrings__")}"' in text
    prefix, _, suffix = name.rpartition("_")
    return f'"{prefix}_"' in text and f'"{suffix}"' in text


def main() -> None:
    project, language = Path(sys.argv[1]), sys.argv[2]
    settings = app_settings(project)
    text = source_text(project)
    if language == "python":
        text, settings = (
            text.lower(),
            {name.lower(): value for name, value in settings.items()},
        )
    missing = [name for name in settings if not is_read(name, text)]
    if missing:
        sys.exit(f"App settings the code never reads: {', '.join(missing)}")
    print(f"All {len(settings)} app settings are read by the code.")


if __name__ == "__main__":
    main()
