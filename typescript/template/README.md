{%- set containers = resources | map(attribute='container') | unique | list -%}
{%- if cloud_service == 'Azure Function App' -%}
{%- set base_path = '/api' -%}
{%- set env_prefix = 'COSMOS_CONTAINER_' -%}
{%- set store_word = 'Cosmos DB container' -%}
{%- elif cloud_service == 'GCP Cloud Function' -%}
{%- set base_path = '' -%}
{%- set env_prefix = 'FIRESTORE_COLLECTION_' -%}
{%- set store_word = 'Firestore collection' -%}
{%- else -%}
{%- set base_path = '' -%}
{%- set env_prefix = 'DYNAMODB_TABLE_NAME_' -%}
{%- set store_word = 'DynamoDB table' -%}
{%- endif -%}
# {{ project_name }} API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

{% if cloud_service == 'Azure Function App' -%}
This project is a TypeScript Node.js REST API built on [Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/) (programming model v4) and backed by [Azure Cosmos DB for NoSQL](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/). Each enabled operation is registered as its own HTTP-triggered function, with one file per resource under `functions/`.
{%- elif cloud_service == 'GCP Cloud Function' -%}
This project is a TypeScript Node.js REST API built on [Cloud Run functions](https://cloud.google.com/functions/docs) with the [Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-nodejs) and backed by [Firestore](https://cloud.google.com/firestore/docs). A single HTTP function named `api` passes every request to the matching resource's handler in `routes/`.
{%- else -%}
This project is a TypeScript Node.js REST API built on [AWS Lambda](https://docs.aws.amazon.com/lambda/) behind Amazon API Gateway and backed by [Amazon DynamoDB](https://docs.aws.amazon.com/dynamodb/). A single Lambda handler passes every request to the matching resource's handler in `routes/`, and `template.yaml` is an [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/) template for local runs and deployment.
{%- endif %}

The API follows a controller → service → repository layout wired together by an [Inversify](https://inversify.io/) container in `config/container.ts`, validates input with [Zod](https://zod.dev/), and soft-deletes records by setting `isDeleted`. The project is an ES module (`"type": "module"`).

## Endpoints

{% if cloud_service == 'Azure Function App' -%}
Azure Functions serves HTTP functions under the `/api` route prefix. Every route{% if health_endpoint %} except `/api/{{ health_endpoint }}`{% endif %} uses `authLevel: 'function'`, so deployed calls need a function key (`x-functions-key` header or `code` query parameter).
{%- elif cloud_service == 'GCP Cloud Function' -%}
Paths are relative to the function URL (for example `https://<region>-<project>.cloudfunctions.net/{{ project_endpoint }}`, or `http://localhost:8080` locally).
{%- else -%}
Paths are relative to the API Gateway stage URL (for example `https://<api-id>.execute-api.<region>.amazonaws.com/Prod`, or `http://127.0.0.1:3000` with `sam local start-api`).
{%- endif %}{{ "\n" }}

{%- if health_endpoint %}
- `GET {{ base_path }}/{{ health_endpoint }}` — health check
{%- endif %}
{%- for resource in resources %}
- **{{ resource.name }}** (storage: `{{ resource.container }}`)
{%- if "list" in resource.operations %}
  - `GET {{ base_path }}/{{ resource.endpoint }}?limit=100` — list (default 100, max 1000)
{%- endif %}
{%- if "get_by_id" in resource.operations %}
  - `GET {{ base_path }}/{{ resource.endpoint }}/{id}` — get by ID
{%- endif %}
{%- if "create" in resource.operations %}
  - `POST {{ base_path }}/{{ resource.endpoint }}` — create
{%- endif %}
{%- if "update" in resource.operations %}
  - `PATCH {{ base_path }}/{{ resource.endpoint }}/{id}` — partial update
{%- endif %}
{%- if "replace" in resource.operations %}
  - `PUT {{ base_path }}/{{ resource.endpoint }}/{id}` — full replace (keeps `createdBy`/`createdTimestamp`, refreshes `updatedTimestamp`)
{%- endif %}
{%- if "delete" in resource.operations %}
  - `DELETE {{ base_path }}/{{ resource.endpoint }}/{id}` — soft delete
{%- endif %}
{%- endfor %}

## Requests and responses

Every response the app sends is JSON (`Content-Type: application/json`), errors included.

- An item is `{"id": "<uuid>", "name": "<string>"}`. A list is a JSON array (`[]` when empty).
- A request body must be a JSON object holding only the resource's fields. POST and PUT require `name` as a non-empty string (numbers and booleans are not converted). PATCH may leave `name` out, but when present it must be a non-empty string.
- Unknown fields are rejected, including `id`, `isDeleted`, the timestamps and `createdBy`/`updatedBy`, so clients can never set ids or system fields. The id comes from the path only.
- `?limit=` on a list is optional: a missing or invalid value uses the default (100) and larger values are capped at 1000.

| Status | When | Body |
|---|---|---|
| 200 | get, list, update, replace | the item, or an array of items |
| 201 | create | the new item |
| 200 | delete | `{"message": "<Name> with id <id> was deleted successfully."}` |
| 400 | body is not valid JSON or not an object, a field is missing or invalid, or an unknown field is sent | `{"errorMessage": "<what is wrong>"}` |
| 404 | the id does not exist, is soft-deleted, or is not a UUID | `{"errorMessage": "<Name> with id <id> was not found."}` |
{%- if cloud_service != 'Azure Function App' %}
| 404 | unknown path | `{"errorMessage": "Not found."}` |
| 405 | known path, operation not generated for the resource | `{"errorMessage": "Method not allowed."}` |
{%- endif %}
| 500 | anything unexpected, such as a database failure | `{"errorMessage": "An unexpected error occurred."}` |

Expected 4xx outcomes are not logged as errors. Unexpected errors are logged at error level with their stack trace (a database failure carries the SDK error as its `cause`); exception text never reaches the client. A failing or unreachable database answers with that 500 within about 8 seconds: every repository operation is bounded by `DATABASE_DEADLINE_MS` (`utils/deadline.util.ts`), and the database client in `config/container.ts` uses short per-attempt timeouts and capped retries. A cancelled request is logged as a warning, not an error.
{%- if cloud_service == 'Azure Function App' %}

Requests for a method or path that has no registered function never reach the app: the Functions host answers them itself with its own 404 (not JSON, and not a 405). That covers unknown paths and operations that were not generated for a resource.
{%- elif cloud_service == 'GCP Cloud Function' %}

Every request reaches the `api` function, so unknown paths get the JSON 404 and operations that were not generated get the JSON 405. The Functions Framework parses request bodies before the function runs; `main.ts` gives its Express app a JSON final handler, so a malformed body is still answered with the JSON 400 above. The framework itself answers `/favicon.ico` and `/robots.txt` with an empty 404.
{%- else %}

API Gateway only routes the methods and paths listed in `template.yaml`, one per generated operation. Any other method or path is answered by API Gateway (and by `sam local start-api`) with `403 {"message": "Missing Authentication Token"}` without invoking the Lambda, so the handler's JSON 404 and 405 answers only apply to requests that reach it.
{%- endif %}

## Storage

Each resource is stored in the {{ store_word }} named by its `container` setting. The name can be overridden per container with an environment variable:

| Container | Environment variable | Default | Resources |
|---|---|---|---|
{%- for container in containers %}
| `{{ container }}` | `{{ env_prefix }}{{ container | upper | replace('-', '_') }}` | `{{ container }}` | {{ resources | selectattr('container', 'equalto', container) | map(attribute='name') | join(', ') }} |
{%- endfor %}

> [!IMPORTANT]
> Resources that share a container share records. There is no type discriminator, so a record created through one resource's endpoint is listed, read, updated and deleted through every other resource that uses the same container. Give resources separate containers unless that is what you want.

> [!NOTE]
> Upgrading from a single-resource project generated by an earlier version of this template? Storage and environment names changed:
{%- if cloud_service == 'Azure Function App' %}
> the single Cosmos DB container was `{{ project_endpoint }}s-sql-container`; each resource now uses the container named by its `container` id (`{{ project_endpoint }}` for the default single resource), overridable with `COSMOS_CONTAINER_<CONTAINER>`. The database is still `{{ project_endpoint }}s-sql-db` (overridable with `COSMOS_DB_DATABASE_NAME`).
{%- elif cloud_service == 'GCP Cloud Function' %}
> `FIRESTORE_COLLECTION` is now `FIRESTORE_COLLECTION_<CONTAINER>`, one per container (for example `FIRESTORE_COLLECTION_{{ containers[0] | upper | replace('-', '_') }}`).
{%- else %}
> `DYNAMODB_TABLE_NAME` is now `DYNAMODB_TABLE_NAME_<CONTAINER>`, one per container (for example `DYNAMODB_TABLE_NAME_{{ containers[0] | upper | replace('-', '_') }}`).
{%- endif %}
> Set the variable to the old name to keep using existing data.

`<CONTAINER>` is the container id upper-cased with `-` replaced by `_`.

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp`, plus `createdBy`/`updatedBy` only when they are set (they are never stored as null or empty). Timestamps are ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`. Create sets both timestamps; PATCH and PUT keep the stored `createdTimestamp` and `createdBy` and refresh `updatedTimestamp`; DELETE is a soft delete that sets `isDeleted: true` and refreshes `updatedTimestamp`.

## Environment variables

| Variable | Required | Description |
|---|---|---|
{%- if cloud_service == 'Azure Function App' %}
| `COSMOS_DB_URL` | yes | Cosmos DB account endpoint |
| `COSMOS_DB_KEY` | yes | Cosmos DB account key |
| `COSMOS_DB_DATABASE_NAME` | no | Database name (default `{{ project_endpoint }}s-sql-db`) |
{%- elif cloud_service == 'GCP Cloud Function' %}
| `GCP_PROJECT_ID` | yes | Google Cloud project that hosts Firestore |
| `FIRESTORE_DATABASE` | no | Firestore database id (default `(default)`) |
{%- else %}
| `AWS_REGION` | no | Region of the DynamoDB tables (default `us-east-1`; set by Lambda at runtime) |
{%- endif %}
{%- for container in containers %}
| `{{ env_prefix }}{{ container | upper | replace('-', '_') }}` | no | {{ store_word }} for `{{ container }}` (default `{{ container }}`) |
{%- endfor %}

Locally the variables are read from the process environment and from a `.env` file in the project root (loaded quietly, without a startup banner).

## Prerequisites

{% if cloud_service == 'Azure Function App' %}- [Node.js](https://nodejs.org/) 22 (LTS){% else %}- [Node.js](https://nodejs.org/) 24 (LTS){% endif %}
- [Yarn](https://yarnpkg.com/) 4, enabled with `corepack enable`
{%- if cloud_service == 'Azure Function App' %}
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) v4 (installed as a dev dependency)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/) and an Azure subscription
- A Cosmos DB for NoSQL account, in Azure or the [emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator)
{%- elif cloud_service == 'GCP Cloud Function' %}
- [Google Cloud CLI](https://cloud.google.com/sdk/docs/install) and a Google Cloud project
- A Firestore database in Native mode, or the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator)
{%- else %}
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) and [Docker](https://www.docker.com/) for local runs
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) and an AWS account
{%- endif %}

## Setup

```console
corepack enable
yarn install
yarn build
```

## Run locally

{% if cloud_service == 'Azure Function App' -%}
Add the environment variables to the `Values` of `local.settings.json` (or to `.env`), then start the Functions host:

```console
yarn build
yarn start
```

The API is served at `http://localhost:7071/api`.
{%- elif cloud_service == 'GCP Cloud Function' -%}
Export the environment variables (or put them in `.env`), then start the Functions Framework:

```console
yarn build
yarn start
```

The API is served at `http://localhost:8080`. To use the Firestore emulator, also set `FIRESTORE_EMULATOR_HOST`.
{%- else -%}
Build the project, then start API Gateway and Lambda locally with SAM (requires Docker):

```console
yarn build
sam build
sam local start-api
```

The API is served at `http://127.0.0.1:3000`. The functions reach DynamoDB with your local AWS credentials; the table names come from `template.yaml`.
{%- endif %}

The `.thunderclient` directory contains a [Thunder Client](https://www.thunderclient.com/) collection with a request for every generated operation, a few error cases, and a `baseUrl` that matches the local server above.

## Deploy

{% if cloud_service == 'Azure Function App' -%}
Create a Function App on the Flex Consumption plan with the Node.js 22 runtime, set the environment variables as app settings, and publish:

```console
az functionapp create \
  --resource-group <resource-group> \
  --name <function-app-name> \
  --storage-account <storage-account> \
  --flexconsumption-location <region> \
  --runtime node \
  --runtime-version 22 \
  --functions-version 4

az functionapp config appsettings set \
  --resource-group <resource-group> \
  --name <function-app-name> \
  --settings COSMOS_DB_URL=<url> COSMOS_DB_KEY=<key>

yarn build
func azure functionapp publish <function-app-name>
```
{%- elif cloud_service == 'GCP Cloud Function' -%}
Deploy the `api` entry point with the Node.js 24 runtime. Cloud Build installs the dependencies and runs `yarn build`:

```console
gcloud functions deploy {{ project_endpoint }} \
  --gen2 \
  --region <region> \
  --runtime nodejs24 \
  --source . \
  --entry-point api \
  --trigger-http \
  --set-env-vars GCP_PROJECT_ID=<project-id>
```

Add `{{ env_prefix }}<CONTAINER>=<name>` to `--set-env-vars` to override a collection name.
{%- else -%}
`template.yaml` defines the Lambda function (Node.js 24, `nodejs24.x`), one API Gateway route per generated operation, and one DynamoDB table per container.

```console
yarn build
sam build
sam deploy --guided
```
{%- endif %}

## Development

```console
yarn lint        # ESLint
yarn format      # ESLint --fix and Prettier
yarn test:unit   # Jest with coverage thresholds
yarn audit       # yarn npm audit --severity moderate
```

## Repository structure

Each resource gets its own file in every layer, named after the resource in lowerCamelCase{% if resources | length > 1 %} (for example `{{ resources[0].name | to_lower_camel }}.controller.ts`){% endif %}. Each layer's `index.ts` re-exports them.

```text
├── .thunderclient                  - Thunder Client collection, one folder per resource
├── config
│   └── container.ts                - Wiring of repositories, services and controllers
├── controllers                     - Request validation
{%- for resource in resources %}
│   {{ '└──' if loop.last else '├──' }} {{ resource.name | to_lower_camel }}.controller.ts
{%- endfor %}
{%- if cloud_service == 'Azure Function App' %}
├── functions                       - Azure Functions HTTP triggers
{%- if health_endpoint %}
│   ├── health.ts
{%- endif %}
│   ├── response.ts                 - JSON responses and the shared handler wrapper
{%- for resource in resources %}
│   {{ '└──' if loop.last else '├──' }} {{ resource.name | to_lower_camel }}.ts
{%- endfor %}
{%- endif %}
├── repositories                    - Data access
│   ├── base.repository.ts          - CRUD, timestamps and soft deletes over a document store
│   ├── document.store.ts           - Store interface
│   ├── {% if cloud_service == 'Azure Function App' %}cosmos.store.ts             - Cosmos DB{% elif cloud_service == 'GCP Cloud Function' %}firestore.store.ts          - Firestore{% else %}dynamo.store.ts             - DynamoDB{% endif %} store
{%- for resource in resources %}
│   {{ '└──' if loop.last else '├──' }} {{ resource.name | to_lower_camel }}.repository.ts
{%- endfor %}
{%- if cloud_service != 'Azure Function App' %}
├── routes                          - Per-resource HTTP method routing
{%- if health_endpoint %}
│   ├── health.routes.ts
{%- endif %}
│   ├── response.ts
{%- for resource in resources %}
│   {{ '└──' if loop.last else '├──' }} {{ resource.name | to_lower_camel }}.routes.ts
{%- endfor %}
{%- endif %}
├── services                        - Business logic
│   ├── schemaValidator.service.ts
{%- for resource in resources %}
│   {{ '└──' if loop.last else '├──' }} {{ resource.name | to_lower_camel }}.service.ts
{%- endfor %}
├── types
│   ├── errors                      - Error types
│   └── models                      - Zod models and environment schema
{%- for resource in resources %}
│       {{ '└──' if loop.last else '├──' }} {{ resource.name | to_lower_camel }}.schema.ts
{%- endfor %}
├── test                            - Shared test mocks
├── utils                           - Error mapping, JSON body parsing, list limits, clock and ids
{%- if cloud_service == 'GCP Cloud Function' %}
├── main.ts                         - Functions Framework entry point and JSON final handler
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
├── lambda.ts                       - Lambda handler
├── template.yaml                   - AWS SAM template
{%- endif %}
└── package.json                    - Scripts and dependencies
```

Unit tests sit in a `__tests__` directory next to the code they cover, one file per resource (for example `controllers/__tests__/{{ resources[0].name | to_lower_camel }}.controller.test.ts`).

## License

This project is licensed under the {% if open_source_license == 'MIT license' -%}MIT License{% elif open_source_license == 'BSD license' %}
BSD License{% elif open_source_license == 'ISC license' -%}ISC License{% elif open_source_license == 'Apache Software License 2.0' -%}Apache Software License 2.0{% elif open_source_license == 'GNU General Public License v3' -%}GNU General Public License v3
{% endif %}. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
