# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built as a single HTTP [Cloud Run function](https://cloud.google.com/functions/docs) (formerly Cloud Functions) with the [Functions Framework for Go](https://github.com/GoogleCloudPlatform/functions-framework-go). The function, registered as `api` in `function.go`, routes every request by its path, and `cmd/main.go` runs it locally.

The REST API exposes the following resources and operations:

- **`/kittenclaws`** (container: `kittenclaws`)
  - `GET /kittenclaws` — list
  - `GET /kittenclaws/{id}` — get by ID
  - `POST /kittenclaws` — create
  - `PATCH /kittenclaws/{id}` — partial update
  - `DELETE /kittenclaws/{id}` — soft delete

A health check is served at `GET /health` and answers `200 {"status":"ok"}`.

The deployed function requires IAM: callers need the Cloud Run Invoker role and send `Authorization: Bearer $(gcloud auth print-identity-token)`. The health check sits behind the same check, because the project exposes one function.

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

Records are stored with `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when set. Delete is a soft delete: it sets `isDeleted` to `true`.

### Logging

Logs are written with `log/slog`. Each request is logged once at info level with its method and path. Records below error level go to stdout; unexpected errors are logged at error level, with a stack trace, to stderr. Expected 4xx outcomes, and requests the client cancels by disconnecting, are not logged as errors.

### Storage containers

Each container is configured by its own setting. When the setting is unset, the container id is used as the Firestore collection name.

| Container | Resources | Setting |
|---|---|---|
| `kittenclaws` | KittenClaws | `FIRESTORE_COLLECTION_KITTENCLAWS` |

Resources that share a container share its records: there is no type discriminator, so every resource mapped to a container reads, lists, updates and deletes all records in it. Give resources separate containers unless they are meant to operate on the same data.

> **Setting renames:** each container now has its own setting. `FIRESTORE_COLLECTION` is replaced by `FIRESTORE_COLLECTION_<CONTAINER>`; the single-container setting is no longer read.

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

## Features

- GCP Cloud Run functions: One HTTP function, `api`, built with the Functions Framework for Go, serves every endpoint on Google Cloud's serverless platform.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.

## Prerequisites

- Go 1.27+

- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage Cloud Functions.

- [Go](https://go.dev/dl/): Go SDK and CLI

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project, or use the local emulator (see [Run locally against the emulator](#run-locally-against-the-emulator)).

## Setup and Installation

1. Install Google Cloud SDK

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud SDK based on your operating system.

2. Install Go SDK

    If you haven't already installed Go, you can do so by following the [official installation guide](https://go.dev/dl/).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

4. Set Environment Variables

    Set the following environment variables for local development:

    - `GCP_PROJECT_ID`: Your GCP project ID
    - `FIRESTORE_DATABASE`: Firestore database name (defaults to "(default)")
    - `FIRESTORE_COLLECTION_KITTENCLAWS`: Firestore collection for the `kittenclaws` container (defaults to `kittenclaws`)

5. Run the API Locally

    ```console
    make run
    ```

    This runs `FUNCTION_TARGET=api PORT=8080 go run ./cmd`: the Functions Framework serves the `api` function at every path on port 8080, for example `http://localhost:8080/kittenclaws`. To run against the Firestore emulator instead of a GCP project, see [Run locally against the emulator](#run-locally-against-the-emulator).

6. Deploy to GCP

    Deploy the `api` entry point as an HTTP Cloud Run function:

    ```console
    gcloud functions deploy kittenclaws-api \
      --gen2 \
      --runtime go127 \
      --region us-central1 \
      --source . \
      --entry-point api \
      --trigger-http \
      --no-allow-unauthenticated \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_KITTENCLAWS=kittenclaws
    ```

    The function's routes have no prefix, and callers need the Cloud Run Invoker role:

    ```console
    curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" \
      https://<function-url>/kittenclaws
    ```

7. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Run locally against the emulator

`docker-compose.yml` runs the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2; for Podman, add `COMPOSE="podman compose"` to each `make` command.

```console
make install
make emulator-up      # docker compose up -d --wait: returns once the emulator is healthy
make emulator-seed    # waits until the emulator answers (collections are created on first write); safe to re-run
make run-emulator     # the Functions Framework on http://localhost:8080 with the emulator settings
make emulator-down    # docker compose down -v: stops the emulator and discards its data
```

`make emulator-logs` follows the emulator's logs. `make emulator-seed` and `make run-emulator` export the settings in `.env.emulator`. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `.env.local`, which git ignores. The emulator keeps no data outside its container, so `make emulator-down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8085 | Firestore emulator (8080 in the container; 8085 on the host leaves 8080 to the Functions Framework) |

- With `FIRESTORE_EMULATOR_HOST` set, the Firestore client connects to the emulator without credentials. `GCP_PROJECT_ID` uses a `demo-` project id, which can never reach a real project.
- The emulator keeps data in memory, enforces no IAM or security rules for server SDKs and does not require composite indexes. Keep `FIRESTORE_DATABASE` at `(default)` locally.

Startup takes about 10 to 20 seconds; the healthcheck allows up to 2 minutes. `make emulator-seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`make emulator-up` fails or never turns healthy:** check `make emulator-logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** check that `make emulator-seed` passes and that the API was started with `make run-emulator`, which sets `FIRESTORE_EMULATOR_HOST`.

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
`function.go`) lives in one file per package.

```text
.
├── .github/workflows              # CI: format, vet, build, lint, unit tests and vulnerability scan
├── .thunderclient                 # Thunder Client requests and the localhost environment
├── .env.emulator                  # public settings for the local emulator
├── .golangci.yml
├── cmd
│   ├── bootstrap
│   │   └── main.go                # prepares the local emulator (make emulator-seed)
│   └── main.go                    # runs the function locally with the Functions Framework
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
├── docker-compose.yml             # local Firestore emulator
├── go.mod
├── Makefile
└── function.go                    # registers the "api" function and wires each resource
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
