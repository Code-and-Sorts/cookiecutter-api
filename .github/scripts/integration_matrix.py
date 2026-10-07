import json
import os

from copier_config import default, write

LANGUAGES = ["python", "typescript", "dotnet", "go"]
CLOUDS = {
    "Azure Function App": "azure",
    "GCP Cloud Function": "gcp",
    "AWS Lambda": "aws",
}
HOSTS = ["local", "container"]


def pick(options: list[str], choice: str) -> list[str]:
    return [option for option in options if choice in ("all", option)]


def main() -> None:
    env = os.environ
    container_clouds = default("infra_clouds")
    fixtures = (
        ["multi"]
        if env["FIXTURE"] == "multi"
        else pick(["single", "edge"], env["FIXTURE"])
    )
    include = [
        {
            "language": language,
            "cloud": {"name": name, "slug": slug},
            "fixture": fixture,
            "host": host,
        }
        for host in pick(HOSTS, env["HOST"])
        for language in pick(LANGUAGES, env["LANGUAGE"])
        for name, slug in CLOUDS.items()
        if env["CLOUD"] in ("all", slug)
        and (host == "local" or name in container_clouds)
        for fixture in fixtures
    ]
    if not include:
        raise SystemExit(
            "No job matches; only clouds with infrastructure run in a container."
        )
    matrix = json.dumps({"include": include}, separators=(",", ":"))
    print(f"Matrix: {matrix}")
    write("GITHUB_OUTPUT", {"matrix": matrix})


if __name__ == "__main__":
    main()
