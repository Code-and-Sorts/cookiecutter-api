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

Stored records hold `id`, `name`, `isDeleted`, `createdTimestamp` and `updatedTimestamp`, plus `createdBy`/`updatedBy` only when they are set (they are never stored as null or empty). Timestamps are ISO-8601 UTC with millisecond precision, for example `2026-09-29T22:49:26.625Z`. Create sets both timestamps; PATCH and PUT keep the stored `createdTimestamp` and `createdBy` and refresh `updatedTimestamp`; DELETE is a soft delete that sets `isDeleted: true` and refreshes `updatedTimestamp`. Every write sets or removes `updatedBy` from the `X-User-Id` header.

## Environment variables

| Variable | Required | Description |
|---|---|---|
| `AWS_REGION` | no | Region of the DynamoDB tables (default `us-east-1`; set by Lambda at runtime) |
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

The API is served at `http://127.0.0.1:3000`. The functions reach DynamoDB with your local AWS credentials; the table names come from `template.yaml`.

The `.thunderclient` directory contains a [Thunder Client](https://www.thunderclient.com/) collection with a request for every generated operation, a few error cases, and a `baseUrl` that matches the local server above.

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
├── services                        - Business logic
│   ├── schemaValidator.service.ts
│   └── kittenClaws.service.ts
├── types
│   ├── errors                      - Error types
│   └── models                      - Zod models and environment schema
│       └── kittenClaws.schema.ts
├── test                            - Shared test mocks
├── utils                           - Error mapping, JSON body parsing, list limits, clock and ids
├── lambda.ts                       - Lambda handler
├── template.yaml                   - AWS SAM template
└── package.json                    - Scripts and dependencies
```

Unit tests sit in a `__tests__` directory next to the code they cover, one file per resource (for example `controllers/__tests__/kittenClaws.controller.test.ts`).

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
