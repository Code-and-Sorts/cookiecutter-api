"""Asserts copier copy rejects every answers file in invalid/ with the message its '# expect:' line names."""

import argparse
import subprocess
import sys
import tempfile
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
INVALID = ROOT / ".github/actions/setup-copier-template/invalid"


def expected_message(path: Path) -> str:
    first = path.read_text().splitlines()[0]
    if not first.startswith("# expect: "):
        raise SystemExit(f"{path.name}: the first line must be '# expect: <part of the error message>'")
    return first.removeprefix("# expect: ").strip()


def check(path: Path, language: str) -> str | None:
    """Returns why the file did not fail as expected, or None."""
    expected = expected_message(path)
    with tempfile.TemporaryDirectory() as destination:
        result = subprocess.run(
            ["copier", "copy", "--defaults", "--trust", "--vcs-ref", "HEAD", "--data", f"language={language}",
             "--data-file", str(path), str(ROOT), destination],
            capture_output=True, text=True, cwd=ROOT,
        )
    output = result.stdout + result.stderr
    if result.returncode == 0:
        return "copier copy succeeded"
    if expected not in output:
        last = output.strip().splitlines()[-1] if output.strip() else "(no output)"
        return f"expected '{expected}', got: {last}"
    return None


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--language", default="python", help="Language answer; the field rules are the same for all.")
    parser.add_argument("files", nargs="*", type=Path, help="Answers files to check (default: all of invalid/).")
    args = parser.parse_args()
    files = args.files or sorted(INVALID.glob("*.yml"))
    with ThreadPoolExecutor(max_workers=8) as pool:
        failures = [(path, error) for path, error in zip(files, pool.map(lambda p: check(p, args.language), files)) if error]
    for path, error in failures:
        print(f"FAIL {path.name}: {error}")
    print(f"{len(files) - len(failures)}/{len(files)} invalid answers files were rejected with their expected message.")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
