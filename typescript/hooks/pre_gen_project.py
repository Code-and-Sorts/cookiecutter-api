from __future__ import annotations

import re
import sys

PROJECT_LOWER_CAMEL_REGEX = r"^[a-z]+(?:[A-Z][a-z]+)*$"
project_lower_camel = "{{cookiecutter.project_lower_camel_name}}"
if not re.match(PROJECT_LOWER_CAMEL_REGEX, project_lower_camel):
    print(
        f"ERROR: The project lower camel {project_lower_camel} is not valid camel case. Please create a name using camelCase."
    )
    # Exit to cancel project
    sys.exit(1)

PROJECT_CLASS_NAME_REGEX = r"^[A-Z][A-Za-z0-9]*$"
project_class_name = "{{cookiecutter.project_class_name}}"
if not re.match(PROJECT_CLASS_NAME_REGEX, project_class_name):
    print(
        f"ERROR: The project class name {project_class_name} is not a valid class name. Please create a class name using PascalCase."
    )
    # Exit to cancel project
    sys.exit(1)

PROJECT_ENDPOINT_REGEX = r"^[a-zA-Z0-9\-_]*$"
project_endpoint = "{{cookiecutter.project_endpoint}}"
if not re.match(PROJECT_ENDPOINT_REGEX, project_endpoint):
    print(
        f"ERROR: The project endpoint {project_endpoint} is not a valid REST endpoint name. Please do not use a space or _ and use - instead."
    )
    # Exit to cancel project
    sys.exit(1)
