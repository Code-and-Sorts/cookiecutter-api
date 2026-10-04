# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Go-based REST API built using [AWS Lambda](https://docs.aws.amazon.com/lambda/) with [API Gateway](https://docs.aws.amazon.com/apigateway/). The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/) framework is used for local development and deployment.

The REST API exposes the following resources and operations:

- **`/kittenclaws`** (container: `kittenclaws`)
  - `GET /kittenclaws` — list
  - `GET /kittenclaws/{id}` — get by ID
  - `POST /kittenclaws` — create
  - `PATCH /kittenclaws/{id}` — partial update
  - `DELETE /kittenclaws/{id}` — soft delete

A health check is served at `GET /health` and answers `200 {"status":"ok"}` without an API key.

API Gateway passes the item id as the `{id}` path parameter (for example `/kittenclaws/{id}` in `template.yaml`). Every route except `/health` requires an API key sent as `x-api-key: <value>`; step 5 of [Setup and Installation](#setup-and-installation) shows how to read it after deploying. `sam local start-api` does not enforce API keys. An API key identifies a caller but is not strong authentication; for that, add an IAM, Cognito or Lambda authorizer.

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

An item is the stored record without `isDeleted`:

```json
{
  "id": "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c",
  "name": "<string>",
  "createdTimestamp": "2026-09-29T22:49:26.625Z",
  "createdBy": "alice",
  "updatedTimestamp": "2026-09-30T08:12:03.041Z",
  "updatedBy": "bob"
}
```

`createdBy` and `updatedBy` appear only when the record has them. A create returns equal `createdTimestamp` and `updatedTimestamp`; update and replace return the new `updatedTimestamp`, keep the created fields, and include `updatedBy` only when that request sent `X-User-Id`.

Request bodies must be JSON objects: create (POST) and replace (PUT) require a non-empty string `name`; update (PATCH) accepts an optional non-empty string `name`. Any other field, including `id`, `isDeleted`, the timestamps, `createdBy` and `updatedBy`, is rejected with a 400. List takes an optional `?limit=` (default 100, at most 1000; invalid values fall back to the default).

Send an optional `X-User-Id` header on create, update, replace and delete to record who made the change: create stores it as `createdBy` and `updatedBy`, later writes set `updatedBy` (or remove it when the header is absent or blank) and never change `createdBy`. Surrounding whitespace is trimmed, and a value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}`. Reads ignore the header. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

API Gateway answers a path or method that `template.yaml` does not map with its own `403 {"message":"Missing Authentication Token"}` before the Lambda function runs; the JSON 404 and 405 responses above apply to requests that reach the function.

Records are stored with `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when set. Delete is a soft delete: it sets `isDeleted` to `true`.

### Logging

Logs are written with `log/slog`. Each request is logged once at info level with its method and path. Records below error level go to stdout; unexpected errors are logged at error level, with a stack trace, to stderr. Expected 4xx outcomes, and requests the client cancels by disconnecting, are not logged as errors.

### Storage containers

Each container is configured by its own setting. When the setting is unset, the container id is used as the DynamoDB table name.

| Container | Resources | Setting |
|---|---|---|
| `kittenclaws` | KittenClaws | `DYNAMODB_TABLE_NAME_KITTENCLAWS` |

Resources that share a container share its records: there is no type discriminator, so every resource mapped to a container reads, lists, updates and deletes all records in it. Give resources separate containers unless they are meant to operate on the same data.

> **Setting renames:** each container now has its own setting. `DYNAMODB_TABLE_NAME` is replaced by `DYNAMODB_TABLE_NAME_<CONTAINER>`; the single-container setting is no longer read.

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

## Features

- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway integration.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- DynamoDB: This project uses DynamoDB for NoSQL data storage.

## Prerequisites

- Go 1.27+

- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To build and run the Lambda functions locally.

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html): To deploy and manage AWS resources.

