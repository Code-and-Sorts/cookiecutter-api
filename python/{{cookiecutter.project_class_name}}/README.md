# {{ cookiecutter.project_name }} API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
This project is a Python-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
This project is a Python-based REST API built using [Google Cloud Functions](https://cloud.google.com/functions/docs). The API leverages GCP's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
This project is a Python-based REST API built using [AWS Lambda](https://docs.aws.amazon.com/lambda/) with API Gateway. The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered Lambda functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}

The REST API has the following endpoints:
- GET (by ID)
- GET (list)
- POST
- PATCH
- DELETE (soft-delete)

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
Dependency management is handled using [Poetry](https://python-poetry.org/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
Dependency management can be handled using either [Poetry](https://python-poetry.org/) for development or requirements.txt for GCP deployment.
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
Dependency management is handled using [Poetry](https://python-poetry.org/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.
{%- endif %}

## Features

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- Poetry for Dependency Management: Manages all Python dependencies with Poetry, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
- GCP Cloud Functions: Utilizes Google Cloud's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- Poetry for Development: Manages all Python dependencies with Poetry, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- Poetry for Dependency Management: Manages all Python dependencies with Poetry, making the development environment consistent and easy to set up.

- DynamoDB: This project uses Amazon DynamoDB as the NoSQL database.
{%- endif %}

## Prerequisites

- Python >=3.9, <3.12

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Poetry](https://python-poetry.org/): For dependency management and virtual environment setup.

- Azure Account: An active Azure subscription for deploying the Function App.

- Cosmos DB NoSQL Account either deployed in Azure or [emulated](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql).
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage GCP Cloud Functions.

- [Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-python): To run Cloud Functions locally.

- [Poetry](https://python-poetry.org/): For dependency management and virtual environment setup.

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project.
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To run the Lambda functions locally.

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html): To deploy and manage AWS resources.

- [Poetry](https://python-poetry.org/): For dependency management and virtual environment setup.

- AWS Account: An active AWS account for deploying Lambda functions.

- DynamoDB Table: A DynamoDB table will be created automatically via the SAM template.
{%- endif %}

## Setup and Installation

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
1. Install Azure Functions Core Tools

    Follow the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-python#install-the-azure-functions-core-tools) to install Azure Function Core Tools based on your operating system.

2. Install Poetry

    If you haven't already installed Poetry, you can do so by following the [official installation guide](https://python-poetry.org/docs/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment:

    ```console
    make install
    ```

    To be able to run the project locally, set the environment variable values in the local.settings.json project file.

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the local development server using the Azure Function Core Tools, where you can interact with your API endpoints.

5. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
1. Install Google Cloud SDK

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud SDK based on your operating system.

2. Install Poetry

    If you haven't already installed Poetry, you can do so by following the [official installation guide](https://python-poetry.org/docs/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment:

    ```console
    make install
    ```

4. Set Environment Variables

    Set the following environment variables for local development:
    - `GCP_PROJECT_ID`: Your GCP project ID
    - `FIRESTORE_DATABASE`: Firestore database name (defaults to "(default)")
    - `FIRESTORE_COLLECTION`: Firestore collection name (defaults to "{{ cookiecutter.project_slug }}")

5. Run the API Locally

    To run a specific function locally using Functions Framework:

    ```console
    # Run the get_list function
    poetry run functions-framework --target=get_list --source=main.py --port=8080
    
    # Or run other functions
    poetry run functions-framework --target=get_by_id --source=main.py --port=8080
    poetry run functions-framework --target=create --source=main.py --port=8080
    ```

6. Deploy to GCP

    Deploy individual functions to GCP Cloud Functions:

    ```console
    # Deploy the get_list function
    gcloud functions deploy get_list \
      --runtime python311 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_list \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION={{ cookiecutter.project_slug }}
    ```
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install the AWS SAM CLI based on your operating system.

2. Install Poetry

    If you haven't already installed Poetry, you can do so by following the [official installation guide](https://python-poetry.org/docs/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the local API Gateway using SAM CLI, where you can interact with your API endpoints.

5. Deploy to AWS

    Build and deploy to AWS using SAM:

    ```console
    sam build
    sam deploy --guided
    ```
{%- endif %}

## Development Workflow

### Adding a New Dependency

```bash
poetry add <package-name>
```

### Removing a Dependency

```bash
poetry remove <package-name>
```

## Running Tests

Ensure your code is working as expected by running unit tests using pytest:

```bash
make test-unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities using [pip-audit](https://pypi.org/project/pip-audit/):

```bash
make audit
```

This is also run automatically in CI on every PR and push to main.

## Repository structure

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
```text
├── cookiecutter-template-python
│   ├── .thunderclient     - Thunderclient collection
│   ├── blueprints         - Function App methods
│   ├── controllers        - Controllers
│   ├── errors             - Custom errors
│   ├── models             - Pydantic models
│   ├── repositories       - Cosmos DB repository
│   ├── services           - Services
│   └── utils              - Error detect & response generator utilities
│
└── function_app.py        - Function App entry method
```
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
```text
├── cookiecutter-template-python
│   ├── blueprints         - Cloud Function methods
│   ├── controllers        - Controllers
│   ├── errors             - Custom errors
│   ├── models             - Pydantic models
│   ├── repositories       - Firestore repository
│   ├── services           - Services
│   └── utils              - Error detect & response generator utilities
│
├── main.py                - Cloud Functions entry point
└── requirements.txt       - Production dependencies for GCP deployment
```
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
```text
├── cookiecutter-template-python
│   ├── blueprints         - Lambda handler methods
│   ├── controllers        - Controllers
│   ├── errors             - Custom errors
│   ├── models             - Pydantic models
│   ├── repositories       - DynamoDB repository
│   ├── services           - Services
│   └── utils              - Error detect & response generator utilities
│
├── lambda_app.py          - Lambda entry point with API Gateway routing
└── template.yaml          - AWS SAM template for deployment
```
{%- endif %}

## License

This project is licensed under the {% if cookiecutter.open_source_license == 'MIT license' -%}MIT License{% elif cookiecutter.open_source_license == 'BSD license' %}
BSD License{% elif cookiecutter.open_source_license == 'ISC license' -%}ISC License{% elif cookiecutter.open_source_license == 'Apache Software License 2.0' -%}Apache Software License 2.0{% elif cookiecutter.open_source_license == 'GNU General Public License v3' -%}GNU General Public License v3
{% endif %}. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
