# AGENTS.md

Guidelines for AI agents working on the Cookiecutter API repository.

## Project Overview

This is a [Copier](https://github.com/copier-org/copier) template repository that generates REST API projects across multiple languages and cloud platforms. Each generated project follows the **controller-service-repository** pattern with full CRUD operations.

## Repository Structure

```
cookiecutter-api/
├── python/                  # Python template (Azure + GCP)
├── typescript/              # TypeScript/Node.js template (Azure + GCP)
├── dotnet/                  # .NET/C# template (Azure only)
├── .github/
│   ├── actions/             # Shared composite actions
│   └── workflows/           # CI pipelines per language
├── .docs/                   # Documentation assets (images, SVGs)
└── README.md                # Support matrix and usage docs
```

Each language directory contains:
- `copier.yml` — Questions, derived values, validators, and Jinja extension config
- `template/` — The template project root (declared via `_subdirectory: template`); its
  contents are rendered directly into the destination directory

## Template Architecture

All templates follow this layered architecture:

```
Entry Point (functions/ or main.ts/main.py)
  → Controller (request validation, routing)
    → Service (business logic, schema validation)
      → Repository (database operations)
        → Database (Cosmos DB or Firestore)
```

### Key Patterns

- **Dependency Injection**: TypeScript uses [Inversify](https://inversify.io/), Python uses manual wiring in the blueprint/entry point
- **Schema Validation**: TypeScript uses [Zod](https://zod.dev/), Python uses [Pydantic](https://docs.pydantic.dev/), .NET uses [FluentValidation](https://docs.fluentvalidation.net/)
- **Soft Deletes**: All templates use an `isDeleted` flag rather than hard deletes
- **Base Records**: All entities extend a base schema with `id`, `isDeleted`, `createdTimestamp`, `updatedTimestamp`

## Multi-Cloud Support

Cloud-specific code is handled through:

1. **Jinja2 conditionals** — `{% if cloud_service == '...' %}` blocks within shared files (repositories, configs, package manifests)
2. **Separate entry point files** — Each cloud has its own entry point (e.g., `functions/*.ts` for Azure, `main.ts` for GCP)
3. **Conditional file/directory names** — Cloud-specific files and directories are named with a Jinja conditional (e.g. `{% if cloud_service == 'AWS Lambda' %}lambda.ts{% endif %}`). Copier skips any path that renders to an empty string, which replaces Cookiecutter's post-generation cleanup hooks.

### Cloud → Database Mapping

| Cloud Provider | Database | TypeScript Client | Python Client |
|---|---|---|---|
| Azure Function App | Cosmos DB | `@azure/cosmos` | `azure-cosmos` |
| GCP Cloud Function | Firestore | `@google-cloud/firestore` | `google-cloud-firestore` |

## Adding a New Cloud Provider

To add a new cloud provider (e.g., AWS Lambda) to an existing language template:

1. **Update `copier.yml`** — Add the new option to the `cloud_service` question's `choices`
2. **Create the entry point** — Add the cloud-specific function entry point file(s), naming them with a `{% if cloud_service == '...' %}...{% endif %}` conditional so they are only generated for that cloud
3. **Add Jinja2 conditionals** to these files:
   - `package.json` / `pyproject.toml` / `.csproj` — Cloud-specific dependencies
   - `repositories/base.repository` — Database client implementation
   - `repositories/{project}.repository` — DI binding for the database client
   - `config/inversity.config.ts` (TypeScript) or blueprint wiring (Python) — DI container setup
   - `types/models/baseEnv.schema` — Environment variable definitions
4. **Name any cloud-specific files/directories conditionally** so they are omitted for the other clouds
5. **Update CI pipeline** — Add the new cloud service to the `cloud-service` matrix in the workflow YAML
6. **Update `README.md`** — Change the support table cell from planned to complete

## Adding a New Language

1. Create a new top-level directory (e.g., `go/`)
2. Add `copier.yml` with the standard questions (`project_name`, `project_endpoint`, `project_class_name`, `cloud_service`, etc.), the `_jinja_extensions`, `_templates_suffix: ""`, and `_subdirectory: template` settings
3. Put the template project under `template/`
4. Add input validation as a `validator:` on the prompted `project_name` question (Copier only runs validators for prompted questions, not for `when: false` derived values)
5. Use conditional file/directory names if supporting multiple cloud providers
6. Create `.github/workflows/build-{language}-pipeline.yaml`
7. Update the root `README.md` support table

## Template Variables

| Variable | Description | Example |
|---|---|---|
| `project_name` | Human-readable name | `"My API"` |
| `project_endpoint` | REST endpoint (kebab-case) | `"my-api"` |
| `project_class_name` | PascalCase class name | `"MyApi"` |
| `project_lower_camel_name` | lowerCamelCase (TS/C#) | `"myApi"` |
| `project_slug` | snake_case (Python only) | `"my_api"` |
| `cloud_service` | Target cloud platform | `"Azure Function App"` |
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
3. Builds the project
4. Runs unit tests

The shared composite action at `.github/actions/setup-copier-template/action.yaml` handles steps 1-2.

Pipelines use a matrix strategy to test across:
- Multiple operating systems (ubuntu, macOS, Windows)
- Multiple runtime versions (Node 18/20, Python 3.12/3.13)
- All supported cloud services

### Local Verification

To verify changes locally, generate a template and test it:

```bash
# Install dependencies
pip install copier jinja2-strcase jinja2-time

# Generate a project
copier copy --defaults --trust \
  --data project_name="TestProject" \
  --data cloud_service="GCP Cloud Function" \
  ./typescript ./TestProject

# Build and test
cd TestProject
yarn install --no-immutable
yarn build
yarn test:unit
```

## Code Conventions

- **TypeScript**: Yarn for packages, Jest for tests, path aliases (`@controllers`, `@services`, etc.) via `tsconfig.json` paths
- **Python**: Poetry for packages, pytest for tests, blueprint pattern for route registration
- **.NET**: NuGet for packages, xUnit for tests, solution/project structure
- Template files use `{{ variable_name }}` in both filenames and content
- Keep controllers, services, and error types cloud-agnostic — only repositories and entry points should contain cloud-specific code
