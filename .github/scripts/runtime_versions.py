import os

from copier_config import runtime_versions, write

write("GITHUB_OUTPUT", runtime_versions(os.environ["CLOUD"]))
