# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a TypeScript Node.js REST API built on [AWS Lambda](https://docs.aws.amazon.com/lambda/) behind Amazon API Gateway and backed by [Amazon DynamoDB](https://docs.aws.amazon.com/dynamodb/). A single Lambda handler passes every request to the matching resource's handler in `routes/`, and `template.yaml` is an [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/) template for local runs and deployment.

The API follows a controller → service → repository layout wired together by an [Inversify](https://inversify.io/) container in `config/container.ts`, validates input with [Zod](https://zod.dev/), and soft-deletes records by setting `isDeleted`. The project is an ES module (`"type": "module"`).

## Endpoints

Paths are relative to the API Gateway stage URL (for example `https://<api-id>.execute-api.<region>.amazonaws.com/Prod`, or `http://127.0.0.1:3000` with `sam local start-api`). Every route except `/health` requires an API key sent as `x-api-key: <value>`; see [Deploy](#deploy) for reading it.

- `GET /health` — health check
- **KittenClaws** (storage: `kittenclaws`)
  - `GET /kittenclaws?limit=100` — list (default 100, max 1000)
  - `GET /kittenclaws/{id}` — get by ID
  - `POST /kittenclaws` — create
  - `PATCH /kittenclaws/{id}` — partial update
  - `DELETE /kittenclaws/{id}` — soft delete

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

API Gateway only routes the methods and paths listed in `template.yaml`, one per generated operation. Any other method or path is answered by API Gateway (and by `sam local start-api`) with `403 {"message": "Missing Authentication Token"}` without invoking the Lambda, so the handler's JSON 404 and 405 answers only apply to requests that reach it.

## Storage

Each resource is stored in the DynamoDB table named by its `container` setting. The name can be overridden per container with an environment variable:

| Container | Environment variable | Default | Resources |
|---|---|---|---|
| `kittenclaws` | `DYNAMODB_TABLE_NAME_KITTENCLAWS` | `kittenclaws` | KittenClaws |

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

`KittenClaws` (`/kittenclaws`) fields:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned | Description |
|---|---|---|---|---|---|---|---|---|
| `name` | string | yes | no | none | min_length: 1 | POST, PATCH | yes | none |

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
| `AWS_REGION` | no | Region of the DynamoDB tables (default `us-east-1`; set by Lambda at runtime) |
| `AWS_ENDPOINT_URL_DYNAMODB` | no | DynamoDB endpoint override, read by the AWS SDK; local development only |
| `DYNAMODB_TABLE_NAME_KITTENCLAWS` | no | DynamoDB table for `kittenclaws` (default `kittenclaws`) |

Locally the variables are read from the process environment and from a `.env` file in the project root (loaded quietly, without a startup banner).

## Prerequisites

- [Node.js](https://nodejs.org/) 24 (LTS)
- [Yarn](https://yarnpkg.com/) 4, enabled with `corepack enable`
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) and [Docker](https://www.docker.com/) for local runs
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) and an AWS account

## Setup

```console
corepack enable
yarn install
yarn build
```

## Run locally

Build the project, then start API Gateway and Lambda locally with SAM (requires Docker):

```console
yarn build
sam build
sam local start-api
```

The API is served at `http://127.0.0.1:3000`. The functions reach DynamoDB with your local AWS credentials; the table names come from `template.yaml`. To use DynamoDB Local instead, see [Run locally against the emulator](#run-locally-against-the-emulator).

The `.thunderclient` directory contains a [Thunder Client](https://www.thunderclient.com/) collection with a request for every generated operation, a few error cases, and a `baseUrl` that matches the local server above.

## Run locally against the emulator

`docker-compose.yml` runs [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2 and the SAM CLI; with Podman, run the `podman compose` equivalents of the `emulator:*` scripts.

```console
yarn install
yarn emulator:up      # docker compose up -d --wait: returns once the emulator is healthy
yarn emulator:seed    # creates one table per storage container; safe to re-run
yarn start:emulator   # yarn build and sam build, then sam local start-api on http://127.0.0.1:3000 with the emulator settings
yarn emulator:down    # docker compose down -v: stops the emulator and discards its data
```

`yarn emulator:logs` follows the emulator's logs. `yarn emulator:seed` loads the settings in `.env.emulator` (through `DOTENV_CONFIG_PATH`, overriding variables already set), and `yarn start:emulator` passes `env.emulator.json` to `sam local`. `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as `.env` or `.env.local`, which git ignores. The emulator keeps no data outside its container, so `yarn emulator:down` (or removing the container) discards every record.

| Port | Purpose |
| --- | --- |
| 8000 | DynamoDB Local (`-inMemory -sharedDb`, so tables do not depend on the region or access key) |

- `yarn emulator:seed` runs on your machine and reaches DynamoDB Local at `http://localhost:8000` (`.env.emulator`). `sam local start-api` runs the function in a container on the `kittenclaws-emulator` Docker network, so `env.emulator.json` points it at `http://dynamodb:8000` and sets the table names, which `sam local` would otherwise take from the template's logical ids. `--warm-containers LAZY` keeps each function's container running between requests and restarts it when the build changes.
- `template.yaml` declares `AWS_ENDPOINT_URL_DYNAMODB` only so `sam local` can set it; deployed stacks leave it out unless you pass the `DynamoDbEndpoint` parameter. Every AWS SDK reads the variable natively.
- `yarn emulator:seed` and `yarn start:emulator` use dummy credentials, which replace your own for these commands. DynamoDB Local 3 accepts only letters and digits in an access key id.

Startup takes a few seconds; the healthcheck allows up to 2 minutes. `yarn emulator:seed` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`yarn emulator:up` fails or never turns healthy:** check `yarn emulator:logs`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** run `yarn emulator:seed`; the tables do not exist until it has run, and `yarn emulator:down` deletes them.
- **`network kittenclaws-emulator not found`:** run `yarn emulator:up` before `yarn start:emulator`.
- **`UnrecognizedClientException`:** the function did not get the dummy credentials; run it through `yarn start:emulator`, which passes them to `sam local`.

## Deploy

`template.yaml` defines the Lambda function (Node.js 24, `nodejs24.x`), one API Gateway route per generated operation, and one DynamoDB table per container.

```console
yarn build
sam build
sam deploy --guided
```

SAM creates an API key and usage plan for the API. Every route except `/health` requires the key in an `x-api-key` header. Read its value from the `KittenClawsApiKeyId` stack output:

```console
aws apigateway get-api-key --api-key <api-key-id> --include-value --query value --output text
curl -H "x-api-key: <value>" https://<api-id>.execute-api.<region>.amazonaws.com/Prod/kittenclaws
```

`sam local start-api` does not enforce API keys. An API key identifies a caller but is not strong authentication; for that, add an IAM, Cognito or Lambda authorizer.

## Development

```console
yarn lint        # ESLint
yarn format      # ESLint --fix and Prettier
yarn test:unit   # Jest with coverage thresholds
yarn audit       # yarn npm audit --severity moderate
```

## Repository structure

Each resource gets its own file in every layer, named after the resource in lowerCamelCase. Each layer's `index.ts` re-exports them.

```text
├── .env.emulator                   - Public settings for the local emulator
├── .thunderclient                  - Thunder Client collection, one folder per resource
├── config
│   └── container.ts                - Wiring of repositories, services and controllers
├── controllers                     - Request validation
│   └── kittenClaws.controller.ts
├── repositories                    - Data access
│   ├── base.repository.ts          - CRUD, timestamps and soft deletes over a document store
│   ├── document.store.ts           - Store interface
│   ├── dynamo.store.ts             - DynamoDB store
│   └── kittenClaws.repository.ts
├── routes                          - Per-resource HTTP method routing
│   ├── health.routes.ts
│   ├── response.ts
│   └── kittenClaws.routes.ts
├── scripts
│   └── bootstrapEmulator.ts        - Prepares the local emulator (yarn emulator:seed)
├── services                        - Business logic
│   ├── schemaValidator.service.ts
│   └── kittenClaws.service.ts
├── types
│   ├── errors                      - Error types
│   └── models                      - Zod models and environment schema
│       └── kittenClaws.schema.ts
├── test                            - Shared test mocks
├── utils                           - Error mapping, JSON body parsing, list limits, clock and ids
├── docker-compose.yml              - Local DynamoDB emulator
├── env.emulator.json               - sam local settings for the emulator
├── lambda.ts                       - Lambda handler
├── template.yaml                   - AWS SAM template
└── package.json                    - Scripts and dependencies
```

Unit tests sit in a `__tests__` directory next to the code they cover, one file per resource (for example `controllers/__tests__/kittenClaws.controller.test.ts`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