- [Go](https://go.dev/dl/): Go SDK and CLI

- AWS Account: An active AWS account for deploying the Lambda function.

- DynamoDB table either deployed in AWS or run locally using DynamoDB Local (see [Run locally against the emulator](#run-locally-against-the-emulator)).

## Setup and Installation

1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install the AWS SAM CLI based on your operating system.

2. Install Go SDK

    If you haven't already installed Go, you can do so by following the [official installation guide](https://go.dev/dl/).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command builds the Go binary using SAM and starts the local API Gateway, where you can interact with your API endpoints. The local API does not enforce API keys.

5. Deploy to AWS

    ```console
    sam build
    sam deploy --guided
    ```

    SAM creates an API key and usage plan for the API. Read the key's value from the `KittenClawsApiKeyId` stack output and send it in an `x-api-key` header:

    ```console
    aws apigateway get-api-key --api-key <ApiKeyId> --include-value --query value --output text
    curl -H "x-api-key: <value>" https://<api-id>.execute-api.<region>.amazonaws.com/Prod/kittenclaws
    ```

6. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Run locally against the emulator

`docker-compose.yml` runs [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2 and the SAM CLI; for Podman, add `COMPOSE="podman compose"` to each `make` command.

```console
make install
make emulator-up      # docker compose up -d --wait: returns once the emulator is healthy
make emulator-seed    # creates one table per storage container; safe to re-run
make run-emulator     # sam build, then sam local start-api on http://127.0.0.1:3000 with the emulator settings
make emulator-down    # docker compose down -v: stops the emulator and discards its data
```

`make emulator-logs` follows the emulator's logs. `make emulator-seed` and `make run-emulator` export the settings in `.env.emulator`. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `.env.local`, which git ignores. The emulator keeps no data outside its container, so `make emulator-down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8000 | DynamoDB Local (`-inMemory -sharedDb`, so tables do not depend on the region or access key) |

- `make emulator-seed` runs on your machine and reaches DynamoDB Local at `http://localhost:8000` (`.env.emulator`). `sam local start-api` runs the function in a container on the `kittenclaws-emulator` Docker network, so `env.emulator.json` points it at `http://dynamodb:8000` and sets the table names, which `sam local` would otherwise take from the template's logical ids. `--warm-containers LAZY` keeps each function's container running between requests and restarts it when the build changes.
- `template.yaml` declares `AWS_ENDPOINT_URL_DYNAMODB` only so `sam local` can set it; deployed stacks leave it out unless you pass the `DynamoDbEndpoint` parameter. Every AWS SDK reads the variable natively.
- `make emulator-seed` and `make run-emulator` use dummy credentials, which replace your own for these commands. DynamoDB Local 3 accepts only letters and digits in an access key id.

Startup takes a few seconds; the healthcheck allows up to 2 minutes. `make emulator-seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`make emulator-up` fails or never turns healthy:** check `make emulator-logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** run `make emulator-seed`; the tables do not exist until it has run, and `make emulator-down` deletes them.
- **`network kittenclaws-emulator not found`:** run `make emulator-up` before `make run-emulator`.
- **`UnrecognizedClientException`:** the function did not get the dummy credentials; run it through `make run-emulator`, which passes them to `sam local`.

## Development Workflow

### Adding a New Dependency

```bash
go get <package-path>
```

### Removing a Dependency

```bash
go mod tidy
```

### Adding a field

Fields are written out in each layer, so a new field touches these files (shown for `KittenClaws`; to add an optional `age`):

1. `models/kitten_claws_model.go`: add ``Age *int64 `json:"age,omitempty" dynamodbav:"age,omitempty" firestore:"age,omitempty"` `` to `KittenClaws`, and ``Age *int64 `json:"age"` `` to `KittenClawsDto` and each request struct, and copy it in `ToKittenClawsDto`. A pointer keeps an absent value distinct from `0`.
2. `controllers/schemas/kitten_claws_*_request.json`: add the property and its rules, for example `"age": {"type": "integer", "minimum": 0}`, and list it under `required` if clients must send it. `additionalProperties: false` keeps rejecting unknown fields.
3. `services/kitten_claws_service.go`: copy the field from each request into the `KittenClaws` it builds.
4. `repositories/kitten_claws_repository.go`: in `Update`, copy the field onto the stored record only when it was sent (`if item.Age != nil`).
5. Add the field to the model, controller, service and repository tests, and to the Thunder Client requests in `.thunderclient/`.

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
│   ├── dynamodb_test.go           # the client honours AWS_ENDPOINT_URL_DYNAMODB
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
├── env.emulator.json              # sam local settings for the emulator
├── template.yaml                  # SAM template: API Gateway routes and DynamoDB tables
├── docker-compose.yml             # local DynamoDB emulator
├── go.mod
├── Makefile
└── main.go                        # wires each resource's repository, service, controller and routes
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
