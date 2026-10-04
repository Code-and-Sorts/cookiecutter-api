"""Pulls the request logs out of saved integration job logs.

Usage: extract_job_logs.py <out dir> <job log file>...

Each integration job prints its requests.jsonl between REQUEST LOG markers. A job log file is the
GitHub tool's saved output (JSON with a logs_content field) or the raw log text. Each one becomes
<out dir>/requests-<language>-<cloud>-<fixture>/requests.jsonl, named from the job's COMBO line.
"""

import json
import re
import sys
from pathlib import Path

CLOUD_SLUGS = {"Azure Function App": "azure", "GCP Cloud Function": "gcp", "AWS Lambda": "aws"}
TIMESTAMP = re.compile(r"^\S+Z ")
COMBO = re.compile(r"COMBO: (\w+) \| ([^|]+?) \| (\w+)")


def log_lines(path: Path) -> list[str]:
    raw = path.read_text()
    try:
        raw = json.loads(raw)["logs_content"]
    except (ValueError, KeyError, TypeError):
        pass
    return [TIMESTAMP.sub("", line) for line in raw.splitlines()]


def extract(path: Path, out: Path) -> None:
    lines = log_lines(path)
    combo = next((m for line in lines if (m := COMBO.search(line))), None)
    if combo is None:
        print(f"{path}: no COMBO line; is this an integration job log?", file=sys.stderr)
        return
    begin = lines.index("=== REQUEST LOG BEGIN ===")
    end = lines.index("=== REQUEST LOG END ===")
    entries = [line for line in lines[begin + 1 : end] if line.startswith("{")]
    for entry in entries:
        json.loads(entry)
    language, cloud, fixture = combo.group(1), CLOUD_SLUGS[combo.group(2).strip()], combo.group(3)
    target = out / f"requests-{language}-{cloud}-{fixture}"
    target.mkdir(parents=True, exist_ok=True)
    (target / "requests.jsonl").write_text("\n".join(entries) + "\n")
    print(f"{language} {cloud} {fixture}: {len(entries)} requests")


def main() -> None:
    out = Path(sys.argv[1])
    for path in sys.argv[2:]:
        extract(Path(path), out)


if __name__ == "__main__":
    main()
