# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview



This project is a Python-based REST API built using [AWS Lambda](https://docs.aws.amazon.com/lambda/) with API Gateway. The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered Lambda functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/kitties`** (container: `kitty_cats`)
  - `GET /kitties` — list
  - `GET /kitties/{item_id}` — get by ID
  - `POST /kitties` — create
  - `PATCH /kitties/{item_id}` — partial update
  - `DELETE /kitties/{item_id}` — soft delete



Dependency management is handled using [Poetry](https://python-poetry.org/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.

## Features



- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- Poetry for Dependency Management: Manages all Python dependencies with Poetry, making the development environment consistent and easy to set up.

- DynamoDB: This project uses Amazon DynamoDB as the NoSQL database.

## Prerequisites

- Python >=3.9, <3.12



- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To run the Lambda functions locally.

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html): To deploy and manage AWS resources.

- [Poetry](https://python-poetry.org/): For dependency management and virtual environment setup.

- AWS Account: An active AWS account for deploying Lambda functions.

- DynamoDB Table: A DynamoDB table will be created automatically via the SAM template.

## Setup and Installation



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

    > **Note:** API endpoints that interact with DynamoDB require a running DynamoDB instance.
    > For local development, you can use [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html)
    > or connect to a deployed DynamoDB table by configuring your AWS credentials and setting the
    > `DYNAMODB_TABLE_NAME` environment variable in `template.yaml`.

5. Deploy to AWS

    Build and deploy to AWS using SAM:

    ```console
    sam build
    sam deploy --guided
    ```

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

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
