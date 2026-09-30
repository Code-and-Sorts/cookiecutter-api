# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [Cloud Run functions](https://cloud.google.com/functions) and the [.NET Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-dotnet). A single HTTP function routes every request to the matching resource, allowing you to deploy and scale the API effortlessly in the cloud.

The REST API exposes the following resources and operations:

- **`/kittenclaws`** (container: `kittenclaws`)
  - `GET /kittenclaws` — list
  - `GET /kittenclaws/{id}` — get by ID
  - `POST /kittenclaws` — create
  - `PATCH /kittenclaws/{id}` — partial update
  - `DELETE /kittenclaws/{id}` — soft delete
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
| `kittenclaws` | `FIRESTORE_COLLECTION_KITTENCLAWS` | `kittenclaws` | KittenClaws |

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

- Google Cloud project: An active project with Firestore enabled, or the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator).

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

    To be able to run the project locally, set `GCP_PROJECT_ID` (and the optional variables listed under [Storage containers](#storage-containers)) in your environment, and either sign in with `gcloud auth application-default login` or point `FIRESTORE_EMULATOR_HOST` at a running [Firestore emulator](https://cloud.google.com/firestore/docs/emulator).

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
    curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" <function-url>/kittenclaws
    ```

6. Thunderclient

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
│   ├── Handlers
│   ├── Interfaces
│   ├── Models
│   │   ├── Dtos
│   │   ├── Entities
│   │   └── Schemas
│   ├── Repositories
│   ├── Services
│   └── Utils
└── KittenClaws.Api.Tests.Unit
    ├── Controllers
    ├── Functions
    ├── Handlers
    ├── Repositories
    ├── Services
    ├── Utils
    └── tests
```

Each resource has its own file in every layer, named after the resource: for example `KittenClawsController.cs`, `KittenClawsService.cs`, `KittenClawsRepository.cs`, `KittenClawsHandler.cs` and their `IKittenClaws…` interfaces, DTO, entity, request and validation models, and unit tests. `Function.cs` routes each request to the handler whose endpoint matches the first path segment.
Each repository extends `EntityRepository`, which holds the shared read, list, create, update and soft-delete logic and talks to the database through `IDocumentStore` (`FirestoreDocumentStore`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
