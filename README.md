# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) with a [custom handler](https://learn.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

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

A health check is served at `GET /api/health` and answers `200 {"status":"ok"}` without a function key.

Routes are served under the Functions host's default `/api` prefix. The resource functions use function-level keys (pass `?code=<key>` or the `x-functions-key` header when deployed); the health check function is anonymous.

### Responses

Every response, including errors, is JSON (`Content-Type: application/json`).

| Case | Status | Body |
|---|---|---|
| Create | 201 | the item |
| Get, update, replace | 200 | the item |
| List | 200 | an array of items (`[]` when there are none) |
| Delete | 200 | `{"message": "<Name> with id <id> was deleted successfully."}` |
| Invalid body | 400 | `{"errorMessage": "<what is wrong>"}` |
| Id not found, soft-deleted or not a UUID | 404 | `{"errorMessage": "<Name> with id <id> was not found."}` |
| Unknown path | 404 | `{"errorMessage": "Not found."}` |
| Known path, method not enabled | 405 | `{"errorMessage": "Method not allowed."}` |
| Anything unexpected | 500 | `{"errorMessage": "An unexpected error occurred."}` |

Each request has an 8-second deadline (`handlers.RequestTimeout`) that covers every database call and the SDK's retries, so an unreachable or failing database answers with the 500 above instead of hanging until the platform times out. A request that hits the deadline returns 500, but the write may still complete, so retrying a create can store a duplicate.

An item is exactly `{"id": "<uuid>", "name": "<string>"}`. Request bodies must be JSON objects: create (POST) and replace (PUT) require a non-empty string `name`; update (PATCH) accepts an optional non-empty string `name`. Any other field, including `id`, `isDeleted`, the timestamps, `createdBy` and `updatedBy`, is rejected with a 400. List takes an optional `?limit=` (default 100, at most 1000; invalid values fall back to the default).

Send an optional `X-User-Id` header on create, update, replace and delete to record who made the change: create stores it as `createdBy` and `updatedBy`, later writes set `updatedBy` (or remove it when the header is absent or blank) and never change `createdBy`. Surrounding whitespace is trimmed, and a value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}`. Reads ignore the header, and responses never include these fields. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

A method a resource does not enable never reaches the handler when the method is missing from every function registered for that path: the Functions host answers it with its own 404.

Records are stored with `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when set. Delete is a soft delete: it sets `isDeleted` to `true`.

### Logging

Logs are written with `log/slog`. Each request is logged once at info level with its method and path. Records below error level go to stdout; unexpected errors are logged at error level, with a stack trace, to stderr. Expected 4xx outcomes, and requests the client cancels by disconnecting, are not logged as errors.

### Storage containers

Each container is configured by its own setting. When the setting is unset, the container id is used as the Cosmos DB container name.

| Container | Resources | Setting |
|---|---|---|
| `animals` | Cat, Dog | `CosmosDbContainerName_Animals` |

Resources that share a container share its records: there is no type discriminator, so every resource mapped to a container reads, lists, updates and deletes all records in it. Give resources separate containers unless they are meant to operate on the same data.

> **Setting renames:** each container now has its own setting. `CosmosDbContainerName` is replaced by `CosmosDbContainerName_<Container>`; the single-container setting is no longer read.

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

## Features

- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers using the custom handler model.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.

## Prerequisites

- Go 1.27+

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

Every resource has its own file in each layer. Shared code (routing, list pagination, id
checks, the schema validator, the generic database store, the base entity, error types, logging and the wiring in
`main.go`) lives in one file per package.

```text
.
├── .github/workflows              # CI: format, vet, build, lint, unit tests and vulnerability scan
├── .thunderclient                 # Thunder Client requests and the localhost environment
├── .golangci.yml
├── controllers
│   ├── schemas                    # request body JSON schemas, one per resource and operation
│   ├── cat_controller.go
│   ├── cat_controller_test.go
│   ├── dog_controller.go
│   ├── dog_controller_test.go
│   ├── ids.go                     # only UUID ids reach the database
│   ├── ids_test.go
│   ├── pagination.go
│   ├── pagination_test.go
│   └── schemas.go                 # embeds the request schemas
├── handlers
│   ├── cat_handler.go
│   ├── cat_handler_test.go
│   ├── dog_handler.go
│   ├── dog_handler_test.go
│   ├── health_handler.go
│   ├── health_handler_test.go
│   ├── router.go                  # request log and deadline, JSON responses and 404, 405 and 500 errors
│   └── router_test.go
├── models                         # each resource's entity, DTO and request types
│   ├── cat_model.go
│   ├── dog_model.go
│   ├── entity.go                  # BaseEntity
│   ├── entity_test.go
│   └── errors.go
├── repositories
│   ├── cat_repository.go
│   ├── dog_repository.go
│   ├── cosmos.go                  # tells a missing item from a missing container
│   ├── cosmos_test.go
│   └── store.go                   # generic database access and the shared update, replace and soft delete
├── services
│   ├── cat_service.go
│   ├── cat_service_test.go
│   ├── dog_service.go
│   ├── dog_service_test.go
│   ├── schema_validator.go
│   └── schema_validator_test.go
├── utils
│   ├── error_detector.go          # maps errors to JSON error responses
│   ├── error_detector_test.go
│   ├── env.go                     # setting or default
│   ├── env_test.go
│   ├── logger.go                  # info logs to stdout, errors to stderr
│   └── logger_test.go
├── catApi
│   └── function.json
├── dogApi
│   └── function.json
├── healthApi
│   └── function.json
├── host.json
├── local.settings.json
├── go.mod
├── Makefile
└── main.go                        # wires each resource's repository, service, controller and routes
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
