# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/api/kittenclaws`** (container: `kittenclaws`)
  - `GET /api/kittenclaws` — list
  - `GET /api/kittenclaws/{id}` — get by ID
  - `POST /api/kittenclaws` — create
  - `PATCH /api/kittenclaws/{id}` — partial update
  - `DELETE /api/kittenclaws/{id}` — soft delete
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

Request bodies and responses follow the [data model](#data-model).

Writes (`POST`, `PATCH`, `PUT` and `DELETE`) record who made them from the optional `X-User-Id` request header (surrounding whitespace is trimmed; an empty or missing header means no user). Create stores it as `createdBy` and `updatedBy`, and later writes store it as `updatedBy`, removing any earlier `updatedBy` when the header is absent. A value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}` before the body is read. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

Each request's database work is bounded by a 5-second deadline (`Utils/RequestDeadline.cs`) and the database client has capped timeouts and retries, so an unreachable or failing database answers the generic `500` well inside the platform timeout. A request that hits the deadline returns `500`, but the write may still complete, so retrying a create can store a duplicate. A request the client cancels is logged at information level, not as an error.

List endpoints accept `?limit=<n>` (default `100`, at most `1000`); a missing or invalid value uses the default and a larger value is capped.

Requests that never reach the app are answered by the Azure Functions host: an unknown path, or a method a function is not registered for, returns `404` with an empty body.

Updates and replacements keep `createdTimestamp` and `createdBy` and refresh `updatedTimestamp` and `updatedBy`; delete is a soft delete that sets `isDeleted` to `true`.

## Data model

Every resource stores the built-in base fields, the fields `base_model` adds and its own `fields`. They
come from the Copier answers in `.copier-answers.yml`: change them there and run `copier update`, so all
layers (models, validation, storage and tests) are regenerated together.

The built-in fields are set by the server and never accepted in a request body: `id` (a UUID, returned),
`isDeleted` (soft delete), `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for
example `2026-09-29T22:49:26.625Z`; a create reads the clock once for both), and `createdBy`/`updatedBy`
(the `X-User-Id` header, stored only when it is sent). Only `id` is returned.

`KittenClaws` (`/kittenclaws`) fields:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned | Description |
|---|---|---|---|---|---|---|---|---|
| `name` | string | yes | no | none | min_length: 1 | POST, PATCH | yes | none |

- A response holds `id` and every field that is not hidden, with `null` for a field without a value. A
  list is a JSON array (`[]` when empty).
- A request body must be a JSON object holding only the fields its operation accepts; any other field,
  including `id` and the built-in fields, is a 400. Values are never converted: `"1"` is not an integer.
- Create (POST) and replace (PUT) give a field left out its default (`$now`, `$today` and `$uuid` are
  evaluated on every write) or no value; a required field without a default must be sent. Replace keeps
  the fields it does not accept, such as immutable ones, at their stored values.
- Update (PATCH) changes only the fields sent; `null` clears a nullable field.
- A field without a value is not stored. Date-times are stored in UTC with milliseconds whatever offset
  was sent, dates as `YYYY-MM-DD`. Reading a record that lacks a field returns its static default
  (`null` for a nullable field).
- Integers must lie within ±9007199254740991, so every language and JSON parser reads them exactly.
  Patterns match anywhere in the value unless anchored with `^` and `$`; `email` and `uri` check the
  value's shape only.


## Storage containers

Each resource reads and writes the Cosmos DB container configured for its `container` id. The container names are read from these settings (see `local.settings.json`), falling back to the container id when a setting is missing:

| Container | Setting | Default value | Resources |
|---|---|---|---|
| `kittenclaws` | `CosmosDbContainerName_Kittenclaws` | `kittenclaws` | KittenClaws |

