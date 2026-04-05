"""Post-generation hook to clean up files based on cloud service selection."""
import os
import shutil

cloud_service = "{{cookiecutter.cloud_service}}"

# Files to remove based on cloud service
if cloud_service == "Azure Function App":
    # Remove GCP and AWS-specific files
    files_to_remove = [
        "template.yaml",
    ]
    dirs_to_remove = []
elif cloud_service == "GCP Cloud Function":
    # Remove Azure and AWS-specific files
    files_to_remove = [
        "host.json",
        "{{cookiecutter._local_settings}}.json",
        "template.yaml",
    ]
    # Remove Azure Function trigger directories
    dirs_to_remove = [
        "create{{cookiecutter.project_class_name}}",
        "delete{{cookiecutter.project_class_name}}",
        "get{{cookiecutter.project_class_name}}",
        "get{{cookiecutter.project_class_name}}s",
        "update{{cookiecutter.project_class_name}}",
    ]
elif cloud_service == "AWS Lambda":
    # Remove Azure and GCP-specific files
    files_to_remove = [
        "host.json",
        "{{cookiecutter._local_settings}}.json",
    ]
    dirs_to_remove = [
        "create{{cookiecutter.project_class_name}}",
        "delete{{cookiecutter.project_class_name}}",
        "get{{cookiecutter.project_class_name}}",
        "get{{cookiecutter.project_class_name}}s",
        "update{{cookiecutter.project_class_name}}",
    ]
else:
    files_to_remove = []
    dirs_to_remove = []

# Remove the files
for file_path in files_to_remove:
    if os.path.exists(file_path):
        os.remove(file_path)
        print(f"Removed {file_path} (not needed for {cloud_service})")

# Remove the directories
for dir_path in dirs_to_remove:
    if os.path.exists(dir_path):
        shutil.rmtree(dir_path)
        print(f"Removed {dir_path}/ (not needed for {cloud_service})")
