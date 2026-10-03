# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) with a [custom handler](https://learn.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/api/kittenclaws`** (container: `kittenclaws`)
  - `GET /api/kittenclaws` — list
  - `GET /api/kittenclaws/{id}` — get by ID
  - `POST /api/kittenclaws` — create
  - `PATCH /api/kittenclaws/{id}` — partial update
  - `DELETE /api/kittenclaws/{id}` — soft delete

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

Request bodies and responses follow the [data model](#data-model). List takes an optional `?limit=` (default 100, at most 1000; invalid values fall back to the default).

Send an optional `X-User-Id` header on create, update, replace and delete to record who made the change: create stores it as `createdBy` and `updatedBy`, later writes set `updatedBy` (or remove it when the header is absent or blank) and never change `createdBy`. Surrounding whitespace is trimmed, and a value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}`. Reads ignore the header, and responses never include these fields. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

A method a resource does not enable never reaches the handler when the method is missing from every function registered for that path: the Functions host answers it with its own 404.

Delete is a soft delete: it sets `isDeleted` to `true`.

### Logging

Logs are written with `log/slog`. Each request is logged once at info level with its method and path. Records below error level go to stdout; unexpected errors are logged at error level, with a stack trace, to stderr. Expected 4xx outcomes, and requests the client cancels by disconnecting, are not logged as errors.

### Storage containers

Each container is configured by its own setting. When the setting is unset, the container id is used as the Cosmos DB container name.

| Container | Resources | Setting |
|---|---|---|
| `kittenclaws` | KittenClaws | `CosmosDbContainerName_Kittenclaws` |

Resources that share a container share its records: there is no type discriminator, so every resource mapped to a container reads, lists, updates and deletes all records in it. Give resources separate containers unless they are meant to operate on the same data.

> **Setting renames:** each container now has its own setting. `CosmosDbContainerName` is replaced by `CosmosDbContainerName_<Container>`; the single-container setting is no longer read.

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

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

- Cosmos DB NoSQL Account either deployed in Azure or emulated locally (see [Run locally against the emulator](#run-locally-against-the-emulator)).

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

## Run locally against the emulator

`docker-compose.yml` runs the [Azure Cosmos DB emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2 and Azure Functions Core Tools; for Podman, add `COMPOSE="podman compose"` to each `make` command.

```console
make install
make emulator-up      # docker compose up -d --wait: returns once the emulator is healthy
make emulator-seed    # creates the database and one container per storage container; safe to re-run
make run-emulator     # go build, then func start with the emulator settings
make emulator-down    # docker compose down -v: stops the emulator and discards its data
```

`make emulator-logs` follows the emulator's logs. `make emulator-seed` and `make run-emulator` export the settings in `.env.emulator`, which take precedence over `local.settings.json` (Core Tools skips any setting already in the environment). `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `local.settings.json` or `.env.local`, which git ignores. The emulator keeps no data outside its container, so `make emulator-down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8081 | Cosmos DB gateway (`http://localhost:8081/`) |
| 1234 | Data Explorer: open `http://localhost:1234` to browse databases and items |

- The image is the Linux [vNext emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) (preview). It serves plain HTTP, so there is no certificate to trust, and it runs natively on x64 and arm64, including Apple Silicon. Partition key `/id`, conditional patch (soft delete), `OFFSET`/`LIMIT` and parameterized queries all work against it.
- `CosmosDbKey` is the emulator's well-known account key, published by Microsoft; it is not a secret and only works against the emulator.
- `CosmosDbEmulator=true` makes the client skip certificate verification for an `https://` endpoint; with the plain-HTTP vNext image it changes nothing. The Go SDK cannot turn off endpoint discovery, and the emulator advertises `http://localhost:8081/`, so run the API on the machine that runs the emulator. Never set it outside local development; when it is unset or false the client is configured exactly as in production.
- To use the older HTTPS-only emulator (`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest`) instead, swap the image in `docker-compose.yml` (its readiness probe is `https://localhost:8081/_explorer/emulator.pem`) and set `CosmosDbEndpoint=https://localhost:8081/`. That image runs on x64 only (not on Apple Silicon) and takes 1 to 3 minutes to start.

Startup takes about 10 to 60 seconds; the healthcheck allows up to 4 minutes for slow machines and CI runners. `make emulator-seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`make emulator-up` fails or never turns healthy:** check `make emulator-logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** run `make emulator-seed`; the database and containers do not exist until it has run, and `make emulator-down` deletes them.
- **A missing container returns 404 instead of 500:** the emulator reports a missing container without the sub-status a Cosmos DB account sends, so `IsItemNotFound` cannot tell it from a missing item; run `make emulator-seed`.

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
├── .env.emulator                  # public settings for the local emulator
├── .golangci.yml
├── cmd
│   ├── bootstrap
│   │   └── main.go                # prepares the local emulator (make emulator-seed)
├── controllers
│   ├── schemas                    # request body JSON schemas, one per resource and operation
│   ├── kitten_claws_controller.go
│   ├── kitten_claws_controller_test.go
│   ├── ids.go                     # only UUID ids reach the database
│   ├── ids_test.go
│   ├── pagination.go
│   ├── pagination_test.go
│   └── schemas.go                 # embeds the request schemas
├── handlers
│   ├── kitten_claws_handler.go
│   ├── kitten_claws_handler_test.go
│   ├── health_handler.go
│   ├── health_handler_test.go
│   ├── router.go                  # request log and deadline, JSON responses and 404, 405 and 500 errors
│   └── router_test.go
├── models                         # each resource's entity, DTO and request types
│   ├── kitten_claws_model.go
│   ├── entity.go                  # BaseEntity
│   ├── entity_test.go
│   └── errors.go
├── repositories
│   ├── kitten_claws_repository.go
│   ├── cosmos.go                  # client options; tells a missing item from a missing container
│   ├── cosmos_test.go
│   └── store.go                   # generic database access and the shared update, replace and soft delete
├── services
│   ├── kitten_claws_service.go
│   ├── kitten_claws_service_test.go
│   ├── schema_validator.go
│   └── schema_validator_test.go
├── utils
│   ├── error_detector.go          # maps errors to JSON error responses
│   ├── error_detector_test.go
│   ├── env.go                     # setting or default
│   ├── env_test.go
│   ├── logger.go                  # info logs to stdout, errors to stderr
│   └── logger_test.go
├── kittenClawsApi
│   └── function.json
├── healthApi
│   └── function.json
├── host.json
├── local.settings.json
├── docker-compose.yml             # local Cosmos DB emulator
├── go.mod
├── Makefile
└── main.go                        # wires each resource's repository, service, controller and routes
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
