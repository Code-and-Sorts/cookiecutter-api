# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a TypeScript Node.js REST API built on [Cloud Run functions](https://cloud.google.com/functions/docs) with the [Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-nodejs) and backed by [Firestore](https://cloud.google.com/firestore/docs). A single HTTP function named `api` passes every request to the matching resource's handler in `routes/`.

The API follows a controller → service → repository layout wired together by an [Inversify](https://inversify.io/) container in `config/container.ts`, validates input with [Zod](https://zod.dev/), and soft-deletes records by setting `isDeleted`. The project is an ES module (`"type": "module"`).

## Endpoints

Paths are relative to the function URL (for example `https://<region>-<project>.cloudfunctions.net/kittenclaws`, or `http://localhost:8080` locally). The deployed function is not public; see [Deploy](#deploy) for calling it.

- `GET /health` — health check
- **Cat** (storage: `cats`)
  - `GET /cats?limit=100` — list (default 100, max 1000)
  - `GET /cats/{id}` — get by ID
  - `POST /cats` — create
  - `PATCH /cats/{id}` — partial update
  - `PUT /cats/{id}` — full replace (keeps `createdBy`/`createdTimestamp`, refreshes `updatedTimestamp`)
  - `DELETE /cats/{id}` — soft delete
- **Dog** (storage: `dogs`)
  - `GET /dogs?limit=100` — list (default 100, max 1000)
  - `GET /dogs/{id}` — get by ID
  - `POST /dogs` — create
  - `PUT /dogs/{id}` — full replace (keeps `createdBy`/`createdTimestamp`, refreshes `updatedTimestamp`)
  - `DELETE /dogs/{id}` — soft delete
- **Visit** (storage: `visits`)
  - `GET /visits/{id}` — get by ID
  - `POST /visits` — create
  - `PATCH /visits/{id}` — partial update

## Requests and responses

Every response the app sends is JSON (`Content-Type: application/json`), errors included.

- Items and request bodies follow the [data model](#data-model). A list is a JSON array (`[]` when empty).
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
| 404 | unknown path | `{"errorMessage": "Not found."}` |
| 405 | known path, operation not generated for the resource | `{"errorMessage": "Method not allowed."}` |
| 500 | anything unexpected, such as a database failure | `{"errorMessage": "An unexpected error occurred."}` |

Expected 4xx outcomes are not logged as errors. Unexpected errors are logged at error level with their stack trace (a database failure carries the SDK error as its `cause`); exception text never reaches the client. A failing or unreachable database answers with that 500 within about 8 seconds: every repository operation is bounded by `DATABASE_DEADLINE_MS` (`utils/deadline.util.ts`), and the database client in `config/container.ts` uses short per-attempt timeouts and capped retries. A cancelled request is logged as a warning, not an error. A request that hits the database deadline returns 500, but the write may still complete; retrying a create can therefore store a duplicate.

Every request reaches the `api` function, so unknown paths get the JSON 404 and operations that were not generated get the JSON 405. The Functions Framework parses request bodies before the function runs; `main.ts` gives its Express app a JSON final handler, so a malformed body is still answered with the JSON 400 above. The framework itself answers `/favicon.ico` and `/robots.txt` with an empty 404.

## Storage

Each resource is stored in the Firestore collection named by its `container` setting. The name can be overridden per container with an environment variable:

| Container | Environment variable | Default | Resources |
|---|---|---|---|
| `cats` | `FIRESTORE_COLLECTION_CATS` | `cats` | Cat |
| `dogs` | `FIRESTORE_COLLECTION_DOGS` | `dogs` | Dog |
| `visits` | `FIRESTORE_COLLECTION_VISITS` | `visits` | Visit |

> [!IMPORTANT]
> Resources that share a container share records. There is no type discriminator, so a record created through one resource's endpoint is listed, read, updated and deleted through every other resource that uses the same container. Give resources separate containers unless that is what you want.

`<CONTAINER>` is the container id upper-cased with `-` replaced by `_`.

Stored records hold the [data model](#data-model)'s fields, plus `createdBy`/`updatedBy` only when they are set (they are never stored as null or empty). Timestamps are ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`. Create sets both timestamps; PATCH and PUT keep the stored `createdTimestamp` and `createdBy` and refresh `updatedTimestamp`; DELETE is a soft delete that sets `isDeleted: true` and refreshes `updatedTimestamp`. Every write sets or removes `updatedBy` from the `X-User-Id` header.

## Data model

Every resource stores the built-in base fields, the fields `base_model` adds and its own `fields`. They
come from the Copier answers in `.copier-answers.yml`: change them there and run `copier update`, so all
layers (models, validation, storage and tests) are regenerated together.

The built-in fields are set by the server and never accepted in a request body: `id` (a UUID, returned),
`isDeleted` (soft delete), `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for
example `2026-09-29T22:49:26.625Z`; a create reads the clock once for both), and `createdBy`/`updatedBy`
(the `X-User-Id` header, stored only when it is sent). Only `id` is returned.

`base_model` adds these fields to every resource:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned | Description |
|---|---|---|---|---|---|---|---|---|
| `tenantId` | string | no | no | `"public"` | max_length: 64; min_length: 1 | POST, PUT, PATCH | yes | Owning tenant |
| `region` | enum | no | no | `"eu"` | one of eu, us | POST | yes | none |
| `priority` | integer | no | yes | none | minimum: 0 | POST, PUT, PATCH | yes | none |
| `rank` | number | yes | no | none | none | POST, PUT, PATCH | yes | none |
| `labels` | array of string | no | no | `[]` | unique_items: true | POST, PUT, PATCH | yes | none |

`Cat` (`/cats`) fields:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned | Description |
|---|---|---|---|---|---|---|---|---|
| `name` | string | yes | no | none | max_length: 100; min_length: 1 | POST, PUT, PATCH | yes | none |
| `breed` | enum | no | no | `"tabby"` | one of siamese, persian, tabby | POST, PUT | yes | none |
| `ageYears` | integer | no | no | `0` | maximum: 40; minimum: 0 | POST, PUT, PATCH | yes | none |
| `weightKg` | number | no | yes | `null` | exclusive_maximum: 100; exclusive_minimum: 0 | POST, PUT, PATCH | yes | none |
| `indoor` | boolean | no | no | `true` | none | POST, PUT, PATCH | yes | none |
| `birthDate` | date | no | no | `$today` | none | POST, PUT | yes | none |
| `microchipId` | uuid | no | no | `$uuid` | none | POST | yes | none |
| `ownerEmail` | string | no | no | `"unknown@example.com"` | format: "email" | POST, PUT, PATCH | yes | none |
| `website` | string | no | yes | none | format: "uri" | POST, PUT, PATCH | yes | none |
| `tagCode` | string | no | no | none | pattern: "^[A-Z]{3}-[0-9]{3}$" | POST, PUT | yes | none |
| `tags` | array of string | no | no | `[]` | max_items: 3; unique_items: true | POST, PUT, PATCH | yes | none |
| `scores` | array of integer | no | yes | none | min_items: 1 | POST, PUT | yes | none |
| `adoptedAt` | date-time | no | yes | `$now` | none | POST, PUT, PATCH | yes | none |
| `lastVisit` | date-time | no | no | `"2026-01-01T00:00:00.000Z"` | none | POST, PUT | yes | none |
| `notes` | string | no | no | `"$none"` | none | POST, PUT, PATCH | no | none |

`Dog` (`/dogs`) fields:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned | Description |
|---|---|---|---|---|---|---|---|---|
| `name` | string | yes | no | none | min_length: 1 | POST, PUT | yes | none |

`Visit` (`/visits`) fields:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned | Description |
|---|---|---|---|---|---|---|---|---|
| `reason` | string | yes | no | none | none | POST | yes | none |
| `visitedOn` | date | yes | no | none | none | POST, PATCH | yes | none |
| `cost` | number | no | no | none | minimum: 0 | POST, PATCH | yes | none |
| `paid` | boolean | no | no | `false` | none | PATCH | yes | none |
| `checkedAt` | array of date-time | no | no | none | none | PATCH | yes | none |

- A response holds `id` and every field that is not hidden, with `null` for a field without a value. A
  list is a JSON array (`[]` when empty).
- A request body must be a JSON object holding only the fields its operation accepts; any other field,
  including `id` and the built-in fields, is a 400. Values are never converted: `"1"` is not an integer.
- Create (POST) and replace (PUT) give a field left out its default (`$now`, `$today` and `$uuid` are
  evaluated on every write) or no value; a required field without a default must be sent. Replace keeps
  the fields it does not accept, such as immutable ones, at their stored values.
- Update (PATCH) changes only the fields sent; `null` clears a nullable field.
- A field without a value is not stored. Date-times are stored in UTC with milliseconds whatever offset
  was sent, dates as `YYYY-MM-DD`. Reading a record that lacks a field returns its static default
  (`null` for a nullable field).
- Integers must lie within ±9007199254740991, so every language and JSON parser reads them exactly.
  Patterns match anywhere in the value unless anchored with `^` and `$`; `email` and `uri` check the
  value's shape only.


## Environment variables

| Variable | Required | Description |
|---|---|---|
| `GCP_PROJECT_ID` | yes | Google Cloud project that hosts Firestore |
| `FIRESTORE_DATABASE` | no | Firestore database id (default `(default)`) |
| `FIRESTORE_EMULATOR_HOST` | no | Firestore emulator address, read by the client library; local development only |
| `FIRESTORE_COLLECTION_CATS` | no | Firestore collection for `cats` (default `cats`) |
| `FIRESTORE_COLLECTION_DOGS` | no | Firestore collection for `dogs` (default `dogs`) |
| `FIRESTORE_COLLECTION_VISITS` | no | Firestore collection for `visits` (default `visits`) |

Locally the variables are read from the process environment and from a `.env` file in the project root (loaded quietly, without a startup banner).

## Prerequisites

- [Node.js](https://nodejs.org/) 24 (LTS)
- [Yarn](https://yarnpkg.com/) 4, enabled with `corepack enable`
- [Google Cloud CLI](https://cloud.google.com/sdk/docs/install) and a Google Cloud project
- A Firestore database in Native mode, or Docker for the local emulator (see [Run locally against the emulator](#run-locally-against-the-emulator))

## Setup

```console
corepack enable
yarn install
yarn build
```

## Run locally

Export the environment variables (or put them in `.env`), then start the Functions Framework:

```console
yarn build
yarn start
```

The API is served at `http://localhost:8080`. To run against the Firestore emulator instead, see [Run locally against the emulator](#run-locally-against-the-emulator).

The `.thunderclient` directory contains a [Thunder Client](https://www.thunderclient.com/) collection with a request for every generated operation, a few error cases, and a `baseUrl` that matches the local server above.

## Run locally against the emulator

`docker-compose.yml` runs the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2; with Podman, run the `podman compose` equivalents of the `emulator:*` scripts.

```console
yarn install
yarn emulator:up      # docker compose up -d --wait: returns once the emulator is healthy
yarn emulator:seed    # waits until the emulator answers (collections are created on first write); safe to re-run
yarn start:emulator   # yarn build, then functions-framework on http://localhost:8080 with the emulator settings
yarn emulator:down    # docker compose down -v: stops the emulator and discards its data
```

`yarn emulator:logs` follows the emulator's logs. `yarn emulator:seed` and `yarn start:emulator` load the settings in `.env.emulator` instead of `.env` (through `DOTENV_CONFIG_PATH`), overriding variables already set. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `.env` or `.env.local`, which git ignores. The emulator keeps no data outside its container, so `yarn emulator:down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8085 | Firestore emulator (8080 in the container; 8085 on the host leaves 8080 to the Functions Framework) |

- With `FIRESTORE_EMULATOR_HOST` set, the Firestore client connects to the emulator without credentials. `GCP_PROJECT_ID` uses a `demo-` project id, which can never reach a real project.
- The emulator keeps data in memory, enforces no IAM or security rules for server SDKs and does not require composite indexes. Keep `FIRESTORE_DATABASE` at `(default)` locally.

Startup takes about 10 to 20 seconds; the healthcheck allows up to 2 minutes. `yarn emulator:seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`yarn emulator:up` fails or never turns healthy:** check `yarn emulator:logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** check that `yarn emulator:seed` passes and that the API was started with `yarn start:emulator`, which sets `FIRESTORE_EMULATOR_HOST`.

## Deploy

Deploy the `api` entry point with the Node.js 24 runtime. Cloud Build installs the dependencies and runs `yarn build`:

```console
gcloud functions deploy kittenclaws \
  --gen2 \
  --region <region> \
  --runtime nodejs24 \
  --source . \
  --entry-point api \
  --trigger-http \
  --no-allow-unauthenticated \
  --set-env-vars GCP_PROJECT_ID=<project-id>
```

Add `FIRESTORE_COLLECTION_<CONTAINER>=<name>` to `--set-env-vars` to override a collection name.

The function is not public. Grant callers the Cloud Run Invoker role (`gcloud functions add-invoker-policy-binding kittenclaws --region <region> --member <principal>`); they send an identity token, the health check included, since the project exposes one function:

```console
curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" \
  https://<region>-<project>.cloudfunctions.net/kittenclaws/cats
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
├── .env.emulator                   - Public settings for the local emulator
├── .thunderclient                  - Thunder Client collection, one folder per resource
├── config
│   └── container.ts                - Wiring of repositories, services and controllers
├── controllers                     - Request validation
│   ├── cat.controller.ts
│   ├── dog.controller.ts
│   └── visit.controller.ts
├── repositories                    - Data access
│   ├── base.repository.ts          - CRUD, timestamps and soft deletes over a document store
│   ├── document.store.ts           - Store interface
│   ├── firestore.store.ts          - Firestore store
│   ├── cat.repository.ts
│   ├── dog.repository.ts
│   └── visit.repository.ts
├── routes                          - Per-resource HTTP method routing
│   ├── health.routes.ts
│   ├── response.ts
│   ├── cat.routes.ts
│   ├── dog.routes.ts
│   └── visit.routes.ts
├── scripts
│   └── bootstrapEmulator.ts        - Prepares the local emulator (yarn emulator:seed)
├── services                        - Business logic
│   ├── schemaValidator.service.ts
│   ├── cat.service.ts
│   ├── dog.service.ts
│   └── visit.service.ts
├── types
│   ├── errors                      - Error types
│   └── models                      - Zod models and environment schema
│       ├── cat.schema.ts
│       ├── dog.schema.ts
│       └── visit.schema.ts
├── test                            - Shared test mocks
├── utils                           - Error mapping, JSON body parsing, list limits, clock and ids
├── main.ts                         - Functions Framework entry point and JSON final handler
├── docker-compose.yml              - Local Firestore emulator
└── package.json                    - Scripts and dependencies
```

Unit tests sit in a `__tests__` directory next to the code they cover, one file per resource (for example `controllers/__tests__/cat.controller.test.ts`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