The Cosmos DB connection string is read from `ConnectionStrings:CosmosDb` (`ConnectionStrings__CosmosDb` in `local.settings.json` `Values`, so a value already in the environment, such as the emulator's, takes precedence) and the database name from `CosmosDbDatabaseName`. Containers must use `/id` as their partition key.

`CosmosDbConnectionMode` selects the Cosmos DB connection mode: `Direct` (the default when the setting is missing, and the best choice in Azure) or `Gateway`. `local.settings.json` sets it to `Gateway` because the [Linux Cosmos DB emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) only supports Gateway mode. `CosmosDbEmulator=true` is for local development only (see [Run locally against the emulator](#run-locally-against-the-emulator)).

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

- Cosmos DB NoSQL Account either deployed in Azure or emulated locally (see [Run locally against the emulator](#run-locally-against-the-emulator)).

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

## Run locally against the emulator

`docker-compose.yml` runs the [Azure Cosmos DB emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2 and Azure Functions Core Tools; for Podman, add `COMPOSE="podman compose"` to each `make` command.

```console
make install
make emulator-up      # docker compose up -d --wait: returns once the emulator is healthy
make emulator-seed    # creates the database and one container per storage container; safe to re-run
make run-emulator     # func start in KittenClaws.Api with the emulator settings
make emulator-down    # docker compose down -v: stops the emulator and discards its data
```

`make emulator-logs` follows the emulator's logs. `make emulator-seed` runs `KittenClaws.Bootstrap`, a console project in the solution that reuses the API's client setup and store name settings. It and `make run-emulator` export the settings in `.env.emulator`, which take precedence over `local.settings.json`: Core Tools skips any setting already in the environment, and `Program.cs` reads environment variables after `local.settings.json`. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `local.settings.json` or `.env.local`, which git ignores. The emulator keeps no data outside its container, so `make emulator-down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8081 | Cosmos DB gateway (`http://localhost:8081/`) |
| 1234 | Data Explorer: open `http://localhost:1234` to browse databases and items |

- The image is the Linux [vNext emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) (preview). It serves plain HTTP, so there is no certificate to trust, and it runs natively on x64 and arm64, including Apple Silicon. Partition key `/id`, conditional patch (soft delete), `OFFSET`/`LIMIT` and parameterized queries all work against it.
- The `AccountKey` in `ConnectionStrings__CosmosDb` is the emulator's well-known account key, published by Microsoft; it is not a secret and only works against the emulator.
- `CosmosDbEmulator=true` switches the client to Gateway mode and `LimitToEndpoint`, so it keeps using the connection string's endpoint instead of the address the emulator advertises, and, for an `https://` endpoint only, skips certificate verification. Never set it outside local development; when it is unset or false the client is configured exactly as in production.
- To use the older HTTPS-only emulator (`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest`) instead, swap the image in `docker-compose.yml` (its readiness probe is `https://localhost:8081/_explorer/emulator.pem`) and set the connection string's `AccountEndpoint` to `https://localhost:8081/`. That image runs on x64 only (not on Apple Silicon) and takes 1 to 3 minutes to start.

Startup takes about 10 to 60 seconds; the healthcheck allows up to 4 minutes for slow machines and CI runners. `make emulator-seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`make emulator-up` fails or never turns healthy:** check `make emulator-logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** run `make emulator-seed`; the database and containers do not exist until it has run, and `make emulator-down` deletes them.
- **A missing container returns 404 instead of 500:** the emulator reports a missing container without the sub-status a Cosmos DB account sends; run `make emulator-seed`.

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
├── KittenClaws.Api.Tests.Unit
│   ├── Controllers
│   ├── Functions
│   ├── Models
│   ├── Repositories
│   ├── Services
│   ├── Utils
│   └── tests
├── KittenClaws.Bootstrap               - Prepares the local emulator (make emulator-seed)
├── .env.emulator                       - Public settings for the local emulator
└── docker-compose.yml                  - Local Cosmos DB emulator
```

Each resource has its own file in every layer, named after the resource: for example `KittenClawsController.cs`, `KittenClawsService.cs`, `KittenClawsRepository.cs`, `KittenClawsFunctions.cs` and their `IKittenClaws…` interfaces, DTO, entity, request and validation models, and unit tests. The models extend the types rendered from `base_model`: `BaseEntity`, `BaseResponse` and, for each request body in use, `BaseCreateRequest`, `BaseReplaceRequest` or `BaseUpdateRequest` (validated in `Models/Schemas/BaseValidation.cs`).
Each repository extends `EntityRepository`, which holds the shared read, list, create, update and soft-delete logic and talks to the database through `IDocumentStore` (`CosmosDocumentStore`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
