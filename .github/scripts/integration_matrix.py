"""Prints the integration test matrix (language x cloud x fixture) for the workflow's inputs as a step output."""

import argparse
import json

LANGUAGES = ["python", "typescript", "dotnet", "go"]
CLOUDS = [
    {"name": "Azure Function App", "slug": "azure"},
    {"name": "GCP Cloud Function", "slug": "gcp"},
    {"name": "AWS Lambda", "slug": "aws"},
]
# The fixtures CI builds; multi and model are published examples that only add cases these cover.
FIXTURES = ["single", "edge"]


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--language", default="all")
    parser.add_argument("--cloud", default="all")
    parser.add_argument("--fixture", default="all")
    args = parser.parse_args()
    matrix = {
        "language": [language for language in LANGUAGES if args.language in ("all", language)],
        "cloud": [cloud for cloud in CLOUDS if args.cloud in ("all", cloud["slug"])],
        "fixture": FIXTURES if args.fixture == "all" else [args.fixture],
    }
    print(f"matrix={json.dumps(matrix)}")


if __name__ == "__main__":
    main()
