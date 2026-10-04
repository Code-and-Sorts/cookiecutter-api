# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Dotnet-based REST API built using [AWS Lambda](https://aws.amazon.com/lambda/) with [API Gateway](https://aws.amazon.com/api-gateway/). The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The Lambda functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/kittenclaws`** (container: `kittenclaws`)
  - `GET /kittenclaws` — list
  - `GET /kittenclaws/{id}` — get by ID
  - `POST /kittenclaws` — create
  - `PATCH /kittenclaws/{id}` — partial update
  - `DELETE /kittenclaws/{id}` — soft delete
- **`/health`**
  - `GET /health` — health check

Paths are relative to the API Gateway stage URL (`https://<api-id>.execute-api.<region>.amazonaws.com/Prod`, or `http://localhost:3000` with `sam local start-api`). Routes have no `/api` prefix; the path parameter is `{id}`.

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

An item has exactly these fields, the stored values (`createdBy` and `updatedBy` only when they are set; never `null`):

```json
{
  "id": "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c",
  "name": "KittenClaws1",
  "createdTimestamp": "2026-09-29T22:49:26.625Z",
  "createdBy": "user-1",
  "updatedTimestamp": "2026-09-30T08:12:03.041Z",
  "updatedBy": "user-2"
}
```

`isDeleted` and database fields are never returned. A create returns `createdTimestamp` equal to `updatedTimestamp`; update and replace return the new `updatedTimestamp` and `updatedBy` (absent when the request has no `X-User-Id`) with the created fields unchanged.

Request bodies must be a JSON object: `name` is required on create (`POST`) and replace (`PUT`) and optional on update (`PATCH`), and when present it must be a non-empty JSON string (numbers and booleans are not converted). Any other field, including `id`, `isDeleted`, the timestamps, `createdBy` and `updatedBy`, is rejected with `400`, so clients can never set ids or system fields.

Writes (`POST`, `PATCH`, `PUT` and `DELETE`) record who made them from the optional `X-User-Id` request header (surrounding whitespace is trimmed; an empty or missing header means no user). Create stores it as `createdBy` and `updatedBy`, and later writes store it as `updatedBy`, removing any earlier `updatedBy` when the header is absent. A value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}` before the body is read. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

Each request's database work is bounded by a 5-second deadline (`Utils/RequestDeadline.cs`) and the database client has capped timeouts and retries, so an unreachable or failing database answers the generic `500` well inside the platform timeout. A request that hits the deadline returns `500`, but the write may still complete, so retrying a create can store a duplicate. A request the client cancels is logged at information level, not as an error.

Each operation is its own Lambda function. Every function checks that the event's method and path are its own route and otherwise answers `405 {"errorMessage": "Method not allowed."}` (another method) or `404 {"errorMessage": "Not found."}` (another path), so an event sent straight to the wrong function is not served.

List endpoints accept `?limit=<n>` (default `100`, at most `1000`); a missing or invalid value uses the default and a larger value is capped.

Requests that never reach the app are answered by API Gateway: an unmapped path or method returns `403` with `{"message": "Missing Authentication Token"}`.

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when they are set. Updates and replacements keep `createdTimestamp` and `createdBy` and refresh `updatedTimestamp` and `updatedBy`; delete is a soft delete that sets `isDeleted` to `true`.

## Storage containers

Each resource reads and writes the DynamoDB table configured for its `container` id. The table names are read from these environment variables, which `template.yaml` sets from the table resources it creates, falling back to the container id when a variable is not set:

| Container | Environment variable | `template.yaml` table resource | Resources |
|---|---|---|---|
| `kittenclaws` | `DYNAMODB_TABLE_NAME_KITTENCLAWS` | `KittenclawsTable` | KittenClaws |

Resources that use the same container share its records: there is no type discriminator, so every resource on a shared container lists, reads, updates and deletes the same items.

## Features

- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway integration.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- DynamoDB: This project uses Amazon DynamoDB as its NoSQL database.

## Prerequisites

- Dotnet 10.x

- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To build and run the Lambda functions locally.

- [AWS CLI](https://aws.amazon.com/cli/): To deploy and manage AWS resources.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- AWS Account: An active AWS account for deploying Lambda functions.

- DynamoDB table (created automatically via the SAM template, or DynamoDB Local; see [Run locally against the emulator](#run-locally-against-the-emulator)).

## Setup and Installation

1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install AWS SAM CLI based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command builds the project and starts a local API Gateway using SAM CLI, where you can interact with your API endpoints. `sam local start-api` does not enforce API keys.

5. Deploy

    ```console
    cd KittenClaws.Api
    sam build
    sam deploy --guided
    ```

    Resource routes require an API key; the health check does not. SAM creates the key and a usage plan, and the `ApiKeyId` stack output holds its id. Read the value and send it as the `x-api-key` header:

    ```console
    aws apigateway get-api-key --api-key <ApiKeyId> --include-value --query value --output text
    curl -H "x-api-key: <value>" https://<api-id>.execute-api.<region>.amazonaws.com/Prod/kittenclaws
    ```

    An API key identifies a caller but is not strong authentication; for that, add an IAM, Cognito or Lambda authorizer.

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

`make emulator-logs` follows the emulator's logs. `make emulator-seed` runs `KittenClaws.Bootstrap`, a console project in the solution that reuses the API's client setup and store name settings. It and `make run-emulator` export the settings in `.env.emulator`. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `.env.local`, which git ignores. The emulator keeps no data outside its container, so `make emulator-down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8000 | DynamoDB Local (`-inMemory -sharedDb`, so tables do not depend on the region or access key) |

