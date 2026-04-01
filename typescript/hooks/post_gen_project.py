"""Post-generation hook to clean up files based on cloud service selection."""
import os
import shutil

cloud_service = "{{cookiecutter.cloud_service}}"

# Files to remove based on cloud service
if cloud_service == "Azure Function App":
    # Remove GCP-specific files
    files_to_remove = [
        "main.ts",
    ]
elif cloud_service == "GCP Cloud Function":
    # Remove Azure-specific files
    files_to_remove = [
        "host.json",
        "local.settings.json",
    ]
    # Remove Azure Functions directory
    if os.path.exists("functions"):
        shutil.rmtree("functions")
        print(f"Removed functions/ directory (not needed for {cloud_service})")
else:
    files_to_remove = []

# Remove the files
for file_path in files_to_remove:
    if os.path.exists(file_path):
        os.remove(file_path)
        print(f"Removed {file_path} (not needed for {cloud_service})")
