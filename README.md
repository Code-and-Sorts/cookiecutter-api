# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [AWS Lambda](https://aws.amazon.com/lambda/) with [API Gateway](https://aws.amazon.com/api-gateway/). The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The Lambda functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/cats`** (container: `animals`)
  - `GET /cats` — list
  - `GET /cats/{id}` — get by ID
  - `POST /cats` — create
  - `PATCH /cats/{id}` — partial update
  - `DELETE /cats/{id}` — soft delete
- **`/dogs`** (container: `animals`)
  - `GET /dogs` — list
  - `GET /dogs/{id}` — get by ID
  - `POST /dogs` — create
  - `PUT /dogs/{id}` — full replace
  - `DELETE /dogs/{id}` — soft delete

Dependency management is handled using [Nuget](https://www.nuget.org/), ensuring a streamlined and consistent environment for managing Dotnet packages and their dependencies.

## Features

- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway integration.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- DynamoDB: This project uses Amazon DynamoDB as its NoSQL database.

## Prerequisites

- Dotnet 8.x

- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To build and run the Lambda functions locally.

- [AWS CLI](https://aws.amazon.com/cli/): To deploy and manage AWS resources.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- AWS Account: An active AWS account for deploying Lambda functions.

- DynamoDB table (created automatically via the SAM template or available via [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html)).

## Setup and Installation

1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install AWS SAM CLI based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command builds the project and starts a local API Gateway using SAM CLI, where you can interact with your API endpoints.

5. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Development Workflow

### Adding a New Dependency

```bash
dotnet add package <package-name>
```

### Removing a Dependency

```bash
dotnet remove package <package-name>
```

## Running Tests

Ensure your code is working as expected by running unit tests using dotnet test:

```bash
make test-unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities:

```bash
make audit
```

This uses `dotnet list package --vulnerable --include-transitive` to check for packages with known CVEs. It is also run automatically in CI on every PR and push to main.

## Repository structure

```text
├── cookiecutter-template-dotnet
    ├── KittenClaws.Api
    │   ├── Controllers
    │   ├── Functions
    │   ├── Interfaces
    │   ├── Repositories
    │   ├── Services
    │   ├── models
    │   │   ├── Dtos
    │   │   ├── Entities
    │   │   └── schemas
    │   └── utils
    └── KittenClaws.Api.Tests.Unit
        ├── Controllers
        ├── Functions
        ├── Repositories
        ├── Services
        └── utils
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
