"""Builds the request log page from the integration suite's --record files.

Usage: build_report.py <logs dir> <out.html> [--fixture single] [--run-url URL]

<logs dir> holds requests-<language>-<cloud>-<fixture>/requests.jsonl, as the integration workflow's
artifacts or extract_job_logs.py leave them.
"""

import argparse
import json
from pathlib import Path

LANGUAGES = ["python", "typescript", "go", "dotnet"]
CLOUDS = ["azure", "gcp", "aws"]
TEMPLATE = Path(__file__).with_name("template.html")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("logs", type=Path)
    parser.add_argument("out", type=Path)
    parser.add_argument("--fixture", default="single")
    parser.add_argument("--run-url", default="")
    args = parser.parse_args()

    combos = []
    for language in LANGUAGES:
        for cloud in CLOUDS:
            log = args.logs / f"requests-{language}-{cloud}-{args.fixture}" / "requests.jsonl"
            lines = log.read_text().splitlines() if log.exists() else []
            entries = [json.loads(line) for line in lines if line.strip()]
            for entry in entries:
                entry["test"] = entry.get("test") or "(suite setup)"
            combos.append({"language": language, "cloud": cloud, "entries": entries})

    data = {"run": args.run_url, "fixture": args.fixture, "combos": combos}
    # "</" would end the inline <script> that holds the data.
    payload = json.dumps(data, separators=(",", ":")).replace("</", "<\\/")
    args.out.write_text(TEMPLATE.read_text().replace("__DATA__", payload))
    total = sum(len(c["entries"]) for c in combos)
    missing = [f"{c['language']}/{c['cloud']}" for c in combos if not c["entries"]]
    print(f"{args.out}: {total} requests" + (f"; no log for {', '.join(missing)}" if missing else ""))


if __name__ == "__main__":
    main()
