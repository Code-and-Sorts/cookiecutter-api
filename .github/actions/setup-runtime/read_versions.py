import os

import yaml

with open(os.path.join(os.environ["GITHUB_WORKSPACE"], "copier.yml"), encoding="utf-8") as copier:
    versions = dict(yaml.safe_load(copier)["runtime_versions"]["default"])
versions.update(versions.pop("clouds").get(os.environ["CLOUD"], {}))

with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as output:
    for name, version in versions.items():
        output.write(f"{name}={version}\n")
