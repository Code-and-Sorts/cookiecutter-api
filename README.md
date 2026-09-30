# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/api/cats`** (container: `animals`)
  - `GET /api/cats` — list
  - `GET /api/cats/{id}` — get by ID
  - `POST /api/cats` — create
  - `PATCH /api/cats/{id}` — partial update
  - `DELETE /api/cats/{id}` — soft delete
- **`/api/dogs`** (container: `animals`)
  - `GET /api/dogs` — list
  - `GET /api/dogs/{id}` — get by ID
  - `POST /api/dogs` — create
  - `PUT /api/dogs/{id}` — full replace
  - `DELETE /api/dogs/{id}` — soft delete
- **`/api/health`**
  - `GET /api/health` — health check

Paths are relative to the Function App host (`http://localhost:7071` when running locally); `/api` is the Azure Functions default route prefix.

Dependency management is handled using [Nuget](https://www.nuget.org/), ensuring a streamlined and consistent environment for managing Dotnet packages and their dependencies.

## API behaviour

Every response body is JSON (`Content-Type: application/json`), including errors.

| Case | Status | Body |
|---|---|---|
| create | 201 | the item |
| get, update, replace | 200 | the item |
| list | 200 | JSON array of items (`[]` when empty) |
| delete | 200 | `{"message": "<Name> with id <id> was deleted successfully."}` |
| health | 200 | `{"status": "ok"}` |
| invalid request body | 400 | `{"errorMessage": "<what is wrong>"}` |
| id not found, soft-deleted or not a UUID | 404 | `{"errorMessage": "<Name> with id <id> was not found."}` |
| anything unexpected | 500 | `{"errorMessage": "An unexpected error occurred."}` (the exception is logged with its stack trace) |

An item is exactly `{"id": "<uuid>", "name": "<string>"}`. Request bodies must be a JSON object: `name` is required on create (`POST`) and replace (`PUT`) and optional on update (`PATCH`), and when present it must be a non-empty JSON string (numbers and booleans are not converted). Any other field, including `id`, `isDeleted`, the timestamps, `createdBy` and `updatedBy`, is rejected with `400`, so clients can never set ids or system fields.

Writes (`POST`, `PATCH`, `PUT` and `DELETE`) record who made them from the optional `X-User-Id` request header (surrounding whitespace is trimmed; an empty or missing header means no user). Create stores it as `createdBy` and `updatedBy`, and later writes store it as `updatedBy`, removing any earlier `updatedBy` when the header is absent. A value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}` before the body is read. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

Each request's database work is bounded by a 5-second deadline (`Utils/RequestDeadline.cs`) and the database client has capped timeouts and retries, so an unreachable or failing database answers the generic `500` well inside the platform timeout. A request that hits the deadline returns `500`, but the write may still complete, so retrying a create can store a duplicate. A request the client cancels is logged at information level, not as an error.

List endpoints accept `?limit=<n>` (default `100`, at most `1000`); a missing or invalid value uses the default and a larger value is capped.

Requests that never reach the app are answered by the Azure Functions host: an unknown path, or a method a function is not registered for, returns `404` with an empty body.

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when they are set. Updates and replacements keep `createdTimestamp` and `createdBy` and refresh `updatedTimestamp` and `updatedBy`; delete is a soft delete that sets `isDeleted` to `true`.

## Storage containers

Each resource reads and writes the Cosmos DB container configured for its `container` id. The container names are read from these settings (see `local.settings.json`), falling back to the container id when a setting is missing:

| Container | Setting | Default value | Resources |
|---|---|---|---|
| `animals` | `CosmosDbContainerName_Animals` | `animals` | Cat, Dog |

The Cosmos DB connection string is read from `ConnectionStrings:CosmosDb` and the database name from `CosmosDbDatabaseName`. Containers must use `/id` as their partition key.

`CosmosDbConnectionMode` selects the Cosmos DB connection mode: `Direct` (the default when the setting is missing, and the best choice in Azure) or `Gateway`. `local.settings.json` sets it to `Gateway` because the [Linux Cosmos DB emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) only supports Gateway mode.

Resources that use the same container share its records: there is no type discriminator, so every resource on a shared container lists, reads, updates and deletes the same items.

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

Each resource has its own file in every layer, named after the resource: for example `CatController.cs`, `CatService.cs`, `CatRepository.cs`, `CatFunctions.cs` and their `ICat…` interfaces, DTO, entity, request and validation models, and unit tests.
Each repository extends `EntityRepository`, which holds the shared read, list, create, update and soft-delete logic and talks to the database through `IDocumentStore` (`CosmosDocumentStore`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
