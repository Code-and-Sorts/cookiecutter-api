# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) with a [custom handler](https://learn.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/kitties`** (container: `kitties`)
  - `GET /kitties` — list
  - `GET /kitties/{id}` — get by ID
  - `POST /kitties` — create
  - `PATCH /kitties/{id}` — partial update
  - `DELETE /kitties/{id}` — soft delete

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

## Features

- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers using the custom handler model.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.

## Prerequisites

- Go 1.22+

- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Go](https://go.dev/dl/): Go SDK and CLI

- Azure Account: An active Azure subscription for deploying the Function App.

- Cosmos DB NoSQL Account either deployed in Azure or [emulated](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql).

## Setup and Installation

1. Install Azure Functions Core Tools

    Follow the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-python#install-the-azure-functions-core-tools) to install Azure Function Core Tools based on your operating system.

2. Install Go SDK

    If you haven't already installed Go, you can do so by following the [official installation guide](https://go.dev/dl/).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

    To be able to run the project locally, set the environment variable values in the local.settings.json project file.

4. Run the API Locally

    ```console
    make run
    ```

    This command builds the Go binary and starts the local development server using the Azure Function Core Tools, where you can interact with your API endpoints.

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
    ├── httpApi
    └── main.go
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
