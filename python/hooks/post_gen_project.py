"""Post-generation hook to clean up files based on cloud service selection."""
import os
import shutil

cloud_service = "{{cookiecutter.cloud_service}}"

# Files to remove based on cloud service
if cloud_service == "Azure Function App":
    # Remove GCP and AWS-specific files
    files_to_remove = [
        "main.py",
        "lambda_app.py",
        "template.yaml",
    ]
elif cloud_service == "GCP Cloud Function":
    # Remove Azure and AWS-specific files
    files_to_remove = [
        "function_app.py",
        "local.settings.json",
        "host.json",
        "lambda_app.py",
        "template.yaml",
    ]
elif cloud_service == "AWS Lambda":
    # Remove Azure and GCP-specific files
    files_to_remove = [
        "function_app.py",
        "local.settings.json",
        "main.py",
        "host.json",
    ]
else:
    files_to_remove = []

# Remove the files
for file_path in files_to_remove:
    if os.path.exists(file_path):
        os.remove(file_path)
        print(f"Removed {file_path} (not needed for {cloud_service})")
