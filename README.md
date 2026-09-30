# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a TypeScript Node.js REST API built on [Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/) (programming model v4) and backed by [Azure Cosmos DB for NoSQL](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/). Each enabled operation is registered as its own HTTP-triggered function, with one file per resource under `functions/`.

The API follows a controller → service → repository layout wired together by an [Inversify](https://inversify.io/) container in `config/container.ts`, validates input with [Zod](https://zod.dev/), and soft-deletes records by setting `isDeleted`. The project is an ES module (`"type": "module"`).

## Endpoints

Azure Functions serves HTTP functions under the `/api` route prefix. Every route except `/api/health` uses `authLevel: 'function'`, so deployed calls need a function key (`x-functions-key` header or `code` query parameter).

- `GET /api/health` — health check
- **Cat** (storage: `animals`)
  - `GET /api/cats?limit=100` — list (default 100, max 1000)
  - `GET /api/cats/{id}` — get by ID
  - `POST /api/cats` — create
  - `PATCH /api/cats/{id}` — partial update
  - `DELETE /api/cats/{id}` — soft delete
- **Dog** (storage: `animals`)
  - `GET /api/dogs?limit=100` — list (default 100, max 1000)
  - `GET /api/dogs/{id}` — get by ID
  - `POST /api/dogs` — create
  - `PUT /api/dogs/{id}` — full replace (keeps `createdBy`/`createdTimestamp`, refreshes `updatedTimestamp`)
  - `DELETE /api/dogs/{id}` — soft delete

## Requests and responses

Every response the app sends is JSON (`Content-Type: application/json`), errors included.

- An item is `{"id": "<uuid>", "name": "<string>"}`. A list is a JSON array (`[]` when empty).
- A request body must be a JSON object holding only the resource's fields. POST and PUT require `name` as a non-empty string (numbers and booleans are not converted). PATCH may leave `name` out, but when present it must be a non-empty string.
- Unknown fields are rejected, including `id`, `isDeleted`, the timestamps and `createdBy`/`updatedBy`, so clients can never set ids or system fields. The id comes from the path only.
- `?limit=` on a list is optional: a missing or invalid value uses the default (100) and larger values are capped at 1000.
- Writes record the user id sent in the optional `X-User-Id` header (any casing; surrounding whitespace is trimmed). POST stores it as `createdBy` and `updatedBy`; PATCH, PUT and DELETE store it as `updatedBy` and keep `createdBy`. A write without the header (or with an empty one) removes `updatedBy`, so it always names the latest writer. GET ignores the header and responses never include these fields. A value longer than 256 characters is rejected with `400 {"errorMessage": "X-User-Id must be at most 256 characters."}` before the body is read.
  The header is taken as sent and is not authenticated: any caller can set it. Before relying on `createdBy`/`updatedBy`, put the API behind an authenticating gateway or authorizer that sets `X-User-Id` from the verified identity and strips any value the client sent.

| Status | When | Body |
|---|---|---|
| 200 | get, list, update, replace | the item, or an array of items |
| 201 | create | the new item |
| 200 | delete | `{"message": "<Name> with id <id> was deleted successfully."}` |
| 400 | body is not valid JSON or not an object, a field is missing or invalid, an unknown field is sent, or `X-User-Id` is too long | `{"errorMessage": "<what is wrong>"}` |
| 404 | the id does not exist, is soft-deleted, or is not a UUID | `{"errorMessage": "<Name> with id <id> was not found."}` |
| 500 | anything unexpected, such as a database failure | `{"errorMessage": "An unexpected error occurred."}` |

Expected 4xx outcomes are not logged as errors. Unexpected errors are logged at error level with their stack trace (a database failure carries the SDK error as its `cause`); exception text never reaches the client. A failing or unreachable database answers with that 500 within about 8 seconds: every repository operation is bounded by `DATABASE_DEADLINE_MS` (`utils/deadline.util.ts`), and the database client in `config/container.ts` uses short per-attempt timeouts and capped retries. A cancelled request is logged as a warning, not an error. A request that hits the database deadline returns 500, but the write may still complete; retrying a create can therefore store a duplicate.

Requests for a method or path that has no registered function never reach the app: the Functions host answers them itself with its own 404 (not JSON, and not a 405). That covers unknown paths and operations that were not generated for a resource.

## Storage

Each resource is stored in the Cosmos DB container named by its `container` setting. The name can be overridden per container with an environment variable:

| Container | Environment variable | Default | Resources |
|---|---|---|---|
| `animals` | `COSMOS_CONTAINER_ANIMALS` | `animals` | Cat, Dog |

> [!IMPORTANT]
> Resources that share a container share records. There is no type discriminator, so a record created through one resource's endpoint is listed, read, updated and deleted through every other resource that uses the same container. Give resources separate containers unless that is what you want.

`<CONTAINER>` is the container id upper-cased with `-` replaced by `_`.

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp`, plus `createdBy`/`updatedBy` only when they are set (they are never stored as null or empty). Timestamps are ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`. Create sets both timestamps; PATCH and PUT keep the stored `createdTimestamp` and `createdBy` and refresh `updatedTimestamp`; DELETE is a soft delete that sets `isDeleted: true` and refreshes `updatedTimestamp`. Every write sets or removes `updatedBy` from the `X-User-Id` header.

## Environment variables

| Variable | Required | Description |
|---|---|---|
| `COSMOS_DB_URL` | yes | Cosmos DB account endpoint |
| `COSMOS_DB_KEY` | yes | Cosmos DB account key |
| `COSMOS_DB_DATABASE_NAME` | no | Database name (default `kittenclawss-sql-db`) |
| `COSMOS_CONTAINER_ANIMALS` | no | Cosmos DB container for `animals` (default `animals`) |

Locally the variables are read from the process environment and from a `.env` file in the project root (loaded quietly, without a startup banner).

## Prerequisites

- [Node.js](https://nodejs.org/) 22 (LTS)
- [Yarn](https://yarnpkg.com/) 4, enabled with `corepack enable`
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) v4 (installed as a dev dependency)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/) and an Azure subscription
- A Cosmos DB for NoSQL account, in Azure or the [emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator)

## Setup

```console
corepack enable
yarn install
yarn build
```

## Run locally

Add the environment variables to the `Values` of `local.settings.json` (or to `.env`), then start the Functions host:

```console
yarn build
yarn start
```

The API is served at `http://localhost:7071/api`.

The `.thunderclient` directory contains a [Thunder Client](https://www.thunderclient.com/) collection with a request for every generated operation, a few error cases, and a `baseUrl` that matches the local server above.

## Deploy

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

## Development

```console
yarn lint        # ESLint
yarn format      # ESLint --fix and Prettier
yarn test:unit   # Jest with coverage thresholds
yarn audit       # yarn npm audit --severity moderate
```

## Repository structure

Each resource gets its own file in every layer, named after the resource in lowerCamelCase (for example `cat.controller.ts`). Each layer's `index.ts` re-exports them.

```text
├── .thunderclient                  - Thunder Client collection, one folder per resource
├── config
│   └── container.ts                - Wiring of repositories, services and controllers
├── controllers                     - Request validation
│   ├── cat.controller.ts
│   └── dog.controller.ts
├── functions                       - Azure Functions HTTP triggers
│   ├── health.ts
│   ├── response.ts                 - JSON responses and the shared handler wrapper
│   ├── cat.ts
│   └── dog.ts
├── repositories                    - Data access
│   ├── base.repository.ts          - CRUD, timestamps and soft deletes over a document store
│   ├── document.store.ts           - Store interface
│   ├── cosmos.store.ts             - Cosmos DB store
│   ├── cat.repository.ts
│   └── dog.repository.ts
├── services                        - Business logic
│   ├── schemaValidator.service.ts
│   ├── cat.service.ts
│   └── dog.service.ts
├── types
│   ├── errors                      - Error types
│   └── models                      - Zod models and environment schema
│       ├── cat.schema.ts
│       └── dog.schema.ts
├── test                            - Shared test mocks
├── utils                           - Error mapping, JSON body parsing, list limits, clock and ids
└── package.json                    - Scripts and dependencies
```

Unit tests sit in a `__tests__` directory next to the code they cover, one file per resource (for example `controllers/__tests__/cat.controller.test.ts`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
