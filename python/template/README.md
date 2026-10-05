# {{ project_name }} API

[![Made with Cookiecutter API](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/Code-and-Sorts/cookiecutter-api/main/.docs/badge.json)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

{% if cloud_service == 'Azure Function App' -%}
This project is a Python-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
This project is a Python-based REST API built using [Google Cloud Functions](https://cloud.google.com/functions/docs). The API leverages GCP's serverless architecture, allowing you to deploy and scale it effortlessly in the cloud. A single HTTP-triggered function serves every endpoint of the API.
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
This project is a Python-based REST API built using [AWS Lambda](https://docs.aws.amazon.com/lambda/) with API Gateway. The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered Lambda functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}

{%- set containers = path_resources | unique(attribute='container') | list %}
{%- set prefix = '/api/' if cloud_service == 'Azure Function App' else '/' %}
The REST API exposes the following resources and operations:
{%- if cloud_service == 'GCP Cloud Function' %}

A single HTTP Cloud Run function with the entry point `api` (in `main.py`) serves every route below, routing each request by its path and method to the matching resource module in `blueprints/`. Paths are relative to the function URL (for example `https://<region>-<project>.cloudfunctions.net/{{ project_endpoint }}`, or `http://localhost:8080` locally).
{%- endif %}
{% for resource in resources %}
- **{{ resource.name }}** (container: `{{ resource.container }}`)
{%- for op in resource.operations %}
{%- set method = {'list': 'GET', 'get_by_id': 'GET', 'create': 'POST', 'update': 'PATCH', 'replace': 'PUT', 'delete': 'DELETE'}[op] %}
{%- set with_id = op not in ['list', 'create'] %}
{%- set label = {'list': 'list (`?limit=` caps the page size)', 'get_by_id': 'get by ID', 'create': 'create', 'update': 'partial update', 'replace': 'full replace', 'delete': 'soft delete'}[op] %}
  - `{{ method }} {{ prefix }}{{ resource.endpoint }}{% if with_id %}/{id}{% endif %}` — {{ label }}
{%- endfor %}
{%- endfor %}{%- if health_endpoint %}

A health check is available at `GET {{ prefix }}{{ health_endpoint }}` and answers `{"status": "ok"}`.
{%- endif %}
{%- for c in containers %}
{%- set container = c.container %}
{%- set sharing = resources | selectattr('container', 'equalto', container) | map(attribute='name') | list %}
{%- if sharing | length > 1 %}

> **Shared container:** {{ sharing[:-1] | join(", ") }} and {{ sharing[-1] }} read and write the `{{ container }}` container. There is no type discriminator, so they share the same records: an item created through one resource is visible, and can be changed or deleted, through the others.
{%- endif %}
{%- endfor %}

## Requests and responses

Every response, including errors, is JSON with `Content-Type: application/json`.

| Request | Status | Body |
| --- | --- | --- |
| create | 201 | the item |
| get, update, replace | 200 | the item |
| list | 200 | an array of items (`[]` when empty) |
| delete | 200 | `{"message": "<Name> with id <id> was deleted successfully."}` |
| invalid body | 400 | `{"errorMessage": "..."}` |
| `X-User-Id` longer than 256 characters | 400 | `{"errorMessage": "X-User-Id must be at most 256 characters."}` |
| unknown id, deleted item, or an id that is not a UUID | 404 | `{"errorMessage": "<Name> with id <id> was not found."}` |
{%- if cloud_service == 'Azure Function App' %}
| unexpected error | 500 | `{"errorMessage": "An unexpected error occurred."}` |
{%- else %}
| unknown path | 404 | `{"errorMessage": "Not found."}` |
| known path, method not enabled | 405 | `{"errorMessage": "Method not allowed."}` |
| unexpected error | 500 | `{"errorMessage": "An unexpected error occurred."}` |
{%- endif %}

An item holds `id`, `name`, `createdTimestamp`, `createdBy`, `updatedTimestamp` and `updatedBy`, with the values exactly as stored. `createdBy` and `updatedBy` are left out (never `null`) when the record has no user, and `isDeleted` and database metadata are never returned. A create with `X-User-Id: alice` answers:

```json
{
  "id": "0d9cf4b5-3f2c-4a3b-9a0e-6f4f1c2b7d10",
  "name": "Tom",
  "createdTimestamp": "2026-09-29T22:49:26.625Z",
  "createdBy": "alice",
  "updatedTimestamp": "2026-09-29T22:49:26.625Z",
  "updatedBy": "alice"
}
```

A later update or replace without the header keeps the created fields, moves `updatedTimestamp` on and drops `updatedBy`:

```json
{
  "id": "0d9cf4b5-3f2c-4a3b-9a0e-6f4f1c2b7d10",
  "name": "Thomas",
  "createdTimestamp": "2026-09-29T22:49:26.625Z",
  "createdBy": "alice",
  "updatedTimestamp": "2026-09-30T08:12:03.107Z"
}
```

Request bodies must be a JSON object: create (POST) and replace (PUT) require `name` as a non-empty string, update (PATCH) accepts any subset of the fields, and any other field, including `id`, `isDeleted`, the timestamps, `createdBy` and `updatedBy`, is rejected with a 400. Ids are always generated by the server. Unexpected errors are logged with their stack trace and never returned to the client.

Writes (create, update, replace and delete) may send an `X-User-Id` header naming the caller; it is the only way to set `createdBy` and `updatedBy`, which request bodies cannot contain. Surrounding whitespace is trimmed, an empty or missing header means no user, and a value longer than 256 characters is rejected with a 400 before the body is read. Reads ignore the header. The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.
{%- if cloud_service == 'Azure Function App' %}

Resource functions use the `function` auth level, so calls need a function key (the `code` query parameter or the `x-functions-key` header) once deployed; the health check is anonymous. For a method a resource does not enable, the Functions host itself answers 404 before any function runs.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

Resource routes require an API key, sent as the `x-api-key` header{% if health_endpoint %}; the health check does not{% endif %}. `sam deploy` creates the key, and the stack's `{{ project_class_name }}ApiKeyId` output names it: read the value with `aws apigateway get-api-key --api-key <id> --include-value --query value --output text`. `sam local start-api` does not enforce API keys. An API key identifies a caller but is not strong authentication; for that, add an IAM, Cognito or Lambda authorizer.

API Gateway only forwards the routes declared in `template.yaml`: for any other path or method it answers `403 {"message": "Missing Authentication Token"}` itself, without invoking the function. The function's own 404 and 405 answers apply when it is invoked some other way.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

The function is deployed with `--no-allow-unauthenticated`, so callers need the Cloud Run Invoker role (`roles/run.invoker`) and must send `Authorization: Bearer $(gcloud auth print-identity-token)`.{% if health_endpoint %} The health check sits behind the same IAM check, because the project exposes a single function.{% endif %}
{%- endif %}

If the database fails or cannot be reached, the request ends with that 500 within 8 seconds: every database call has a short timeout and a capped retry policy, and `utils/deadline.py` bounds each request's database work as a whole. A request that hits the deadline returns 500, but the write may still complete; retrying a create can therefore store a duplicate.{% if cloud_service == 'GCP Cloud Function' %} WSGI servers such as the Functions Framework are not told when a client disconnects, so an abandoned request keeps running until its database deadline.{% endif %}

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for example `2026-09-29T22:49:26.625Z`), plus `createdBy`/`updatedBy` only when set. Create sets both to the `X-User-Id` user; update, replace and delete set `updatedBy` to it, or remove `updatedBy` when the request has no user, so it always names whoever made the latest write. Update and replace keep the creation fields; delete sets `isDeleted` and refreshes `updatedTimestamp`.

{% if cloud_service == 'Azure Function App' -%}
Dependency management is handled using [uv](https://docs.astral.sh/uv/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
Dependency management is handled using [uv](https://docs.astral.sh/uv/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
Dependency management is handled using [uv](https://docs.astral.sh/uv/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.
{%- endif %}

## Features

{% if cloud_service == 'Azure Function App' -%}
- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- uv for Dependency Management: Manages all Python dependencies with uv, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
- GCP Cloud Functions: Utilizes Google Cloud's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- uv for Dependency Management: Manages all Python dependencies with uv, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- uv for Dependency Management: Manages all Python dependencies with uv, making the development environment consistent and easy to set up.

- DynamoDB: This project uses Amazon DynamoDB as the NoSQL database.
{%- endif %}

## Prerequisites

- Python 3.14

{% if cloud_service == 'Azure Function App' -%}
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [uv](https://docs.astral.sh/uv/): For dependency management and virtual environment setup.

- Azure Account: An active Azure subscription for deploying the Function App. Python 3.14 apps need the Flex Consumption, Premium or Dedicated plan; Linux Consumption stops at Python 3.12.

- Cosmos DB NoSQL Account either deployed in Azure or emulated locally (see [Run locally against the emulator](#run-locally-against-the-emulator)).
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage GCP Cloud Functions.

- [Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-python): To run Cloud Functions locally.

- [uv](https://docs.astral.sh/uv/): For dependency management and virtual environment setup.

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project, or use the local emulator (see [Run locally against the emulator](#run-locally-against-the-emulator)).
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To run the Lambda functions locally.

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html): To deploy and manage AWS resources.

- [uv](https://docs.astral.sh/uv/): For dependency management and virtual environment setup.

- AWS Account: An active AWS account for deploying Lambda functions.

- DynamoDB Tables: One table per container is created automatically via the SAM template.
{%- endif %}

## Configuration

Settings are read from environment variables (case-insensitive).
{%- if cloud_service == 'Azure Function App' %} For local development set them in `local.settings.json`.{% endif %}
{%- if cloud_service == 'AWS Lambda' %} The table names are wired to the tables created in `template.yaml`.{% endif %}

| Variable | Description | Default |
| --- | --- | --- |
{%- if cloud_service == 'Azure Function App' %}
| `Cosmos_Db_Uri` | Cosmos DB account endpoint | required |
| `Cosmos_Db_Key` | Cosmos DB account key | required |
| `Cosmos_Db_Database_Name` | Cosmos DB database name | required |
| `Cosmos_Db_Emulator` | `true` only for the local emulator (see [Run locally against the emulator](#run-locally-against-the-emulator)) | `false` |
{%- for c in containers %}
| `Container_Name_{{ c.container_key }}` | Cosmos DB container for `{{ c.container }}` | `{{ c.container }}` |
{%- endfor %}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
| `GCP_PROJECT_ID` | GCP project ID | required |
| `FIRESTORE_DATABASE` | Firestore database name | `(default)` |
| `FIRESTORE_EMULATOR_HOST` | Firestore emulator address, read by the client library; local development only | unset |
{%- for c in containers %}
| `FIRESTORE_COLLECTION_{{ c.env_key }}` | Firestore collection for `{{ c.container }}` | `{{ c.container }}` |
{%- endfor %}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
| `AWS_REGION` | AWS region | `us-east-1` |
| `AWS_ENDPOINT_URL_DYNAMODB` | DynamoDB endpoint override, read by the AWS SDK; local development only | unset |
{%- for c in containers %}
| `DYNAMODB_TABLE_NAME_{{ c.env_key }}` | DynamoDB table for `{{ c.container }}` | `{{ c.container }}` |
{%- endfor %}
{%- endif %}

## Setup and Installation

{% if cloud_service == 'Azure Function App' -%}
1. Install Azure Functions Core Tools

    Follow the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-python#install-the-azure-functions-core-tools) to install Azure Function Core Tools based on your operating system.

2. Install uv

    If you haven't already installed uv, you can do so by following the [official installation guide](https://docs.astral.sh/uv/getting-started/installation/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment. `make install` runs `uv sync`, which writes `uv.lock` on the first run; commit it so every install and deploy uses the same versions:

    ```console
    make install
    ```

    To be able to run the project locally, set the environment variable values in the local.settings.json project file.

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the local development server using the Azure Function Core Tools, where you can interact with your API endpoints.

5. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

6. Deploy to Azure

    Azure Functions installs Python dependencies from a `requirements.txt`. Generate it from `uv.lock` (it is gitignored, so regenerate it before every publish):

    ```console
    make requirements
    ```

    This runs `uv export --no-dev --format requirements-txt --output-file requirements.txt`, which pins every package with its hashes. Then publish with a remote build (`.funcignore` keeps the virtual environment, tests and uv files out of the package), and set the settings listed under [Configuration](#configuration) as application settings:

    ```console
    func azure functionapp publish <FunctionAppName> --python
    ```
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
1. Install Google Cloud SDK

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud SDK based on your operating system.

2. Install uv

    If you haven't already installed uv, you can do so by following the [official installation guide](https://docs.astral.sh/uv/getting-started/installation/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment. `make install` runs `uv sync`, which writes `uv.lock` on the first run; commit it so every install and deploy uses the same versions:

    ```console
    make install
    ```

4. Set Environment Variables

    Set the environment variables listed under [Configuration](#configuration) for local development.

5. Run the API Locally

    ```console
    make run
    ```

    This serves the `api` function with the Functions Framework (`uv run functions-framework --target=api --source=main.py --port=8080`), so every route is available under `http://localhost:8080`. To run against the Firestore emulator instead of a GCP project, see [Run locally against the emulator](#run-locally-against-the-emulator).

6. Deploy to GCP

    Cloud Run functions install dependencies from a `requirements.txt`. Generate it from `uv.lock` (it is gitignored, so regenerate it before every deploy):

    ```console
    make requirements
    ```

    This runs `uv export --no-dev --format requirements-txt --output-file requirements.txt`, which pins every package with its hashes.

    Then deploy the single `api` entry point:

    ```console
    gcloud functions deploy {{ project_endpoint }} \
      --gen2 \
      --runtime python314 \
      --trigger-http \
      --no-allow-unauthenticated \
      --entry-point api \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id{% for c in containers %},FIRESTORE_COLLECTION_{{ c.env_key }}={{ c.container }}{% endfor %}
    ```

    Callers need the Cloud Run Invoker role and an identity token:

    ```console
    curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" https://<function-url>/{{ resources[0].endpoint }}{% if 'list' not in resources[0].operations %}/<id>{% endif %}
    ```
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install the AWS SAM CLI based on your operating system.

2. Install uv

    If you haven't already installed uv, you can do so by following the [official installation guide](https://docs.astral.sh/uv/getting-started/installation/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment. `make install` runs `uv sync`, which writes `uv.lock` on the first run; commit it so every install and deploy uses the same versions:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the local API Gateway using SAM CLI, where you can interact with your API endpoints.

    > **Note:** API endpoints that interact with DynamoDB require a running DynamoDB instance.
    > For local development, use DynamoDB Local (see [Run locally against the emulator](#run-locally-against-the-emulator))
    > or connect to deployed DynamoDB tables by configuring your AWS credentials and setting the
    > per-container table variables in `template.yaml`:
{%- for c in containers %}
    > `DYNAMODB_TABLE_NAME_{{ c.env_key }}`{% if not loop.last %},{% else %}.{% endif %}
{%- endfor %}

5. Deploy to AWS

    Build and deploy to AWS using SAM:

    ```console
    sam build
    sam deploy --guided
    ```

    Resource routes need the API key from the `{{ project_class_name }}ApiKeyId` stack output:

    ```console
    API_KEY=$(aws apigateway get-api-key --api-key <id> --include-value --query value --output text)
    curl -H "x-api-key: $API_KEY" https://<api-id>.execute-api.<region>.amazonaws.com/Prod/{{ resources[0].endpoint }}{% if 'list' not in resources[0].operations %}/<id>{% endif %}
    ```

    `sam build` runs the Makefile's `build-{{ project_class_name }}Function` target (`BuildMethod: makefile` in `template.yaml`): it exports the main dependencies from `uv.lock`, installs them as Linux x86_64 (manylinux) wheels for Python 3.14 with `uv pip install --python-platform x86_64-manylinux_2_34 --python-version 3.14 --only-binary :all:`, and copies every project module except the tests. It needs `make` and uv, but not Docker.
{%- endif %}

{% set emulator_settings -%}
`make emulator-seed` and `make run-emulator` export the settings in `.env.emulator`{% if cloud_service == 'Azure Function App' %}, which take precedence over `local.settings.json` (Core Tools skips any setting already in the environment){% endif %}.
{%- endset %}
{%- set emulator = {
    'tool': 'make',
    'core_tools': true,
    'run_note': {'Azure Function App': 'func start', 'GCP Cloud Function': 'functions-framework on http://localhost:8080', 'AWS Lambda': 'sam build, then sam local start-api on http://127.0.0.1:3000'}[cloud_service],
    'secret_files': ('`local.settings.json` or ' if cloud_service == 'Azure Function App' else '') ~ '`.env.local`',
    'cosmos_key': '`Cosmos_Db_Key`',
    'cosmos_flag': '`Cosmos_Db_Emulator=true` turns off endpoint discovery, so the client keeps using `Cosmos_Db_Uri` instead of the address the emulator advertises, and, for an `https://` endpoint only, skips certificate verification.',
    'cosmos_https': 'set `Cosmos_Db_Uri=https://localhost:8081/`',
    'missing_container': '',
    'firestore_client': 'With `FIRESTORE_EMULATOR_HOST` set, the Firestore client connects to the emulator without credentials.',
    'sdk_note': 'Every AWS SDK reads the variable natively.',
} -%}
{% include 'shared/_README.emulator.md' %}
## Development Workflow

### Adding a New Dependency

```bash
uv add <package-name>
```

### Removing a Dependency

```bash
uv remove <package-name>
```

### Adding a field
{% set r = resources[0] %}
Each resource's fields are plain Pydantic models in `models/{{ r.name | to_snake }}.py`; the repositories store whatever the model holds, so the model is usually the only file to change. To add an optional `age`:

```python
class Base{{ r.name }}(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)

    name: str = Field(min_length=1)
    age: int | None = Field(default=None, ge=0)


class {{ r.name }}Update(BaseModel):
    ...
    age: int = Field(default=None, ge=0)


class {{ r.name }}Response(BaseResponse):
    name: str
    age: int | None = None
```

- `Base{{ r.name }}` is the create and replace body, `{{ r.name }}Update` the PATCH body (a default of `None` keeps an absent field unset), and `{{ r.name }}Response` what clients get back: it extends `BaseResponse` in `models/base.py`, which holds the id, timestamps and users every resource returns and writes the resource's own fields between the id and the timestamps.
- `extra="forbid"` keeps rejecting unknown fields, and `strict=True` rejects wrongly typed values instead of converting them.
- Add the field to the model, service and controller tests, and to the Thunder Client requests in `.thunderclient/`.

## Running Tests

Ensure your code is working as expected by running unit tests using pytest:

```bash
make test-unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities using [pip-audit](https://pypi.org/project/pip-audit/):

```bash
make audit
```

This is also run automatically in CI on every PR and push to main.

## Repository structure

Each resource gets its own module in every layer, named after the resource. Every module has a matching `*_test.py` unit test file next to it.

{%- set db = {'Azure Function App': 'Cosmos DB', 'GCP Cloud Function': 'Firestore', 'AWS Lambda': 'DynamoDB'}[cloud_service] %}
{%- set fn = {'Azure Function App': 'Function App functions', 'GCP Cloud Function': 'route handlers', 'AWS Lambda': 'route handlers'}[cloud_service] %}

```text
{{ "%-34s" | format("├── .thunderclient") }}- Thunder Client collection
{{ "%-34s" | format("├── blueprints") }}- Cloud entry points, one module per endpoint
{{ "%-34s" | format("│   ├── database.py") }}- {{ db }} client wiring shared by every resource
{%- if health_endpoint %}
{{ "%-34s" | format("│   ├── health.py") }}- Health check at `{{ health_endpoint }}`
{%- endif %}
{%- for resource in resources %}
{{ "%-34s" | format("│   " ~ ("└── " if loop.last else "├── ") ~ (resource.name | to_snake) ~ ".py") }}- {{ resource.name }} {{ fn }}
{%- endfor %}
{{ "%-34s" | format("├── config") }}- Settings loaded from environment variables
{{ "%-34s" | format("├── controllers") }}- Request validation (cloud-agnostic)
{{ "%-34s" | format("│   ├── pagination.py") }}- Shared list `limit` handling
{{ "%-34s" | format("│   ├── validation.py") }}- Shared body and id validation
{%- for resource in resources %}
{{ "%-34s" | format("│   " ~ ("└── " if loop.last else "├── ") ~ (resource.name | to_snake) ~ "_controller.py") }}- {{ resource.name }}Controller
{%- endfor %}
{{ "%-34s" | format("├── errors") }}- Expected errors and their status codes
{{ "%-34s" | format("├── models") }}- Pydantic models
{{ "%-34s" | format("│   ├── base.py") }}- BaseResponse and the timestamp helper
{%- for resource in resources %}
{{ "%-34s" | format("│   " ~ ("└── " if loop.last else "├── ") ~ (resource.name | to_snake) ~ ".py") }}- {{ resource.name }} models
{%- endfor %}
{{ "%-34s" | format("├── repositories") }}- {{ db }} repositories
{{ "%-34s" | format("│   ├── base_repository.py") }}- {{ db }} operations shared by every resource
{%- for resource in resources %}
{{ "%-34s" | format("│   " ~ ("└── " if loop.last else "├── ") ~ (resource.name | to_snake) ~ "_repository.py") }}- {{ resource.name }}Repository
{%- endfor %}
{{ "%-34s" | format("├── scripts") }}
{{ "%-34s" | format("│   └── bootstrap_emulator.py") }}- Prepares the local emulator (`make emulator-seed`)
{{ "%-34s" | format("├── services") }}- Business logic
{%- for resource in resources %}
{{ "%-34s" | format("│   " ~ ("└── " if loop.last else "├── ") ~ (resource.name | to_snake) ~ "_service.py") }}- {{ resource.name }}Service
{%- endfor %}
{{ "%-34s" | format("├── utils") }}- JSON responses, error handling, database deadline, `X-User-Id` header{% if cloud_service != 'Azure Function App' %} and routing{% endif %}
{{ "%-34s" | format("├── .env.emulator") }}- Public settings for the local emulator
{{ "%-34s" | format("├── conftest.py") }}- Constants shared by the unit tests
{{ "%-34s" | format("├── docker-compose.yml") }}- Local {{ db }} emulator
{%- if cloud_service == 'AWS Lambda' %}
{{ "%-34s" | format("├── env.emulator.json") }}- `sam local` settings for the emulator
{%- endif %}
{%- if health_endpoint %}
{{ "%-34s" | format("├── health_test.py") }}- Health check unit tests
{%- endif %}
{%- if cloud_service == 'Azure Function App' %}
{{ "%-34s" | format("├── function_app.py") }}- Function App entry point, registers each blueprint
{{ "%-34s" | format("└── function_app_test.py") }}- Route, auth level and response tests
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
{{ "%-34s" | format("├── main.py") }}- The `api` function, routes every request
{{ "%-34s" | format("└── main_test.py") }}- Routing and response tests
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
{{ "%-34s" | format("├── lambda_app.py") }}- Lambda entry point, routes every request
{{ "%-34s" | format("├── lambda_app_test.py") }}- Routing and response tests
{{ "%-34s" | format("└── template.yaml") }}- AWS SAM template for deployment
{%- endif %}
```

## License

This project is licensed under the {% if open_source_license == 'MIT license' -%}MIT License{% elif open_source_license == 'BSD license' %}
BSD License{% elif open_source_license == 'ISC license' -%}ISC License{% elif open_source_license == 'Apache Software License 2.0' -%}Apache Software License 2.0{% elif open_source_license == 'GNU General Public License v3' -%}GNU General Public License v3
{% endif %}. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
