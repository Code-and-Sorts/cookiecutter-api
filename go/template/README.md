# {{ project_class_name }} API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview
{% if cloud_service == 'Azure Function App' %}
This project is a Go-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) with a [custom handler](https://learn.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
This project is a Go-based REST API built as a single HTTP [Cloud Run function](https://cloud.google.com/functions/docs) (formerly Cloud Functions) with the [Functions Framework for Go](https://github.com/GoogleCloudPlatform/functions-framework-go). The function, registered as `api` in `function.go`, routes every request by its path, and `cmd/main.go` runs it locally.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
This project is a Go-based REST API built using [AWS Lambda](https://docs.aws.amazon.com/lambda/) with [API Gateway](https://docs.aws.amazon.com/apigateway/). The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/) framework is used for local development and deployment.
{%- endif %}

{%- set route_prefix = '/api' if cloud_service == 'Azure Function App' else '' %}
{%- set containers = path_resources | unique(attribute='container') | list %}

The REST API exposes the following resources and operations:
{% for resource in resources %}
- **`{{ route_prefix }}/{{ resource.endpoint }}`** (container: `{{ resource.container }}`)
{%- if "list" in resource.operations %}
  - `GET {{ route_prefix }}/{{ resource.endpoint }}` — list
{%- endif %}
{%- if "get_by_id" in resource.operations %}
  - `GET {{ route_prefix }}/{{ resource.endpoint }}/{id}` — get by ID
{%- endif %}
{%- if "create" in resource.operations %}
  - `POST {{ route_prefix }}/{{ resource.endpoint }}` — create
{%- endif %}
{%- if "update" in resource.operations %}
  - `PATCH {{ route_prefix }}/{{ resource.endpoint }}/{id}` — partial update
{%- endif %}
{%- if "replace" in resource.operations %}
  - `PUT {{ route_prefix }}/{{ resource.endpoint }}/{id}` — full replace
{%- endif %}
{%- if "delete" in resource.operations %}
  - `DELETE {{ route_prefix }}/{{ resource.endpoint }}/{id}` — soft delete
{%- endif %}
{%- endfor %}

{%- if health_endpoint %}

A health check is served at `GET {{ route_prefix }}/{{ health_endpoint }}` and answers `200 {"status":"ok"}`{% if cloud_service == 'Azure Function App' %} without a function key{% elif cloud_service == 'AWS Lambda' %} without an API key{% endif %}.
{%- endif %}
{%- if cloud_service == 'Azure Function App' %}

Routes are served under the Functions host's default `/api` prefix. The resource functions use function-level keys (pass `?code=<key>` or the `x-functions-key` header when deployed){% if health_endpoint %}; the health check function is anonymous{% endif %}.
{%- elif cloud_service == 'AWS Lambda' %}

API Gateway passes the item id as the `{id}` path parameter (for example `/{{ resources[0].endpoint }}/{id}` in `template.yaml`). Every route{% if health_endpoint %} except `/{{ health_endpoint }}`{% endif %} requires an API key sent as `x-api-key: <value>`; step 5 of [Setup and Installation](#setup-and-installation) shows how to read it after deploying. `sam local start-api` does not enforce API keys. An API key identifies a caller but is not strong authentication; for that, add an IAM, Cognito or Lambda authorizer.
{%- elif cloud_service == 'GCP Cloud Function' %}

The deployed function requires IAM: callers need the Cloud Run Invoker role and send `Authorization: Bearer $(gcloud auth print-identity-token)`.{% if health_endpoint %} The health check sits behind the same check, because the project exposes one function.{% endif %}
{%- endif %}

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
{%- if cloud_service == 'Azure Function App' %}

A method a resource does not enable never reaches the handler when the method is missing from every function registered for that path: the Functions host answers it with its own 404.
{%- elif cloud_service == 'AWS Lambda' %}

API Gateway answers a path or method that `template.yaml` does not map with its own `403 {"message":"Missing Authentication Token"}` before the Lambda function runs; the JSON 404 and 405 responses above apply to requests that reach the function.
{%- endif %}

Delete is a soft delete: it sets `isDeleted` to `true`.

### Logging

Logs are written with `log/slog`. Each request is logged once at info level with its method and path. Records below error level go to stdout; unexpected errors are logged at error level, with a stack trace, to stderr. Expected 4xx outcomes, and requests the client cancels by disconnecting, are not logged as errors.

### Storage containers

Each container is configured by its own setting. When the setting is unset, the container id is used as the {% if cloud_service == 'Azure Function App' %}Cosmos DB container{% elif cloud_service == 'GCP Cloud Function' %}Firestore collection{% else %}DynamoDB table{% endif %} name.

| Container | Resources | Setting |
|---|---|---|
{%- for c in containers %}
| `{{ c.container }}` | {{ resources | selectattr('container', 'equalto', c.container) | map(attribute='name') | join(', ') }} | `{% if cloud_service == 'Azure Function App' %}CosmosDbContainerName_{{ c.container_class }}{% elif cloud_service == 'GCP Cloud Function' %}FIRESTORE_COLLECTION_{{ c.env_key }}{% else %}DYNAMODB_TABLE_NAME_{{ c.env_key }}{% endif %}` |
{%- endfor %}

Resources that share a container share its records: there is no type discriminator, so every resource mapped to a container reads, lists, updates and deletes all records in it. Give resources separate containers unless they are meant to operate on the same data.

> **Setting renames:** each container now has its own setting. {% if cloud_service == 'Azure Function App' %}`CosmosDbContainerName` is replaced by `CosmosDbContainerName_<Container>`{% elif cloud_service == 'GCP Cloud Function' %}`FIRESTORE_COLLECTION` is replaced by `FIRESTORE_COLLECTION_<CONTAINER>`{% else %}`DYNAMODB_TABLE_NAME` is replaced by `DYNAMODB_TABLE_NAME_<CONTAINER>`{% endif %}; the single-container setting is no longer read.

Dependency management is handled using [Go Modules](https://go.dev/ref/mod), ensuring a streamlined and consistent environment for managing Go packages and their dependencies.

{% include 'shared/_README.model.md' %}

## Features
{% if cloud_service == 'Azure Function App' %}
- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers using the custom handler model.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
- GCP Cloud Run functions: One HTTP function, `api`, built with the Functions Framework for Go, serves every endpoint on Google Cloud's serverless platform.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway integration.

- Go-Based: Written entirely in Go, leveraging its performance, simplicity, and rich standard library for rapid development.

- Go Modules for Dependency Management: Manages all Go dependencies with Go Modules, making the development environment consistent and easy to set up.

- DynamoDB: This project uses DynamoDB for NoSQL data storage.
{%- endif %}

## Prerequisites

- Go 1.27+
{% if cloud_service == 'Azure Function App' %}
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Go](https://go.dev/dl/): Go SDK and CLI

- Azure Account: An active Azure subscription for deploying the Function App.

- Cosmos DB NoSQL Account either deployed in Azure or emulated locally (see [Run locally against the emulator](#run-locally-against-the-emulator)).
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage Cloud Functions.

- [Go](https://go.dev/dl/): Go SDK and CLI

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project, or use the local emulator (see [Run locally against the emulator](#run-locally-against-the-emulator)).
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To build and run the Lambda functions locally.

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html): To deploy and manage AWS resources.

- [Go](https://go.dev/dl/): Go SDK and CLI

- AWS Account: An active AWS account for deploying the Lambda function.

- DynamoDB table either deployed in AWS or run locally using DynamoDB Local (see [Run locally against the emulator](#run-locally-against-the-emulator)).
{%- endif %}

## Setup and Installation
{% if cloud_service == 'Azure Function App' %}
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
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
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
{%- for c in containers %}
    - `FIRESTORE_COLLECTION_{{ c.env_key }}`: Firestore collection for the `{{ c.container }}` container (defaults to `{{ c.container }}`)
{%- endfor %}

5. Run the API Locally

    ```console
    make run
    ```

    This runs `FUNCTION_TARGET=api PORT=8080 go run ./cmd`: the Functions Framework serves the `api` function at every path on port 8080, for example `http://localhost:8080/{{ resources[0].endpoint }}`. To run against the Firestore emulator instead of a GCP project, see [Run locally against the emulator](#run-locally-against-the-emulator).

6. Deploy to GCP

    Deploy the `api` entry point as an HTTP Cloud Run function:

    ```console
    gcloud functions deploy {{project_endpoint}}-api \
      --gen2 \
      --runtime go127 \
      --region us-central1 \
      --source . \
      --entry-point api \
      --trigger-http \
      --no-allow-unauthenticated \
      --set-env-vars GCP_PROJECT_ID=your-project-id{% for c in containers %},FIRESTORE_COLLECTION_{{ c.env_key }}={{ c.container }}{% endfor %}
    ```

    The function's routes have no prefix, and callers need the Cloud Run Invoker role:

    ```console
    curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" \
      https://<function-url>/{{ resources[0].endpoint }}{% if 'list' not in resources[0].operations %}/<id>{% endif %}
    ```
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
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

    SAM creates an API key and usage plan for the API. Read the key's value from the `{{ project_class_name }}ApiKeyId` stack output and send it in an `x-api-key` header:

    ```console
    aws apigateway get-api-key --api-key <ApiKeyId> --include-value --query value --output text
    curl -H "x-api-key: <value>" https://<api-id>.execute-api.<region>.amazonaws.com/Prod/{{ resources[0].endpoint }}{% if 'list' not in resources[0].operations %}/<id>{% endif %}
    ```
{%- endif %}

{{ {'GCP Cloud Function': 7, 'AWS Lambda': 6}.get(cloud_service, 5) }}. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

{% set emulator_settings -%}
`make emulator-seed` and `make run-emulator` export the settings in `.env.emulator`{% if cloud_service == 'Azure Function App' %}, which take precedence over `local.settings.json` (Core Tools skips any setting already in the environment){% endif %}.
{%- endset %}
{%- set emulator = {
    'tool': 'make',
    'core_tools': true,
    'run_note': {'Azure Function App': 'go build, then func start', 'GCP Cloud Function': 'the Functions Framework on http://localhost:8080', 'AWS Lambda': 'sam build, then sam local start-api on http://127.0.0.1:3000'}[cloud_service],
    'secret_files': ('`local.settings.json` or ' if cloud_service == 'Azure Function App' else '') ~ '`.env.local`',
    'cosmos_key': '`CosmosDbKey`',
    'cosmos_flag': '`CosmosDbEmulator=true` makes the client skip certificate verification for an `https://` endpoint; with the plain-HTTP vNext image it changes nothing. The Go SDK cannot turn off endpoint discovery, and the emulator advertises `http://localhost:8081/`, so run the API on the machine that runs the emulator.',
    'cosmos_https': 'set `CosmosDbEndpoint=https://localhost:8081/`',
    'missing_container': ', so `IsItemNotFound` cannot tell it from a missing item',
    'firestore_client': 'With `FIRESTORE_EMULATOR_HOST` set, the Firestore client connects to the emulator without credentials.',
    'sdk_note': 'Every AWS SDK reads the variable natively.',
} -%}
{% include 'shared/_README.emulator.md' %}
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
`{{ 'function.go' if cloud_service == 'GCP Cloud Function' else 'main.go' }}`) lives in one file per package.

```text
.
├── .github/workflows              # CI: format, vet, build, lint, unit tests and vulnerability scan
├── .thunderclient                 # Thunder Client requests and the localhost environment
├── .env.emulator                  # public settings for the local emulator
├── .golangci.yml
├── cmd
│   ├── bootstrap
│   │   └── main.go                # prepares the local emulator (make emulator-seed)
{%- if cloud_service == 'GCP Cloud Function' %}
│   └── main.go                    # runs the function locally with the Functions Framework
{%- endif %}
├── controllers
│   ├── schemas                    # request body JSON schemas, one per resource and operation
{%- for resource in resources %}
│   ├── {{ resource.name | to_snake }}_controller.go
│   ├── {{ resource.name | to_snake }}_controller_test.go
{%- endfor %}
│   ├── ids.go                     # only UUID ids reach the database
│   ├── ids_test.go
│   ├── pagination.go
│   ├── pagination_test.go
│   └── schemas.go                 # embeds the request schemas
├── handlers
{%- for resource in resources %}
│   ├── {{ resource.name | to_snake }}_handler.go
│   ├── {{ resource.name | to_snake }}_handler_test.go
{%- endfor %}
{%- if health_endpoint %}
│   ├── health_handler.go
│   ├── health_handler_test.go
{%- endif %}
│   ├── router.go                  # request log and deadline, JSON responses and 404, 405 and 500 errors
│   └── router_test.go
├── models                         # each resource's entity, DTO and request types
{%- for resource in resources %}
│   ├── {{ resource.name | to_snake }}_model.go
{%- endfor %}
│   ├── entity.go                  # BaseEntity
│   ├── entity_test.go
│   └── errors.go
├── repositories
{%- for resource in resources %}
│   ├── {{ resource.name | to_snake }}_repository.go
{%- endfor %}
{%- if cloud_service == 'Azure Function App' %}
│   ├── cosmos.go                  # client options; tells a missing item from a missing container
│   ├── cosmos_test.go
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
│   ├── dynamodb_test.go           # the client honours AWS_ENDPOINT_URL_DYNAMODB
{%- endif %}
│   └── store.go                   # generic database access and the shared update, replace and soft delete
├── services
{%- for resource in resources %}
│   ├── {{ resource.name | to_snake }}_service.go
│   ├── {{ resource.name | to_snake }}_service_test.go
{%- endfor %}
│   ├── schema_validator.go
│   └── schema_validator_test.go
├── utils
│   ├── error_detector.go          # maps errors to JSON error responses
│   ├── error_detector_test.go
│   ├── env.go                     # setting or default
│   ├── env_test.go
│   ├── logger.go                  # info logs to stdout, errors to stderr
│   └── logger_test.go
{%- if cloud_service == 'Azure Function App' %}
{%- for resource in resources %}
├── {{ resource.name | to_lower_camel }}Api
│   └── function.json
{%- endfor %}
{%- if health_endpoint %}
├── healthApi
│   └── function.json
{%- endif %}
├── host.json
├── local.settings.json
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
├── env.emulator.json              # sam local settings for the emulator
├── template.yaml                  # SAM template: API Gateway routes and DynamoDB tables
{%- endif %}
├── docker-compose.yml             # local {{ {'Azure Function App': 'Cosmos DB', 'GCP Cloud Function': 'Firestore', 'AWS Lambda': 'DynamoDB'}[cloud_service] }} emulator
├── go.mod
├── Makefile
{%- if cloud_service == 'GCP Cloud Function' %}
└── function.go                    # registers the "api" function and wires each resource
{%- else %}
└── main.go                        # wires each resource's repository, service, controller and routes
{%- endif %}
```

## License

This project is licensed under the {% if open_source_license == 'MIT license' -%}MIT License{% elif open_source_license == 'BSD license' %}
BSD License{% elif open_source_license == 'ISC license' -%}ISC License{% elif open_source_license == 'Apache Software License 2.0' -%}Apache Software License 2.0{% elif open_source_license == 'GNU General Public License v3' -%}GNU General Public License v3
{% endif %}. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
