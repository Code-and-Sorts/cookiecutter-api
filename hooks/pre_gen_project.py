from __future__ import annotations

import re
import sys

PROJECT_NAME_REGEX = r"^[-a-zA-Z ][-a-zA-Z0-9 ]+$"
project_name = "{{cookiecutter.project_name}}"
if not re.match(PROJECT_NAME_REGEX, project_name):
    print(
        f"ERROR: The project name {project_name} is not a valid Python module name. Please do not use a _ and use - instead"
    )
    # Exit to cancel project
    sys.exit(1)

PROJECT_SLUG_REGEX = r"^[_a-zA-Z][_a-zA-Z0-9]+$"
project_slug = "{{cookiecutter.project_slug}}"
if not re.match(PROJECT_SLUG_REGEX, project_slug):
    print(
        f"ERROR: The project slug {project_slug} is not a valid Python module name. Please do not use a - and use _ instead."
    )
    # Exit to cancel project
    sys.exit(1)

PROJECT_ENDPOINT_REGEX = r"^[a-zA-Z0-9\-_]*$"
project_endpoint = "{{cookiecutter.project_endpoint}}"
if not re.match(PROJECT_ENDPOINT_REGEX, project_slug):
    print(
        f"ERROR: The project endpoint {project_endpoint} is not a valid REST endpoint name. Please do not use a space or _ and use - instead."
    )
    # Exit to cancel project
    sys.exit(1)

PROJECT_CLASS_NAME_REGEX = r"^[A-Z][A-Za-z0-9]*$"
project_class_name = "{{cookiecutter.project_class_name}}"
if not re.match(PROJECT_CLASS_NAME_REGEX, project_class_name):
    print(
        f"ERROR: The project class name {project_class_name} is not a valid Python class name. Please create a class name using PascalCase."
    )
    # Exit to cancel project
    sys.exit(1)
