# AGENTS.md

Guidelines for AI agents working on the Cookiecutter API repository.

## Project Overview

This is a [Copier](https://github.com/copier-org/copier) template repository that generates REST API projects across multiple languages and cloud platforms. Each generated project follows the **controller-service-repository** pattern and exposes one or more configurable REST resources.

## Repository Structure

```
cookiecutter-api/
├── copier.yml               # The one Copier config: questions, derived values, validators
├── shared/                  # Files identical across languages, included by each language's copy, and _fields.jinja
├── python/template/         # Python template (Azure + GCP + AWS)
├── typescript/template/     # TypeScript/Node.js template (Azure + GCP + AWS)
├── dotnet/template/         # .NET/C# template (Azure + GCP + AWS)
├── go/template/             # Go template (Azure + GCP + AWS)
├── .github/
│   ├── actions/             # Shared composite actions, resource fixtures and invalid answers files
│   ├── scripts/             # Template checks: invalid answers, defaults guard, cross-language base types
│   └── workflows/           # CI pipelines per language, integration tests, example publishing
├── tests/integration/       # Black-box HTTP suite for the API contract, run against the emulators
├── .docs/                   # Documentation assets (images, SVGs)
└── README.md                # Support matrix and usage docs
```

The repository root is the Copier template root. `copier.yml` asks `language` first and sets
`_subdirectory: "{{ language }}/template"`, so only that language's tree is rendered into the
destination; `shared/`, the other languages and the repository files are never copied. Template
files include and import by path from the repository root, for example
`{% include 'shared/LICENSE' -%}` or `{% from 'python/_macros.jinja' import routes %}`
(Copier forbids includes outside the template root, which is why the root is the repository).

### Shared files

A file that would be identical in two or more languages lives once in `shared/`, and each
language's copy is a one-line `{% include 'shared/<path>' -%}` (the `-` keeps the included
file's own trailing newline as the only one). `shared/` mirrors the generated path
(`shared/LICENSE`, `shared/.vscode/extensions.json`); a partial that is included into a
differently named file starts with `_`. Only share what is really the same: a file that
differs by language or needs per-language conditionals stays in each `template/`.
`shared/_fields.jinja` is the one exception: it is never included into a project but imported by
`copier.yml` and the templates, and holds everything about fields that is the same in every
language (see [Models](#models)). Per-language rendering macros live in `<language>/_model.jinja`.

## Template Architecture

All templates follow this layered architecture:

```
Entry Point (Azure functions, GCP main, or AWS Lambda handler)
  → Controller (request validation, routing)
    → Service (business logic, schema validation)
      → Repository (database operations)
        → Database (Cosmos DB, Firestore, or DynamoDB)
```

### Key Patterns

- **Dependency Injection**: .NET uses `Microsoft.Extensions.DependencyInjection`, TypeScript uses [Inversify](https://inversify.io/) (`config/container.ts`); Python (blueprint/entry point) and Go (`main.go`, or `function.go` on GCP) wire dependencies manually
- **Schema Validation**: TypeScript uses [Zod](https://zod.dev/), Python uses [Pydantic](https://docs.pydantic.dev/), .NET uses [FluentValidation](https://docs.fluentvalidation.net/), Go uses JSON Schema
- **Soft Deletes**: All templates use an `isDeleted` flag rather than hard deletes
- **Base Records**: All entities extend a base schema with `id`, `isDeleted`, `createdTimestamp`, `updatedTimestamp`

### Resources

Every template asks a `resources` question: a list of `{name, endpoint, container, operations,
fields, requests}`. The default is a single resource derived from the project name with `list`,
`get_by_id`, `create`, `update` and `delete` and one field, `name`.

- Each resource gets its own named types (for example `CatController`, `CatService`,
  `CatRepository`; in .NET the stored entity is `CatEntity`) and its own file in every
  layer: entry point/handlers, controller, service, repository, models and tests. A
  per-resource file is a single template whose path uses Copier's `yield` tag, for
  example `controllers/{% yield resource from path_resources %}{{ resource.lower_camel_name }}.controller.ts{% endyield %}`;
  Copier renders it once per resource with `resource` in context (`resources` is still the
  full list). Git for Windows cannot check out a path containing `|`, so paths never use
  Jinja filters: `path_resources` (a derived `when: false` copy of `resources`, the same for
  every language) precomputes the names and flags templates need: `snake_name`,
  `lower_camel_name`, `container_key` (`-` becomes `_`), `env_key` (that, upper case),
  `container_class` (PascalCase container), `has_body`, `has_collection`, `has_item`,
  `has_dto`, and the normalized `fields` and `client_fields` (see [Models](#models)). Keep every repository path under about 200 characters too: Git for
  Windows fails checkout past 260 characters including the clone directory (Copier clones the
  template into the system temp directory), so long
  conditions belong in a derived value (for example .NET's `azure_dir`/`gcp_dir`/`aws_dir`, which
  render the cloud's folder name or nothing, and `body_resources` for resources that accept a request body). Only one `yield` is allowed per path segment and none inside file contents,
  so files common to all resources (base repository, base entity, errors, DI wiring, env schema, barrels)
  still loop over `resources`. This needs Copier 9.18.2+ (`_min_copier_version`).
- Controllers, services, routes and repositories expose only the resource's `operations`
  (`update` is PATCH, `replace` is PUT). Database code lives once per cloud, never per
  resource: a shared base repository (TypeScript `BaseRepository`, Python `BaseRepository`,
  .NET `EntityRepository`, Go `store[T]`) holds the CRUD, merging and not-found logic, and a
  per-cloud store does the SDK calls. The base keeps all six operations and its own tests,
  so coverage holds when no resource uses one.
- Resources with the same `container` share one store and see each other's records; there
  is no type discriminator.
- GCP and AWS read per-container settings named `FIRESTORE_COLLECTION_<CONTAINER>` and
  `DYNAMODB_TABLE_NAME_<CONTAINER>` (upper case, `-` becomes `_`). Azure setting names
  follow each language's existing convention.
- The health check is its own handler file per cloud, served at `health_endpoint` (default
  `health`) and generated only when that answer is non-empty.
- The `resources` validator rejects an endpoint equal to `health_endpoint`, the name `Health`, duplicate
  operations, names or containers that collide after case and separator normalization, and
  per-language reserved names. It is one validator in the root `copier.yml`: checks every
  language shares run for all, and reserved names and generated-name clashes branch on
  `language`. Only add a reserved name after rendering it and watching the generated project
  fail to build.

### Models

`base_model` (a list of fields, before `resources`) and each resource's `fields` share one field
schema: `name`, `type`, `required`, `nullable`, `default`, `immutable`, `hidden`, `values`, `items`,
`rules`, `example`, `description`; `requests.<create|replace|update>` narrows the fields a body accepts.
The six pre-populated `base_model` fields (`id`, `isDeleted`, the timestamps, `createdBy`,
`updatedBy`) are locked and `managed` (set by the server; `$user` is the `X-User-Id` header); only
their `description` may change, and `managed` is refused anywhere else. README.md documents the keys.

- `shared/_fields.jinja` holds the field rules once: `check_field` (used by the `base_model` and
  `resources` validators, which stop at the first error and name the resource or `base_model` and the
  field), the value `PATTERNS`, `MAX_SAFE_INTEGER`, `REQUEST_KINDS`, `DATE_TIME_EXAMPLE`, `DEFAULT_FIELDS`
  and the derived field data. Add a rule, type or derived value there, never per language.
- Templates never read the raw answers for fields: `base_fields`, `client_base_fields` and
  `path_resources[].fields`/`client_fields` hold normalized dicts with `pascal`, `item_type`,
  `enum_values`, flags with defaults filled in, `dynamic` (`now`, `today`, `uuid`, `user` or empty),
  static `default`, `read_default`/`has_read_default`, `needs_value` (required without a default),
  `defaulted` (create and replace fill a value), `in_create`/`in_replace`/`in_update`, and test data:
  `sample` (a valid value), `rejected` (a wrongly typed value, null unless nullable, every rule
  violation) and `boundaries` (values on each rule's bound). A missing `fields` answer (an older
  answers file) falls back to `DEFAULT_FIELDS`; a missing `base_model` gets the question default.
- Every language renders `BaseEntity`, `BaseCreateRequest`, `BaseReplaceRequest`,
  `BaseUpdateRequest` and `BaseResponse` from `client_base_fields`, and each resource's entity,
  request and response types extend them; generate code from loops over the fields, not per type.
- Field names are checked against one union list of reserved names, so an answer valid in one
  language is valid in all; resource names keep their per-language reserved lists.

### API Contract

Every language and cloud must generate the same HTTP behaviour; change all four templates together.

- **Routes:** `{prefix}/<endpoint>` and `{prefix}/<endpoint>/{id}`, where the prefix is `/api` on Azure
  Functions (the host default) and empty on GCP and AWS. The health check is `GET {prefix}/<health_endpoint>`,
  matched exactly. SAM path parameters are named `{id}`. GCP projects expose one HTTP function: entry point `api`, or the `Function` class in .NET, whose
  Functions Framework resolves entry points by type name.
- **Responses:** always JSON. Create 201; get, update, replace 200 with `id` and every field that is not
  `hidden` (`{"id", "name"}` with the default answers), `null` for a field without a value; list 200 with an array
  (`[]` when empty); delete 200 with `{"message": "<Name> with id <id> was deleted successfully."}`; health 200
  with `{"status": "ok"}`.
- **Errors:** `{"errorMessage": "..."}`. 400 for malformed JSON, a non-object body, a missing required field
  without a default (create and replace), a wrongly typed value (never converted), `null` for a field that is
  not nullable, a broken rule, or any field the operation does not accept (unknown, server-managed such as `id`,
  immutable on update and replace, or outside `requests.<operation>`), so a create can never overwrite a record;
  404 for unknown, deleted or non-UUID ids and unknown paths; 405 for a method the resource does not enable
  when the request reaches app code; 500 with a generic message for anything else, logged with its stack trace
  and never echoed to the client. API Gateway (403) and the Azure host (404) answer some unmapped methods
  before app code runs.
- **Fields:** create and replace give a field the body leaves out its default (`$now`, `$today`, `$uuid` per
  write) or no value; replace keeps the fields it does not accept; update changes only the fields sent and
  `null` clears a nullable one. A record without a field reads as its static default (`null` if nullable).
  Date-times are accepted with any offset and stored in UTC with milliseconds; integers lie within
  ±9007199254740991; patterns, `email` and `uri` use the shared `PATTERNS`.
- **Storage:** `id`, `isDeleted`, `createdTimestamp`, `updatedTimestamp` (ISO 8601 UTC, milliseconds,
  `Z`) and every field that has a value (a field without one is not stored, never `null`), plus
  `createdBy`/`updatedBy` only when set. Create reads the clock once and uses that value for both
  `createdTimestamp` and `updatedTimestamp` (never a separate default per field, which can differ by a
  millisecond). Update and replace keep the created fields; delete is soft.
- **User id:** the optional `X-User-Id` header (trimmed, at most 256 characters, else 400) is the only source of
  `createdBy`/`updatedBy`. Create sets both; update, replace and delete set `updatedBy`, and a write without the
  header removes it. Bodies still reject both fields. The header is not authenticated; every generated README says so.
- **Auth:** resource routes need credentials and health is open where the platform allows it. Azure: function keys,
  anonymous health function. AWS: API Gateway API keys (`x-api-key`, SAM usage plan), health exempt. GCP: IAM
  invoker (deployed with `--no-allow-unauthenticated`); the one function means health needs the token too.
- `?limit=` on list is honoured everywhere; a missing, non-numeric or non-positive limit means 100, and more than
  1000 means 1000.
- A failing database yields the generic 500 within 10 seconds: database calls use a per-request deadline or
  capped retries, so the answer arrives well inside the platform timeout.

## Multi-Cloud Support

Cloud-specific code is handled through:

1. **Jinja2 conditionals** — `{% if cloud_service == '...' %}` blocks within files every cloud uses (repositories, configs, package manifests)
2. **Separate entry point files** — Each cloud has its own entry point (e.g., `functions/` for Azure, `main.ts` for GCP, `lambda.ts` for AWS)
3. **Conditional file/directory names** — Cloud-specific files and directories are named with a Jinja conditional (e.g. `{% if cloud_service == 'AWS Lambda' %}lambda.ts{% endif %}`). Copier skips any path that renders to an empty string, which replaces Cookiecutter's post-generation cleanup hooks.

### Local Emulators

Every generated project runs against a local emulator with no cloud account, and the
[integration tests](#integration-tests) run through the same commands and settings rather than their own.
The cross-cloud parts live once in `shared/`:

- `shared/docker-compose.yml` — Jinja renders only the chosen cloud's emulator (Cosmos DB vNext on
  8081 + Data Explorer 1234, Firestore on host 8085, DynamoDB Local `-inMemory -sharedDb` on 8000),
  with a healthcheck, pinned image tags (bumped by a Renovate regex manager), no volumes and the
  fixed network `{{project_endpoint}}-emulator`.
- `shared/.env.emulator` — the public emulator settings (committed; real credentials stay in untracked
  files). GCP and AWS settings are the same everywhere; the Azure block picks each language's Cosmos
  setting names with `language`.
- `shared/env.emulator.json` — the AWS `sam local start-api --env-vars ... --docker-network ...`
  settings, which reach `http://dynamodb:8000` and name one table per container.
- `shared/_Makefile.emulator` — the `COMPOSE` variable and the `emulator-up`, `emulator-seed`,
  `emulator-down` and `emulator-logs` targets plus the `run-emulator` header; each Makefile sets
  `emulator_seed` (its bootstrap command) before including it and writes its own `run-emulator` recipe.
- `shared/_README.emulator.md` — the "Run locally against the emulator" README section; each README
  sets `emulator` (make or yarn, setting names and per-language notes) and `emulator_settings`.

Per language: the bootstrap, the `run-emulator` recipe or TypeScript package scripts, and the client options.

- A language-native bootstrap that reuses the app's store name settings and client options:
  Python `scripts/bootstrap_emulator.py`, TypeScript `scripts/bootstrapEmulator.ts`, Go
  `cmd/bootstrap`, .NET `<Project>.Bootstrap` (in the solution). It creates one Cosmos DB container
  (partition key `/id`) or DynamoDB table (hash key `id`, on-demand) per unique `container`, only
  checks Firestore is reachable, refuses to run unless the emulator settings are present, retries
  for 2 minutes and is safe to re-run. Never create stores at app startup.
- Make targets `emulator-up`, `emulator-seed`, `emulator-down`, `emulator-logs`, `run-emulator`
  (Python, Go, .NET, from `shared/_Makefile.emulator`); package scripts `emulator:up`, `emulator:seed`, `emulator:down`,
  `emulator:logs`, `start:emulator` (TypeScript, through dotenv's `DOTENV_CONFIG_PATH`).
- Clients switch only on emulator settings, so production paths are unchanged: a Cosmos flag
  (`Cosmos_Db_Emulator`, `COSMOS_DB_EMULATOR`, `CosmosDbEmulator`) turns off endpoint discovery
  (.NET: Gateway mode + `LimitToEndpoint`) and skips certificate checks only for an `https://`
  endpoint; DynamoDB and Firestore rely on the SDKs reading `AWS_ENDPOINT_URL_DYNAMODB` and
  `FIRESTORE_EMULATOR_HOST` (.NET Firestore needs `EmulatorDetection.EmulatorOrProduction`). SAM
  templates declare `AWS_ENDPOINT_URL_DYNAMODB` behind the `DynamoDbEndpoint` parameter so
  deployed stacks omit it; `sam local` passes it empty otherwise, which .NET must ignore.
- Azure `local.settings.json` uses `"AzureWebJobsStorage": ""`: every trigger is HTTP, so no Azurite.

### Cloud → Database Mapping

| Cloud Provider | Database | TypeScript Client | Python Client | .NET Client | Go Client |
|---|---|---|---|---|---|
| Azure Function App | Cosmos DB | `@azure/cosmos` | `azure-cosmos` | `Microsoft.Azure.Cosmos` | `azcosmos` |
| GCP Cloud Function | Firestore | `@google-cloud/firestore` | `google-cloud-firestore` | `Google.Cloud.Firestore` | `cloud.google.com/go/firestore` |
| AWS Lambda | DynamoDB | `@aws-sdk/client-dynamodb` | `aioboto3` | `AWSSDK.DynamoDBv2` | `aws-sdk-go-v2/service/dynamodb` |

## Adding a New Cloud Provider

To add a new cloud provider to an existing language template:

1. **Update the root `copier.yml`** — Add the new option to the `cloud_service` question's `choices` (shared by every language; gate any per-language derived value or reserved name on `language`)
2. **Create the entry point** — Add the cloud-specific function entry point file(s) and any per-resource handlers (TypeScript `functions/` or `routes/`, Python `blueprints/`, .NET `Functions/` or `Handlers/`, Go `handlers/`), naming them with a `{% if cloud_service == '...' %}...{% endif %}` conditional so they are only generated for that cloud
3. **Add Jinja2 conditionals** to these files:
   - `package.json` / `pyproject.toml` / `.csproj` / `go.mod` — Cloud-specific dependencies
   - The per-cloud store (TypeScript `repositories/*.store.ts`, .NET `Repositories/*DocumentStore.cs`,
     Go `repositories/store.go`, Python `repositories/base_repository.py`) — Database client implementation
   - `config/container.ts` (TypeScript), blueprint wiring (Python), `DependencyInjection.cs` (.NET) or `main.go` / `function.go` (Go) — dependency wiring
   - `types/models/baseEnv.schema` — Environment variable definitions
4. **Name any cloud-specific files/directories conditionally** so they are omitted for the other clouds
5. **Add the local emulator** (see [Local Emulators](#local-emulators)) — a service in `shared/docker-compose.yml`,
   its branch in `shared/_README.emulator.md`, its settings in `shared/.env.emulator`, an emulator-only client
   option with unit tests, and a bootstrap branch
6. **Update CI pipeline** — Add the new cloud service to the `cloud-service` matrix in the workflow YAML
7. **Add it to the integration tests** — its host tool and base URL in `.github/actions/start-local-api`, the cloud in
   the `integration-tests.yaml` plan, its `CLOUDS` slug in `tests/integration/project.py`, its `NOT_ROUTED`
   statuses in `tests/integration/test_operations.py`, and a store in `tests/integration/store.py`
8. **Update `README.md`** — Change the support table cell from planned to complete

## Adding a New Language

1. Put the template project under `<language>/template/` (e.g., `java/template/`); there is no per-language `copier.yml`
2. In the root `copier.yml`, add the language to the `language` question's `choices`. The shared questions
   (`project_name`, `cloud_service`, `resources`, etc.) and `path_resources` already apply; add any derived value
   only this language needs as a `when: false` question gated on `language`
3. Add the language's input validation as `language` branches in the existing `project_name` and `resources`
   validators (Copier only runs validators for prompted questions, not for `when: false` derived values):
   its reserved names and any generated-name clashes
4. Include files that are identical to another language's from `shared/` (see [Shared files](#shared-files))
   instead of copying them, and move a file to `shared/` when it becomes identical
5. Use conditional file/directory names if supporting multiple cloud providers
6. Render the base and per-resource models from the normalized field data with macros in
   `<language>/_model.jinja` (see [Models](#models)), and add the language's base type extractor to
   `.github/scripts/base_consistency.py`
7. Include `shared/docker-compose.yml`, `shared/.env.emulator` (add the language's Cosmos setting names, and the
   same names to `COSMOS_SETTINGS` in `tests/integration/store.py`),
   `shared/env.emulator.json` (AWS), `shared/_Makefile.emulator` and `shared/_README.emulator.md`, and add
   a bootstrap command and the run command (see [Local Emulators](#local-emulators))
8. Add the language's runtime to `.github/actions/setup-runtime`, create `.github/workflows/build-{language}-pipeline.yaml`
   (its path filters include `copier.yml` and `shared/**`), and add the language to `publish-examples.yml`, the
   `template-setup.yml` language map and the setup issue form
9. Add the language to the integration tests: its install and emulator commands in
   `.github/actions/start-local-api` and the language in the `integration-tests.yaml` plan
10. Update the root `README.md` support table

## Template Variables

| Variable | Description | Example |
|---|---|---|
| `language` | Template language, asked first; picks `<language>/template` | `"python"`, `"typescript"`, `"dotnet"`, `"go"` |
| `project_name` | Human-readable name | `"My API"` |
| `project_endpoint` | REST endpoint (kebab-case) | `"my-api"` |
| `project_class_name` | PascalCase class name | `"MyApi"` |
| `project_lower_camel_name` | lowerCamelCase | `"myApi"` |
| `project_slug` | snake_case (the Python module name) | `"my_api"` |
| `cloud_service` | Target cloud platform | `"Azure Function App"` |
| `health_endpoint` | Health check URL segment; empty skips the health check | `"health"` |
| `base_model` | Fields every resource stores; six locked built-in fields, then your own | see [Models](#models) |
| `resources` | REST resources to generate, each with `fields` and optional `requests` | see [Resources](#resources) |
| `author` | Project author | `"Your Name"` |
| `open_source_license` | License type | `"MIT license"` |

`project_slug`, `project_endpoint`, `project_class_name`, and `project_lower_camel_name`
are derived from `project_name` via `when: false` questions, as are `base_fields`,
`client_base_fields` and `path_resources` from `base_model` and `resources`, so they are computed
automatically and never prompted. The `jinja2_strcase` extension provides `to_camel` and
`to_lower_camel` filters for case conversion, and `jinja2_time` provides the `{% now %}`
tag used in `LICENSE`.

## Testing

### CI Pipelines

Each language has a GitHub Actions workflow that:
1. Generates a project with `copier copy --defaults --trust --data language=<language>` from the repository root
2. Installs dependencies
3. Builds and lints the project
4. Runs unit tests

The shared composite action at `.github/actions/setup-copier-template/action.yaml` handles steps 1-2,
and on Linux runners also validates the emulator files (`docker compose config`, and `env.emulator.json`
as JSON) without starting them.
Its `template-language` input is passed as the `language` answer, and its `resources-fixture` input renders
`fixtures/<name>-resources.yml`. CI uses `edge`: every resource
shape the default single resource doesn't cover (each operation subset, shared and hyphenated containers,
names of differing lengths) and no health check, and `model`: every field type, rule and kind of default,
`requests` subsets, extra `base_model` fields and a resource with the default fields. `model-shared` (two
models in one container) and `edge` run in the integration tests; `multi` only feeds the published example
branches. Fold new field shapes into `model`.
Every language's pipeline also runs when the root `copier.yml` or `shared/` changes.

`.github/workflows/template-checks.yaml` runs three scripts from `.github/scripts/` on pull requests:
`check_invalid_answers.py` (each file in `.github/actions/setup-copier-template/invalid/` is rejected with
the message on its `# expect:` line; add one per new validator rule), `defaults_guard.py` (every language,
cloud and fixture renders the same without and with the default `fields` and `base_model`) and
`base_consistency.py` (every language renders the same base field names, kinds and flags).
`.github/actions/setup-runtime` sets up the language's runtime and package manager for the build
pipelines and the integration tests alike, plus a pinned uv for Python projects and, with `uv: "true"`, for the
integration suite in every language.

Pipelines use a small matrix, one job per distinct risk rather than every combination:
- Ubuntu: every cloud service, with the default single resource and with `edge`
- Windows (path length, checkout) and macOS (BSD tools) once each, on different clouds
- The newest GA runtime each cloud supports, set once in `setup-runtime`: Node 24 (Node 22 on Azure Functions),
  Python 3.14, .NET 10, Go 1.27

Add a job or fixture only for a combination no existing job exercises; fold new resource shapes into `edge`.

### Integration Tests

`tests/integration` is one pytest suite for every language and cloud. It talks to a running project over HTTP
and checks the [API contract](#api-contract): each enabled operation, validation and ids, soft deletes, disabled
operations, shared and separate containers, `?limit=`, the health check, `X-User-Id`, the stored record
format and every field's types, rules, defaults and nullability. It reads `base_model` and `resources` from the
project's `.copier-answers.yml` and parametrizes itself (`@pytest.mark.ops` and `@pytest.mark.each_operation`
pick the resources and operations a test needs, `@pytest.mark.fields` the field cases), so never render tests
with Jinja; `values.py` mirrors the sample and violation rules of `shared/_fields.jinja` in Python. `store.py` reads records straight from the emulator, using the project's `.env.emulator`, and seeds a
container for a resource that cannot `create`. Contract changes go into the suite with the template change.
The suite is a uv project with its own `uv.lock`: `uv sync --project tests/integration`, then
`uv run --project tests/integration pytest tests/integration --project-dir <project> --base-url <url>`.

`.github/workflows/integration-tests.yaml` runs every language x cloud x fixture (`single`, `edge`, `model` and
`model-shared`; `multi` on
request) on pushes to `main` and on `workflow_dispatch`, never on pull requests: dispatch it on your branch before merging a
contract or emulator change. Each job renders the project, then `.github/actions/start-local-api` starts the
emulator and host with the project's own commands (`make emulator-up emulator-seed run-emulator`, or the
TypeScript `yarn` scripts) and outputs the base URL. Failed jobs upload the host and emulator logs, the JUnit
XML and the project.

### Local Verification

To verify changes locally, generate a template and test it:

```bash
# Install dependencies
pip install copier jinja2-strcase jinja2-time

# Generate a project from the repository root, outside the repository
copier copy --defaults --trust --vcs-ref HEAD \
  --data language="typescript" \
  --data project_name="TestProject" \
  --data cloud_service="GCP Cloud Function" \
  --data-file .github/actions/setup-copier-template/fixtures/edge-resources.yml \
  . ../TestProject

# Build and test
cd ../TestProject
yarn install --no-immutable
yarn build
yarn test:unit
```

The repository root is a Git repository, so Copier renders from a clone of it: `--vcs-ref HEAD` picks the
checked-out commit rather than the newest tag, and uncommitted changes (including new, unignored files) are
added on top with a `DirtyLocalWarning`. Render outside the repository so a generated project is not swept
into the next render.

### Dependency Updates

Renovate keeps package versions current and merges its own PRs once every check passes
(see `renovate.json`), except integration test dependencies, which wait for a review. Its `pep621` manager
updates `pyproject.toml` and the matching `uv.lock` together. Runtime versions
(Node, Python, .NET, Go) are bumped by hand once Azure Functions, Cloud Run functions and AWS Lambda all support
the new version GA, in `.github/actions/setup-runtime`.

## Code Conventions

- **TypeScript**: ES modules (`"type": "module"`), Yarn for packages, Jest for tests (ESM mode; import `jest` and friends from `@jest/globals`, mock modules with `jest.unstable_mockModule`), path aliases (`@controllers`, `@services`, etc.) via `tsconfig.json` paths, rewritten to `.js` paths by `tsc-alias`
- **Python**: uv for packages (PEP 621 `pyproject.toml`, `[tool.uv] package = false`), pytest for tests, blueprint
  pattern for route registration; controllers are cloud-agnostic (plain values in, `utils/routing.py` resolves
  GCP/AWS routes), and AWS builds with SAM's makefile builder, which exports `uv.lock` and installs manylinux
  wheels with `uv pip install --target`; Azure and GCP deploy from `make requirements` (`uv export`), and `utils/deadline.py` bounds each request's database work to 8 seconds
- **.NET**: NuGet for packages, xUnit v3 for tests, solution/project structure
- **Go**: Go modules, `go test`, gofmt enforced through golangci-lint
- Template files use `{{ variable_name }}` in both filenames and content
- Comments only record a reason the code can't show (a platform or SDK quirk, a workaround, a security choice), in one short line; never restate what the code does
- Keep controllers, services, and error types cloud-agnostic — only repositories and entry points should contain cloud-specific code
