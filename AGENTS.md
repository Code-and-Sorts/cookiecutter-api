# AGENTS.md

Guidelines for AI agents working on the Cookiecutter API repository.

## Project Overview

This is a [Copier](https://github.com/copier-org/copier) template repository that generates REST API projects across multiple languages and cloud platforms. Each generated project follows the **controller-service-repository** pattern and exposes one or more configurable REST resources.

## Repository Structure

```
cookiecutter-api/
├── python/                  # Python template (Azure + GCP + AWS)
├── typescript/              # TypeScript/Node.js template (Azure + GCP + AWS)
├── dotnet/                  # .NET/C# template (Azure + GCP + AWS)
├── go/                      # Go template (Azure + GCP + AWS)
├── .github/
│   ├── actions/             # Shared composite actions, resource fixtures and the OpenAPI lint rules
│   └── workflows/           # CI pipelines per language, OpenAPI consistency, example publishing
├── .docs/                   # Documentation assets (images, SVGs)
└── README.md                # Support matrix and usage docs
```

Each language directory contains:
- `copier.yml` — Questions, derived values, validators, and Jinja extension config
- `template/` — The template project root (declared via `_subdirectory: template`); its
  contents are rendered directly into the destination directory
- `_openapi.yaml.jinja` — The OpenAPI document as YAML, included by `template/openapi.json`
  (`template/<Project>.Api/openapi.json` in .NET). Copier cannot share files between
  templates, so every language keeps a byte-identical copy; edit all four together

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
- **OpenAPI**: Every project has an OpenAPI 3.1 `openapi.json`, rendered from `_openapi.yaml.jinja` (the
  `from_yaml` and `to_json` filters convert it, so no project needs a YAML parser) and served at
  `GET {prefix}/openapi.json`.
  A contract test per language (Go `handlers/openapi_test.go`, Python `openapi_test.py`, TypeScript
  `__tests__/openapi.test.ts`, .NET `OpenApi/OpenApiSpecTests.cs`) fails when the spec's (method, path) set
  differs from the registered routes, or its request schemas from the validator's field names and required
  fields

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
  Jinja filters: `path_resources` (a derived `when: false` copy of `resources`) adds the
  `snake_name` and `lower_camel_name` stems. Keep every repository path under about 200 characters too: Git for
  Windows fails checkout past 260 characters including the clone directory, so long
  conditions belong in a derived value (for example .NET's `azure_dir`/`gcp_dir`/`aws_dir`, which
  render the cloud's folder name or nothing, and `body_resources` for resources that accept a request body). Only one `yield` is allowed per path segment and none inside file contents,
  so shared files (base repository, base entity, errors, DI wiring, env schema, barrels)
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
- `openapi.json` lists exactly the generated routes: per resource a `{Name}` schema and a
  `{Name}CreateRequest`/`UpdateRequest`/`ReplaceRequest` schema for each body operation it enables.
  The spec route has its own handler file per cloud too (Go keeps it in `handlers/openapi.go`, which no
  resource file name can collide with).
- The `resources` validator rejects an endpoint equal to `health_endpoint`, the name `Health`, duplicate
  operations, names or containers that collide after case and separator normalization, and
  per-language reserved names. Only add a reserved name after rendering it and watching
  the generated project fail to build.

### API Contract

Every language and cloud must generate the same HTTP behaviour; change all four templates together.

- **Routes:** `{prefix}/<endpoint>` and `{prefix}/<endpoint>/{id}`, where the prefix is `/api` on Azure
  Functions (the host default) and empty on GCP and AWS. The health check is `GET {prefix}/<health_endpoint>`,
  matched exactly. SAM path parameters are named `{id}`. GCP projects expose one HTTP function: entry point `api`, or the `Function` class in .NET, whose
  Functions Framework resolves entry points by type name.
- **OpenAPI:** `GET {prefix}/openapi.json` serves the project's `openapi.json` and has the health check's auth
  (anonymous on Azure, no API key on AWS, IAM on GCP), whether or not the health check exists. The spec's
  paths carry no prefix; its `servers` add `/api` on Azure. A change to any route (path, method, status,
  body or header) must update the loop in `_openapi.yaml.jinja` in the same change; the contract tests and
  the `openapi-consistency` workflow fail otherwise.
- **Responses:** always JSON. Create 201; get, update, replace 200 with `{"id", "name"}`; list 200 with an array
  (`[]` when empty); delete 200 with `{"message": "<Name> with id <id> was deleted successfully."}`; health 200
  with `{"status": "ok"}`.
- **Errors:** `{"errorMessage": "..."}`. 400 for malformed JSON, a non-object body, a missing, empty or non-string
  `name`, or any unknown field (including `id` and system fields, so a create can never overwrite a record);
  404 for unknown, deleted or non-UUID ids and unknown paths; 405 for a method the resource does not enable
  when the request reaches app code; 500 with a generic message for anything else, logged with its stack trace
  and never echoed to the client. API Gateway (403) and the Azure host (404) answer some unmapped methods
  before app code runs.
- **Storage:** `id`, `name`, `isDeleted`, `createdTimestamp`, `updatedTimestamp` (ISO 8601 UTC, milliseconds,
  `Z`), plus `createdBy`/`updatedBy` only when set. Create reads the clock once and uses that value for both
  `createdTimestamp` and `updatedTimestamp` (never a separate default per field, which can differ by a
  millisecond). Update and replace keep the created fields; delete is soft.
- **User id:** the optional `X-User-Id` header (trimmed, at most 256 characters, else 400) is the only source of
  `createdBy`/`updatedBy`. Create sets both; update, replace and delete set `updatedBy`, and a write without the
  header removes it. Bodies still reject both fields. The header is not authenticated; every generated README says so.
- **Auth:** resource routes need credentials and health is open where the platform allows it. Azure: function keys,
  anonymous health function. AWS: API Gateway API keys (`x-api-key`, SAM usage plan), health exempt. GCP: IAM
  invoker (deployed with `--no-allow-unauthenticated`); the one function means health needs the token too.
- `?limit=` on list is honoured everywhere.
- A failing database yields the generic 500 within 10 seconds: database calls use a per-request deadline or
  capped retries, so the answer arrives well inside the platform timeout.

## Multi-Cloud Support

Cloud-specific code is handled through:

1. **Jinja2 conditionals** — `{% if cloud_service == '...' %}` blocks within shared files (repositories, configs, package manifests)
2. **Separate entry point files** — Each cloud has its own entry point (e.g., `functions/` for Azure, `main.ts` for GCP, `lambda.ts` for AWS)
3. **Conditional file/directory names** — Cloud-specific files and directories are named with a Jinja conditional (e.g. `{% if cloud_service == 'AWS Lambda' %}lambda.ts{% endif %}`). Copier skips any path that renders to an empty string, which replaces Cookiecutter's post-generation cleanup hooks.

### Cloud → Database Mapping

| Cloud Provider | Database | TypeScript Client | Python Client | .NET Client | Go Client |
|---|---|---|---|---|---|
| Azure Function App | Cosmos DB | `@azure/cosmos` | `azure-cosmos` | `Microsoft.Azure.Cosmos` | `azcosmos` |
| GCP Cloud Function | Firestore | `@google-cloud/firestore` | `google-cloud-firestore` | `Google.Cloud.Firestore` | `cloud.google.com/go/firestore` |
| AWS Lambda | DynamoDB | `@aws-sdk/client-dynamodb` | `aioboto3` | `AWSSDK.DynamoDBv2` | `aws-sdk-go-v2/service/dynamodb` |

## Adding a New Cloud Provider

To add a new cloud provider to an existing language template:

1. **Update `copier.yml`** — Add the new option to the `cloud_service` question's `choices`
2. **Create the entry point** — Add the cloud-specific function entry point file(s) and any per-resource handlers (TypeScript `functions/` or `routes/`, Python `blueprints/`, .NET `Functions/` or `Handlers/`, Go `handlers/`), naming them with a `{% if cloud_service == '...' %}...{% endif %}` conditional so they are only generated for that cloud
3. **Add Jinja2 conditionals** to these files:
   - `package.json` / `pyproject.toml` / `.csproj` / `go.mod` — Cloud-specific dependencies
   - The per-cloud store (TypeScript `repositories/*.store.ts`, .NET `Repositories/*DocumentStore.cs`,
     Go `repositories/store.go`, Python `repositories/base_repository.py`) — Database client implementation
   - `config/container.ts` (TypeScript), blueprint wiring (Python), `DependencyInjection.cs` (.NET) or `main.go` / `function.go` (Go) — dependency wiring
   - `types/models/baseEnv.schema` — Environment variable definitions
   - `_openapi.yaml.jinja` (all four copies) — the cloud's `servers`, security scheme and route prefix
4. **Serve the spec** — Route `GET {prefix}/openapi.json` with the health check's auth, and extend the contract test so it reads the new cloud's registered routes
5. **Name any cloud-specific files/directories conditionally** so they are omitted for the other clouds
6. **Update CI pipeline** — Add the new cloud service to the `cloud-service` matrix in the workflow YAML and to the render loop in `openapi-consistency.yaml`
7. **Update `README.md`** — Change the support table cell from planned to complete

## Adding a New Language

1. Create a new top-level directory (e.g., `java/`)
2. Add `copier.yml` with `_min_copier_version: "9.18.2"`, the standard questions (`project_name`, `project_endpoint`, `project_class_name`, `cloud_service`, `resources`, etc.), the `_jinja_extensions`, `_templates_suffix: ""`, and `_subdirectory: template` settings
3. Put the template project under `template/`
4. Add input validation as a `validator:` on the prompted `project_name` and `resources` questions (Copier only runs validators for prompted questions, not for `when: false` derived values)
5. Use conditional file/directory names if supporting multiple cloud providers
6. Copy `_openapi.yaml.jinja` and `template/openapi.json` unchanged, serve the spec at `GET {prefix}/openapi.json`, and add a contract test that compares the spec with the registered routes and the request validators
7. Create `.github/workflows/build-{language}-pipeline.yaml` with the `lint-openapi` step, and add the language to `openapi-consistency.yaml`
8. Update the root `README.md` support table

## Template Variables

| Variable | Description | Example |
|---|---|---|
| `project_name` | Human-readable name | `"My API"` |
| `project_endpoint` | REST endpoint (kebab-case) | `"my-api"` |
| `project_class_name` | PascalCase class name | `"MyApi"` |
| `project_lower_camel_name` | lowerCamelCase (TS/C#) | `"myApi"` |
| `project_slug` | snake_case (Python only) | `"my_api"` |
| `cloud_service` | Target cloud platform | `"Azure Function App"` |
| `health_endpoint` | Health check URL segment; empty skips the health check | `"health"` |
| `resources` | REST resources to generate | see [Resources](#resources) |
| `author` | Project author | `"Your Name"` |
| `open_source_license` | License type | `"MIT license"` |

`project_slug`, `project_endpoint`, `project_class_name`, and `project_lower_camel_name`
are derived from `project_name` via `when: false` questions, so they are computed
automatically and never prompted. The `jinja2_strcase` extension provides `to_camel` and
`to_lower_camel` filters for case conversion, and `jinja2_time` provides the `{% now %}`
tag used in `LICENSE`.

## Testing

### CI Pipelines

Each language has a GitHub Actions workflow that:
1. Generates a project with `copier copy --defaults --trust`
2. Installs dependencies
3. Builds and lints the project, and lints `openapi.json` with the shared `.github/actions/lint-openapi` action
   (Redocly `recommended-strict`, see its `redocly.yaml`)
4. Runs unit tests, including the OpenAPI contract test

The shared composite action at `.github/actions/setup-copier-template/action.yaml` handles steps 1-2.
Its `resources-fixture` input renders `fixtures/<name>-resources.yml`. CI uses `edge`: every resource
shape the default single resource doesn't cover (each operation subset, shared and hyphenated containers,
names of differing lengths) and no health check. `multi` only feeds the published example branches.

Pipelines use a small matrix, one job per distinct risk rather than every combination:
- Ubuntu: every cloud service, with the default single resource and with `edge`
- Windows (path length, checkout) and macOS (BSD tools) once each, on different clouds
- The newest GA runtime each cloud supports: Node 24 (Node 22 on Azure Functions), Python 3.14, .NET 10, Go 1.27

Add a job or fixture only for a combination no existing job exercises; fold new resource shapes into `edge`.

`openapi-consistency.yaml` renders every language, cloud and fixture (`single`, `multi`, `edge`) in one
job, checks that the four `_openapi.yaml.jinja` copies are identical, that every language renders the
same document for a cloud and that clouds differ only in `servers` and security, and lints each distinct
document once.

### Local Verification

To verify changes locally, generate a template and test it:

```bash
# Install dependencies
pip install copier jinja2-strcase jinja2-time

# Generate a project
copier copy --defaults --trust \
  --data project_name="TestProject" \
  --data cloud_service="GCP Cloud Function" \
  --data-file .github/actions/setup-copier-template/fixtures/edge-resources.yml \
  ./typescript ./TestProject

# Build and test
cd TestProject
yarn install --no-immutable
yarn build
yarn test:unit
```

### Dependency Updates

Renovate keeps package versions current and merges its own PRs once every check passes
(see `renovate.json`). Runtime versions (Node, Python, .NET, Go) are bumped by hand once
Azure Functions, Cloud Run functions and AWS Lambda all support the new version GA.

## Code Conventions

- **TypeScript**: ES modules (`"type": "module"`), Yarn for packages, Jest for tests (ESM mode; import `jest` and friends from `@jest/globals`, mock modules with `jest.unstable_mockModule`), path aliases (`@controllers`, `@services`, etc.) via `tsconfig.json` paths, rewritten to `.js` paths by `tsc-alias`
- **Python**: Poetry for packages, pytest for tests, blueprint pattern for route registration; controllers are
  cloud-agnostic (plain values in, `utils/routing.py` resolves GCP/AWS routes), and AWS builds with SAM's makefile
  builder, which exports the Poetry lock (run `make install` before `sam build`); Azure and GCP deploy from
  `make requirements`, and `utils/deadline.py` bounds each request's database work to 8 seconds
- **.NET**: NuGet for packages, xUnit v3 for tests, solution/project structure
- **Go**: Go modules, `go test`, gofmt enforced through golangci-lint
- Template files use `{{ variable_name }}` in both filenames and content
- Comments only record a reason the code can't show (a platform or SDK quirk, a workaround, a security choice), in one short line; never restate what the code does
- Keep controllers, services, and error types cloud-agnostic — only repositories and entry points should contain cloud-specific code
