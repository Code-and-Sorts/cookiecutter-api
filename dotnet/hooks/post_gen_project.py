"""Post-generation hook to clean up files based on cloud service selection."""
import os
import shutil

cloud_service = "{{cookiecutter.cloud_service}}"

# Files to remove based on cloud service
if cloud_service == "Azure Function App":
    # Remove GCP-specific files
    files_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Function.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Startup.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Interfaces", "IFirestoreContext.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Repositories", "FirestoreContext.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "FunctionTests.cs"),
    ]
    dirs_to_remove = []
elif cloud_service == "GCP Cloud Function":
    # Remove Azure-specific files
    files_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "host.json"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "{{cookiecutter._local_settings}}.json"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Program.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "Get{{cookiecutter.project_class_name}}Tests.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "Get{{cookiecutter.project_class_name}}ListTests.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "Create{{cookiecutter.project_class_name}}Tests.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "Update{{cookiecutter.project_class_name}}Tests.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "Delete{{cookiecutter.project_class_name}}Tests.cs"),
    ]
    dirs_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Functions"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Properties"),
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
        print(f"Removed {dir_path}/ directory (not needed for {cloud_service})")
