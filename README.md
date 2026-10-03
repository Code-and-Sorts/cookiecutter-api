# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [Cloud Run functions](https://cloud.google.com/functions) and the [.NET Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-dotnet). A single HTTP function routes every request to the matching resource, allowing you to deploy and scale the API effortlessly in the cloud.

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
- **`/health`**
  - `GET /health` — health check

Paths are relative to the function URL (`http://localhost:8080` when running locally). Methods that are not enabled for a resource return `405 Method Not Allowed`, and unknown endpoints return `404 Not Found`.

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
| unknown path | 404 | `{"errorMessage": "Not found."}` |
| known path, method not enabled | 405 | `{"errorMessage": "Method not allowed."}` |
| anything unexpected | 500 | `{"errorMessage": "An unexpected error occurred."}` (the exception is logged with its stack trace) |

An item is exactly `{"id": "<uuid>", "name": "<string>"}`. Request bodies must be a JSON object: `name` is required on create (`POST`) and replace (`PUT`) and optional on update (`PATCH`), and when present it must be a non-empty JSON string (numbers and booleans are not converted). Any other field, including `id`, `isDeleted`, the timestamps, `createdBy` and `updatedBy`, is rejected with `400`, so clients can never set ids or system fields.

Writes (`POST`, `PATCH`, `PUT` and `DELETE`) record who made them from the optional `X-User-Id` request header (surrounding whitespace is trimmed; an empty or missing header means no user). Create stores it as `createdBy` and `updatedBy`, and later writes store it as `updatedBy`, removing any earlier `updatedBy` when the header is absent. A value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}` before the body is read. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

Each request's database work is bounded by a 5-second deadline (`Utils/RequestDeadline.cs`) and the database client has capped timeouts and retries, so an unreachable or failing database answers the generic `500` well inside the platform timeout. A request that hits the deadline returns `500`, but the write may still complete, so retrying a create can store a duplicate. A request the client cancels is logged at information level, not as an error.

List endpoints accept `?limit=<n>` (default `100`, at most `1000`); a missing or invalid value uses the default and a larger value is capped.

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when they are set. Updates and replacements keep `createdTimestamp` and `createdBy` and refresh `updatedTimestamp` and `updatedBy`; delete is a soft delete that sets `isDeleted` to `true`.

## Storage containers

Each resource reads and writes the Firestore collection configured for its `container` id. The collection names are read from these environment variables, falling back to the container id when a variable is not set:

| Container | Environment variable | Default value | Resources |
|---|---|---|---|
| `animals` | `FIRESTORE_COLLECTION_ANIMALS` | `animals` | Cat, Dog |

The Google Cloud project is read from `GCP_PROJECT_ID` (required) and the Firestore database from `FIRESTORE_DATABASE` (defaults to `(default)`).

Resources that use the same container share its records: there is no type discriminator, so every resource on a shared container lists, reads, updates and deletes the same items.

## Features

- Cloud Run functions: Utilizes Google Cloud's serverless platform to serve the API from a single HTTP function.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- Firestore: This project uses Firestore as its NoSQL database.

## Prerequisites

- Dotnet 10.x

- [Google Cloud CLI](https://cloud.google.com/sdk/docs/install): To deploy and manage Cloud Run functions.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- Google Cloud project: An active project with Firestore enabled, or the local Firestore emulator (see [Run locally against the emulator](#run-locally-against-the-emulator)).

## Setup and Installation

1. Install the Google Cloud CLI

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud CLI based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

    To be able to run the project locally, set `GCP_PROJECT_ID` (and the optional variables listed under [Storage containers](#storage-containers)) in your environment, and either sign in with `gcloud auth application-default login` or run against the local Firestore emulator (see [Run locally against the emulator](#run-locally-against-the-emulator)).

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the function locally with the .NET Functions Framework, where you can interact with your API endpoints.

5. Deploy

    ```console
    gcloud functions deploy kittenclaws --gen2 --runtime=dotnet10 --trigger-http --no-allow-unauthenticated --entry-point=KittenClaws.Api.Function --source=KittenClaws.Api --set-env-vars=GCP_PROJECT_ID=<project-id>
    ```

    The whole API is one HTTP function. The .NET Functions Framework names the entry point by its type, so `--entry-point` is the `KittenClaws.Api.Function` class, which routes every request by its path.

    The function is private: callers need the Cloud Run Invoker role and send `Authorization: Bearer $(gcloud auth print-identity-token)`. Because the whole API is one function, the health check sits behind the same IAM check.

    ```console
    curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" <function-url>/cats
    ```

6. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Run locally against the emulator

`docker-compose.yml` runs the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2; for Podman, add `COMPOSE="podman compose"` to each `make` command.

```console
make install
make emulator-up      # docker compose up -d --wait: returns once the emulator is healthy
make emulator-seed    # waits until the emulator answers (collections are created on first write); safe to re-run
make run-emulator     # dotnet run in KittenClaws.Api, on http://localhost:8080 with the emulator settings
make emulator-down    # docker compose down -v: stops the emulator and discards its data
```

`make emulator-logs` follows the emulator's logs. `make emulator-seed` runs `KittenClaws.Bootstrap`, a console project in the solution that reuses the API's client setup and store name settings. It and `make run-emulator` export the settings in `.env.emulator`. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `.env.local`, which git ignores. The emulator keeps no data outside its container, so `make emulator-down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8085 | Firestore emulator (8080 in the container; 8085 on the host leaves 8080 to the Functions Framework) |

- The Firestore client is built with `EmulatorDetection.EmulatorOrProduction`, so with `FIRESTORE_EMULATOR_HOST` set it connects to the emulator without credentials. `GCP_PROJECT_ID` uses a `demo-` project id, which can never reach a real project.
- The emulator keeps data in memory, enforces no IAM or security rules for server SDKs and does not require composite indexes. Keep `FIRESTORE_DATABASE` at `(default)` locally.

Startup takes about 10 to 20 seconds; the healthcheck allows up to 2 minutes. `make emulator-seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`make emulator-up` fails or never turns healthy:** check `make emulator-logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** check that `make emulator-seed` passes and that the API was started with `make run-emulator`, which sets `FIRESTORE_EMULATOR_HOST`.

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
│   ├── Handlers
│   ├── Interfaces
│   ├── Models
│   │   ├── Dtos
│   │   ├── Entities
│   │   └── Schemas
│   ├── Repositories
│   ├── Services
│   └── Utils
├── KittenClaws.Api.Tests.Unit
│   ├── Controllers
│   ├── Functions
│   ├── Handlers
│   ├── Repositories
│   ├── Services
│   ├── Utils
│   └── tests
├── KittenClaws.Bootstrap               - Prepares the local emulator (make emulator-seed)
├── .env.emulator                       - Public settings for the local emulator
└── docker-compose.yml                  - Local Firestore emulator
```

Each resource has its own file in every layer, named after the resource: for example `CatController.cs`, `CatService.cs`, `CatRepository.cs`, `CatHandler.cs` and their `ICat…` interfaces, DTO, entity, request and validation models, and unit tests. `Function.cs` routes each request to the handler whose endpoint matches the first path segment.
Each repository extends `EntityRepository`, which holds the shared read, list, create, update and soft-delete logic and talks to the database through `IDocumentStore` (`FirestoreDocumentStore`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
