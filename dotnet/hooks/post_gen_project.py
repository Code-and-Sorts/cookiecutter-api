"""Post-generation hook to clean up files based on cloud service selection."""
import os
import shutil

cloud_service = "{{cookiecutter.cloud_service}}"

files_to_remove = []
dirs_to_remove = []

if cloud_service == "Azure Function App":
    # Remove GCP-specific files
    files_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Function.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Startup.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Interfaces", "IFirestoreContext.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Repositories", "FirestoreContext.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "FunctionTests.cs"),
        # Remove AWS-specific files
        os.path.join("{{cookiecutter.project_class_name}}.Api", "template.yaml"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "aws-lambda-tools-defaults.json"),
    ]
    dirs_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "LambdaFunctions"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "LambdaFunctions"),
    ]
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
        # Remove AWS-specific files
        os.path.join("{{cookiecutter.project_class_name}}.Api", "template.yaml"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "aws-lambda-tools-defaults.json"),
    ]
    dirs_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Functions"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Properties"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "LambdaFunctions"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "LambdaFunctions"),
    ]
elif cloud_service == "AWS Lambda":
    # Remove Azure-specific files
    files_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "host.json"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "{{cookiecutter._local_settings}}.json"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Interfaces", "IDbConnectionFactory.cs"),
        # Remove GCP-specific files
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Function.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Startup.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Interfaces", "IFirestoreContext.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Repositories", "FirestoreContext.cs"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions", "FunctionTests.cs"),
    ]
    dirs_to_remove = [
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Functions"),
        os.path.join("{{cookiecutter.project_class_name}}.Api", "Properties"),
        os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions"),
    ]

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

# Rename LambdaFunctions to Functions for AWS Lambda
if cloud_service == "AWS Lambda":
    lambda_src = os.path.join("{{cookiecutter.project_class_name}}.Api", "LambdaFunctions")
    functions_src = os.path.join("{{cookiecutter.project_class_name}}.Api", "Functions")
    if os.path.isdir(lambda_src):
        os.rename(lambda_src, functions_src)
        print(f"Renamed {lambda_src} to {functions_src}")

    lambda_test = os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "LambdaFunctions")
    functions_test = os.path.join("{{cookiecutter.project_class_name}}.Api.Tests.Unit", "Functions")
    if os.path.isdir(lambda_test):
        os.rename(lambda_test, functions_test)
        print(f"Renamed {lambda_test} to {functions_test}")