- `make emulator-seed` runs on your machine and reaches DynamoDB Local at `http://localhost:8000` (`.env.emulator`). `sam local start-api` runs the function in a container on the `kittenclaws-emulator` Docker network, so `env.emulator.json` points it at `http://dynamodb:8000` and sets the table names, which `sam local` would otherwise take from the template's logical ids. `--warm-containers LAZY` keeps each function's container running between requests and restarts it when the build changes.
- `template.yaml` declares `AWS_ENDPOINT_URL_DYNAMODB` only so `sam local` can set it; deployed stacks leave it out unless you pass the `DynamoDbEndpoint` parameter. The AWS SDK reads the variable natively; when `sam local` runs without `env.emulator.json` it passes the variable empty, and the client then ignores it instead of failing.
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
dotnet add package <package-name>
```

### Removing a Dependency

```bash
dotnet remove package <package-name>
```

### Adding a field

Fields are written out in each layer, so a new field touches these files under `KittenClaws.Api` (shown for `KittenClaws`; to add an optional `Age`):

1. `Models/Entities/KittenClawsEntity.cs`: add `public int? Age { get; set; }`, and map it in `WriteAttributes` and `ReadAttributes`.
2. `Models/Dtos/KittenClawsDto.cs`: add the property clients get back; it serializes after `name` and before the timestamps, which `BaseDto` holds.
3. `Models/KittenClawsRequests.cs`: add `[JsonPropertyName("age")] public int? Age { get; set; }` to each request; `RequestBody` rejects any field without `[JsonPropertyName]`.
4. `Models/Schemas/KittenClawsValidation.cs`: add the rules, for example `RuleFor(x => x.Age).GreaterThanOrEqualTo(0).When(x => x.Age != null);`.
5. `Services/KittenClawsService.cs` and `Repositories/KittenClawsRepository.cs`: copy the field from the request into the entity, set it in `MapFields` (`EntityRepository` maps `id` and the audit fields), and in the update merge keep the stored value when the request leaves it out (`changes.Age ?? current.Age`).
6. Add the field to the tests in `KittenClaws.Api.Tests.Unit` and to the Thunder Client requests in `.thunderclient/`.

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
│   ├── Repositories
│   ├── Services
│   └── Utils
├── KittenClaws.Api.Tests.Unit
│   ├── Controllers
│   ├── Functions
│   ├── Repositories
│   ├── Services
│   ├── Utils
│   └── tests
├── KittenClaws.Bootstrap               - Prepares the local emulator (make emulator-seed)
├── .env.emulator                       - Public settings for the local emulator
├── env.emulator.json                   - sam local settings for the emulator
└── docker-compose.yml                  - Local DynamoDB emulator
```

Each resource has its own file in every layer, named after the resource: for example `KittenClawsController.cs`, `KittenClawsService.cs`, `KittenClawsRepository.cs`, `KittenClawsFunctions.cs` and their `IKittenClaws…` interfaces, DTO, entity, request and validation models, and unit tests.
Each repository extends `EntityRepository`, which holds the shared read, list, create, update and soft-delete logic and talks to the database through `IDocumentStore` (`DynamoDocumentStore`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
