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

## Storage containers

Each resource reads and writes the Firestore collection configured for its `container` id. The collection names are read from these environment variables, falling back to the container id when a variable is not set:

| Container | Environment variable | Default value | Resources |
|---|---|---|---|
| `animals` | `FIRESTORE_COLLECTION_ANIMALS` | `animals` | Cat, Dog |

The Google Cloud project is read from `GCP_PROJECT_ID` (required) and the Firestore database from `FIRESTORE_DATABASE` (defaults to `(default)`).

Resources that use the same container share its records: there is no type discriminator, so every resource on a shared container lists, reads, updates and deletes the same items.

> **Upgrading from a single-resource project:** the storage setting name now includes the container id. `FIRESTORE_COLLECTION` became `FIRESTORE_COLLECTION_<CONTAINER>` (see the table above), so rename it in every deployed environment.

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
    gcloud functions deploy kitties --gen2 --runtime=dotnet10 --trigger-http --entry-point=KittenClaws.Api.Function --source=KittenClaws.Api --set-env-vars=GCP_PROJECT_ID=<project-id>
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
    ├── Repositories
    ├── Services
    ├── Utils
    └── tests
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
