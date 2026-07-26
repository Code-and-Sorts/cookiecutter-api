# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built using [Google Cloud Functions](https://cloud.google.com/functions/docs). The API leverages GCP's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered function serves as the entry point for the API, providing a seamless way to handle client requests.

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

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

## Features

- GCP Cloud Functions: Utilizes Google Cloud's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.

## Prerequisites

- Go 1.22+

- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage Cloud Functions.

- [Go](https://go.dev/dl/): Go SDK and CLI

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project.

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
    - `FIRESTORE_COLLECTION`: Firestore collection name

5. Run the API Locally

    ```console
    make run
    ```

    This command builds the Go binary and starts the local development server on port 8080, where you can interact with your API endpoints.

6. Deploy to GCP

    Build and deploy to Cloud Run (recommended for Go HTTP servers):

    ```console
    gcloud run deploy kitties-api \
      --source . \
      --region us-central1 \
      --allow-unauthenticated \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION=kitties
    ```

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

```text
├── cookiecutter-template-go
    ├── controllers
    ├── services
    ├── repositories
    ├── models
    ├── utils
    └── main.go
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
