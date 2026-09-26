# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/api/kitties`** (container: `kitties`)
  - `GET /api/kitties` — list
  - `GET /api/kitties/{id}` — get by ID
  - `POST /api/kitties` — create
  - `PATCH /api/kitties/{id}` — partial update
  - `DELETE /api/kitties/{id}` — soft delete
- **`/api/health`**
  - `GET /api/health` — health check

Paths are relative to the Function App host (`http://localhost:7071` when running locally); `/api` is the Azure Functions default route prefix.

Dependency management is handled using [Nuget](https://www.nuget.org/), ensuring a streamlined and consistent environment for managing Dotnet packages and their dependencies.

## Storage containers

Each resource reads and writes the Cosmos DB container configured for its `container` id. The container names are read from these settings (see `local.settings.json`), falling back to the container id when a setting is missing:

| Container | Setting | Default value | Resources |
|---|---|---|---|
| `kitties` | `CosmosDbContainerName_Kitties` | `kitties` | KittenClaws |

The Cosmos DB connection string is read from `ConnectionStrings:CosmosDb` and the database name from `CosmosDbDatabaseName`. Containers must use `/id` as their partition key.

Resources that use the same container share its records: there is no type discriminator, so every resource on a shared container lists, reads, updates and deletes the same items.

> **Upgrading from a single-resource project:** the storage setting name now includes the container id. `CosmosDbContainerName` became `CosmosDbContainerName_<Container>` (see the table above), so rename it in every deployed environment.

## Features

- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.

## Prerequisites

- Dotnet 10.x

- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- Azure Account: An active Azure subscription for deploying the Function App. On Linux, .NET 10 apps must run on the [Flex Consumption](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan) plan (or Premium/Dedicated); the Linux Consumption plan does not support .NET 10.

- Cosmos DB NoSQL Account either deployed in Azure or [emulated](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql).

## Setup and Installation

1. Install Azure Functions Core Tools

    Follow the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-python#install-the-azure-functions-core-tools) to install Azure Function Core Tools based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

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

    This command starts the local development server using the Azure Function Core Tools, where you can interact with your API endpoints.

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
├── KittenClaws.Api
│   ├── Controllers
│   ├── Functions
│   ├── Interfaces
│   ├── Models
│   │   ├── Dtos
│   │   ├── Entities
│   │   └── Schemas
│   ├── Properties
│   ├── Repositories
│   ├── Services
│   └── Utils
└── KittenClaws.Api.Tests.Unit
    ├── Controllers
    ├── Functions
    ├── Repositories
    ├── Services
    ├── Utils
    └── tests
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
