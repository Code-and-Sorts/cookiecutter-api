# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

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

Dependency management is handled using [Nuget](https://www.nuget.org/), ensuring a streamlined and consistent environment for managing Dotnet packages and their dependencies.

## Features

## Prerequisites

- Dotnet 8.x

## Setup and Installation

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
├── cookiecutter-template-dotnet
    ├── KittenClaws.Api
    │   ├── Controllers
    │   ├── Functions
    │   ├── Interfaces
    │   ├── Repositories
    │   ├── Services
    │   ├── models
    │   │   ├── Dtos
    │   │   ├── Entities
    │   │   └── schemas
    │   └── utils
    └── KittenClaws.Api.Tests.Unit
        ├── Controllers
        ├── Functions
        ├── Repositories
        ├── Services
        └── utils
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
