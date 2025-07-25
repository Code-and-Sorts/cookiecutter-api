#!/usr/bin/env python
"""Post-generation hook to handle conditional files based on cloud service."""

import os

cloud_service = "{{cookiecutter.cloud_service}}"

# Create main.py only for Google Cloud Functions
if cloud_service == "Google Cloud Function":
    main_py_content = '''"""
Main entry point for Google Cloud Functions.
This file is required by Google Cloud Functions runtime.
"""

from blueprints.{{cookiecutter.project_slug}}_api import {{ cookiecutter.project_slug }}_api

# The function name must match what's defined in the blueprint
# Google Cloud Functions will call this function directly
'''
    with open("main.py", "w") as f:
        f.write(main_py_content)

# Remove Azure-specific files for Google Cloud Functions
if cloud_service == "Google Cloud Function":
    files_to_remove = ["host.json", "{{cookiecutter._local_settings}}.json"]
    for file_path in files_to_remove:
        if os.path.exists(file_path):
            os.remove(file_path)

# Remove Google Cloud Function-specific files for Azure Function Apps
if cloud_service == "Azure Function App":
    if os.path.exists("main.py"):
        os.remove("main.py")