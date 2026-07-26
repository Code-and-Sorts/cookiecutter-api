# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built using [AWS Lambda](https://docs.aws.amazon.com/lambda/) with [API Gateway](https://docs.aws.amazon.com/apigateway/). The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/) framework is used for local development and deployment.

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

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

## Features

- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway integration.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- DynamoDB: This project uses DynamoDB for NoSQL data storage.

## Prerequisites

- Go 1.22+

- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To build and run the Lambda functions locally.

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html): To deploy and manage AWS resources.

- [Go](https://go.dev/dl/): Go SDK and CLI

- AWS Account: An active AWS account for deploying the Lambda function.

- DynamoDB table either deployed in AWS or run locally using [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html).

## Setup and Installation

1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install the AWS SAM CLI based on your operating system.

2. Install Go SDK

    If you haven't already installed Go, you can do so by following the [official installation guide](https://go.dev/dl/).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command builds the Go binary using SAM and starts the local API Gateway, where you can interact with your API endpoints.

5. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Development Workflow

### Adding a New Dependency

```bash
go get <package-path>
```

### Removing a Dependency

```bash
go mod tidy
```

## Running Tests

Ensure your code is working as expected by running unit tests using go test:

```bash
make test-unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities using [govulncheck](https://pkg.go.dev/golang.org/x/vuln/cmd/govulncheck):

```bash
make audit
```

This is also run automatically in CI on every PR and push to main.

## Repository structure

```text
├── cookiecutter-template-go
    ├── controllers
    ├── services
    ├── repositories
    ├── models
    ├── utils
    └── template.yaml
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
