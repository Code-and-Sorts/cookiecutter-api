# AGENTS.md

Guidelines for AI agents working on the Cookiecutter API repository.

## Project Overview

This is a [Copier](https://github.com/copier-org/copier) template repository that generates REST API projects across multiple languages and cloud platforms. Each generated project follows the **controller-service-repository** pattern and exposes one or more configurable REST resources.

## Repository Structure

```
cookiecutter-api/
├── copier.yml               # The one Copier config: questions, derived values, validators
├── shared/                  # Files identical across languages, included by each language's copy
│   └── infra/               # Atmos stacks and the per-cloud Terraform component (include_infrastructure)
├── python/template/         # Python template (Azure + GCP + AWS)
├── typescript/template/     # TypeScript/Node.js template (Azure + GCP + AWS)
├── dotnet/template/         # .NET/C# template (Azure + GCP + AWS)
├── go/template/             # Go template (Azure + GCP + AWS)
├── .github/
│   ├── actions/             # Shared composite actions and resource fixtures
│   └── workflows/           # CI pipelines per language, infrastructure validation, integration tests, example publishing
├── tests/integration/       # Black-box HTTP suite for the API contract, run against the emulators
├── .claude/skills/           # Agent skills for this repository (integration-report)
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

Every template asks a `resources` question: a list of `{name, endpoint, container, operations}`.
The default is a single resource derived from the project name with `list`, `get_by_id`,
`create`, `update` and `delete`.

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
  `container_class` (PascalCase container), `has_body`, `has_collection`, `has_item` and
  `has_dto`. Keep every repository path under about 200 characters too: Git for
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

### API Contract

Every language and cloud must generate the same HTTP behaviour; change all four templates together.

- **Routes:** `{prefix}/<endpoint>` and `{prefix}/<endpoint>/{id}`, where the prefix is `/api` on Azure
  Functions (the host default) and empty on GCP and AWS. The health check is `GET {prefix}/<health_endpoint>`,
  matched exactly. SAM path parameters are named `{id}`. GCP projects expose one HTTP function: entry point `api`, or the `Function` class in .NET, whose
  Functions Framework resolves entry points by type name.
- **Responses:** always JSON. Create 201; get, update, replace 200 with `id`, `name`, `createdTimestamp`,
  `updatedTimestamp`, and `createdBy`/`updatedBy` only when stored (never `null`), and never `isDeleted`; list 200
  with an array of the same (`[]` when empty); delete 200 with `{"message": "<Name> with id <id> was deleted successfully."}`; health 200
  with `{"status": "ok"}`.
- **Errors:** `{"errorMessage": "..."}`. 400 for malformed JSON, a non-object body, a missing (create and replace;
  update keeps the stored name), empty or non-string `name`, or any unknown field (including `id` and system fields, so a create can never overwrite a record);
  404 for unknown, deleted or non-UUID ids and unknown paths; 405 for a method the resource does not enable
  when the request reaches app code; 500 with a generic message for anything else, logged with its stack trace
  and never echoed to the client. API Gateway (403) and the Azure host (404) answer some unmapped methods
  before app code runs.
- **Storage:** `id`, `name`, `isDeleted`, `createdTimestamp`, `updatedTimestamp` (ISO 8601 UTC, milliseconds,
  `Z`), plus `createdBy`/`updatedBy` only when set. Create reads the clock once and uses that value for both
  `createdTimestamp` and `updatedTimestamp` (never a separate default per field, which can differ by a
  millisecond). Update and replace keep the created fields; delete is soft. A Cosmos DB write after a read sends the
  ETag it read (If-Match), so a racing write is a 500, never lost.
- **User id:** the optional `X-User-Id` header (trimmed, at most 256 characters, else 400) is the only source of
  `createdBy`/`updatedBy`. Create sets both; update, replace and delete set `updatedBy`, and a write without the
  header removes it. Bodies still reject both fields. The header is not authenticated; every generated README says so.
- **Auth:** resource routes need credentials and health is open where the platform allows it. Azure: function keys,
  anonymous health function; with `include_infrastructure`, an API Management API key (`x-api-key`) instead, health
  open, and API Management calls the app with an Entra ID token (App Service authentication), so the functions'
  auth level is `function_auth_level` (`anonymous`). AWS: API Gateway API keys (`x-api-key`, SAM usage plan), health exempt. GCP: IAM
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

### Infrastructure

`include_infrastructure` (asked only for Azure until GCP and AWS land) renders `infra/`, an Atmos project, and
`.github/workflows/deploy.yml` (OIDC login; plans the first stack on pull requests, deploys every stack in order on
`main`, then publishes the code with Core Tools or pushes the image to the registry).

- **Stacks are the same shape on every cloud.** One Atmos component, `api`, with cloud-neutral variables: `name`,
  `stage`, `region`, `tags`, `network`, `compute` (`hosting`, `sku`, `runtime`, scaling, `app_settings`),
  `database` (`capacity`, `throughput`, `containers`) and `gateway` (`sku`, `capacity`, `routes`,
  `health_endpoint`). Only the values differ per cloud. `stacks/catalog/defaults.yaml` holds the state backend,
  `stacks/catalog/api.yaml` what every stack shares (rendered from `language`, `resources` and `health_endpoint`),
  and `stacks/deploy/<stage>.yaml` (one per `infra_environments` item, through `yield` over `path_environments`)
  the region and tiers. `name_pattern: "{stage}"`, so stacks are `dev`, `prod` and so on.
- **Each cloud's options are data.** `infra_clouds` in `copier.yml` maps a `cloud_service` to its `slug`, default
  `region`, `compute.hostings` (each with a default `sku`, a `skus` regex and whether it runs a `container` image),
  `database.capacities` (each with an optional `throughput` default, `min` and `step`) and `gateway.skus` (each with
  a `max_capacity`), plus the default of each. `infra_cloud` is the chosen cloud's entry, and
  `include_infrastructure` is only asked when it exists. The questions `infra_region`, `infra_compute_hosting`,
  `infra_compute_sku`, `infra_database_capacity`, `infra_database_throughput`, `infra_gateway_sku`,
  `infra_gateway_capacity` and `infra_admin_email` take their defaults, help and validation from `infra_cloud`, so
  they have no `choices` (Copier checks a skipped question's default against its choices). Each
  `infra_environments` item can override any of them without the `infra_` prefix. `path_environments` resolves each
  stack's values (a SKU follows its hosting unless set); the `infra_environments` validator repeats that
  resolution, because Copier has not computed `path_environments` when it runs.
- **Sources:** everything lives once in `shared/infra/` and each language includes it file by file. Cloud-neutral
  files render under `{{ infra_dir }}`; a cloud's Terraform lives in `shared/infra/components/terraform/api/<cloud>/`
  and renders under `{{ infra_<cloud>_dir }}/components/terraform/api/` (empty for other clouds), so a project
  always gets `components/terraform/api`. Language differences (runtime, app setting names) belong in
  `stacks/catalog/api.yaml`, never in Terraform. Cloud-specific prose and steps go in partials the shared files
  include for that cloud: `shared/infra/_README.<cloud>.md` and `shared/.github/workflows/_deploy.<cloud>.yml`
  (login and credentials are a branch in `deploy.yml`).
- **App settings** are names the code reads, with `${database_endpoint}` and `${database_name}` filled in by
  Terraform (`templatestring`); the per-container names default to the container id, so they match the store
  Terraform creates. CI checks every name appears in the generated code.
- **Azure:** API Management (`Developer`, `StandardV2` or `Premium`, the tiers that reach a private backend) in the
  virtual network; Functions (Flex Consumption, App Service plan or Premium) behind a private endpoint, or Container
  Apps in an internal environment; Cosmos DB with public access and keys off, reached by a user-assigned identity
  (`AZURE_CLIENT_ID`) through a private endpoint, which is why every language's Cosmos client uses
  `DefaultAzureCredential` when no key is set; a storage account admitting only the app subnet.
- **Tests:** `components/terraform/api/tests/*.tftest.hcl` plan against mocked providers (`tests/mocks/`), so
  `terraform test` needs no account; `tests/stacks` plans one stack's real variables
  (`atmos terraform generate varfile`). Add a `run` for every new branch in the component.

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
8. **Add its infrastructure** (see [Infrastructure](#infrastructure)) — an `infra_clouds` entry, a component in
   `shared/infra/components/terraform/api/<cloud>/` with the same variables and its `terraform test` suite, an
   `infra_<cloud>_dir` derived value and the one-line includes in every language, the backend in
   `stacks/catalog/defaults.yaml`, the cloud's app settings in `stacks/catalog/api.yaml`, its login in
   `shared/.github/workflows/deploy.yml` and its publish steps in `_deploy.<cloud>.yml`, `shared/infra/_README.<cloud>.md`,
   `fixtures/<cloud>-infra-environments.yml`, and the cloud in `validate-infra.yaml`
9. **Update `README.md`** — Change the support table cell from planned to complete

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
6. Include `shared/docker-compose.yml`, `shared/.env.emulator` (add the language's Cosmos setting names, and the
   same names to `COSMOS_SETTINGS` in `tests/integration/store.py`),
   `shared/env.emulator.json` (AWS), `shared/_Makefile.emulator` and `shared/_README.emulator.md`, and add
   a bootstrap command and the run command (see [Local Emulators](#local-emulators))
7. Add the language's runtime version to `runtime_versions` in `copier.yml` and its setup step to `.github/actions/setup-runtime`, create `.github/workflows/build-{language}-pipeline.yaml`
   (its path filters include `copier.yml` and `shared/**`), and add the language to `publish-examples.yml`, the
   `template-setup.yml` language map and the setup issue form
8. Add the language to the integration tests: its install and emulator commands in
   `.github/actions/start-local-api` and the language in the `integration-tests.yaml` plan
9. Include the `shared/infra/` files, `shared/.github/workflows/deploy.yml` and `shared/_gitignore.infra`, add the
   language's runtime and app setting names to `shared/infra/stacks/catalog/api.yaml` and its build and publish
   steps to the deploy workflow, a `Dockerfile` for `infra_containers`, and the language to `validate-infra.yaml`
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
| `resources` | REST resources to generate | see [Resources](#resources) |
| `author` | Project author | `"Your Name"` |
| `open_source_license` | License type | `"MIT license"` |
| `include_infrastructure` | Generate `infra/` and the deploy workflow (Azure for now) | `false` |
| `infra_region`, `infra_compute_hosting`, `infra_compute_sku`, `infra_database_capacity`, `infra_database_throughput`, `infra_gateway_sku`, `infra_gateway_capacity`, `infra_admin_email` | Defaults for every stack | `"eastus"`, `"flex_consumption"`, `"FC1"`, `"serverless"`, `400`, `"Developer"`, `1`, `"admin@example.com"` |
| `infra_environments` | Stacks, each `{name, ...overrides}` | `[{name: dev}, {name: prod}]` |

`project_slug`, `project_endpoint`, `project_class_name`, and `project_lower_camel_name`
are derived from `project_name` via `when: false` questions, so they are computed
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
names of differing lengths) and no health check. `multi` only feeds the published example branches.
Every language's pipeline also runs when the root `copier.yml` or `shared/` changes.
`.github/actions/setup-runtime` sets up the language's runtime and package manager for the build
pipelines and the integration tests alike, plus a pinned uv for Python projects and, with `uv: "true"`, for the
integration suite in every language.

Pipelines use a small matrix, one job per distinct risk rather than every combination:
- Ubuntu: every cloud service, with the default single resource and with `edge`
- Windows (path length, checkout) and macOS (BSD tools) once each, on different clouds
- The newest GA runtime each cloud supports, set once in `runtime_versions` in `copier.yml` (with per-cloud
  overrides under `clouds`, such as Node on Azure Functions); `setup-runtime` and `setup-copier-template` read it

Add a job or fixture only for a combination no existing job exercises; fold new resource shapes into `edge`.

Each language pipeline has one more Ubuntu job (`include-infrastructure: "true"`, Azure, `edge`) that builds and
tests the code rendered with infrastructure. `validate-infra.yaml` renders every language with
`fixtures/<cloud>-infra-environments.yml` (one stack per hosting, database capacity and gateway tier) and runs `terraform fmt`,
`validate`, TFLint, the component's `terraform test`, `atmos validate stacks`, a mocked plan of every stack, the app
setting name check, actionlint on `deploy.yml` and Checkov (`.checkov.yaml` lists the skipped checks; the generated
`infra/README.md` gives each one's reason). Nothing in this repository's CI deploys or holds cloud credentials.

### Integration Tests

`tests/integration` is one pytest suite for every language and cloud. It talks to a running project over HTTP
and checks the [API contract](#api-contract): each enabled operation, validation and ids, soft deletes, disabled
operations, shared and separate containers, `?limit=`, the health check, `X-User-Id` and the stored record
format. It reads `resources` from the project's `.copier-answers.yml` and parametrizes itself (`@pytest.mark.ops`
and `@pytest.mark.each_operation` pick the resources and operations a test needs), so never render tests with
Jinja. `store.py` reads records straight from the emulator, using the project's `.env.emulator`, and seeds a
container for a resource that cannot `create`. Contract changes go into the suite with the template change.
The suite is a uv project with its own `uv.lock`: `uv sync --project tests/integration`, then
`uv run --project tests/integration pytest tests/integration --project-dir <project> --base-url <url>`.

`.github/workflows/integration-tests.yaml` runs every language x cloud x fixture (`single` and `edge`; `multi` on
request) on pushes to `main` and on `workflow_dispatch`, never on pull requests: dispatch it on your branch before merging a
contract or emulator change. Each job renders the project, then `.github/actions/start-local-api` starts the
emulator and host with the project's own commands (`make emulator-up emulator-seed run-emulator`, or the
TypeScript `yarn` scripts) and outputs the base URL. Failed jobs upload the host and emulator logs, the JUnit
XML and the project.

`--record <file>` makes the suite write every request it sends, the response, and the stored document before and
after each write, one JSON line each (`recorder.py`); the workflow records every job, uploads the file as a
`requests-<language>-<cloud>-<fixture>` artifact and prints it in the job log. The `integration-report` skill
(`.claude/skills/integration-report/`) runs the workflow and turns those logs into one HTML page for review.

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
(Node, Python, .NET, Go, and uv) are bumped by hand once Azure Functions, Cloud Run functions and AWS Lambda all
support the new version GA, in `runtime_versions` in `copier.yml`: templates render `runtime` (those versions with the
chosen cloud's overrides) into project files, generated pipelines, Dockerfiles and the stacks, and CI reads the same
values. Its `terraform` manager bumps the provider pins in
`shared/infra/components/terraform/api/*/versions.tf`, and regex managers the Atmos and Terraform versions in
`infra_tools` in `copier.yml`, the one place both the generated deploy workflow and `validate-infra.yaml` read them
from. Atmos telemetry is off in `shared/infra/atmos.yaml`.

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
