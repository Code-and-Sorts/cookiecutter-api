# AGENTS.md

Guidelines for AI agents working on the Cookiecutter API repository.

## Project Overview

This is a [Cookiecutter](https://github.com/cookiecutter/cookiecutter) template repository that generates REST API projects across multiple languages and cloud platforms. Each generated project follows the **controller-service-repository** pattern with full CRUD operations.

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
- `cookiecutter.json` — Template variables and cloud service options
- `hooks/` — Pre/post generation scripts (Python)
- `{{cookiecutter.project_class_name}}/` — The template project root

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

1. **Jinja2 conditionals** — `{% if cookiecutter.cloud_service == '...' %}` blocks within shared files (repositories, configs, package manifests)
2. **Separate entry point files** — Each cloud has its own entry point (e.g., `functions/*.ts` for Azure, `main.ts` for GCP)
3. **Post-generation hooks** — `hooks/post_gen_project.py` removes files not needed for the selected cloud service

### Cloud → Database Mapping

| Cloud Provider | Database | TypeScript Client | Python Client |
|---|---|---|---|
| Azure Function App | Cosmos DB | `@azure/cosmos` | `azure-cosmos` |
| GCP Cloud Function | Firestore | `@google-cloud/firestore` | `google-cloud-firestore` |

## Adding a New Cloud Provider

To add a new cloud provider (e.g., AWS Lambda) to an existing language template:

1. **Update `cookiecutter.json`** — Add the new option to the `cloud_service` array
2. **Create the entry point** — Add the cloud-specific function entry point file(s)
3. **Add Jinja2 conditionals** to these files:
   - `package.json` / `pyproject.toml` / `.csproj` — Cloud-specific dependencies
   - `repositories/base.repository` — Database client implementation
   - `repositories/{project}.repository` — DI binding for the database client
   - `config/inversity.config.ts` (TypeScript) or blueprint wiring (Python) — DI container setup
   - `types/models/baseEnv.schema` — Environment variable definitions
4. **Update `hooks/post_gen_project.py`** — Add cleanup rules for the new cloud's files
5. **Update CI pipeline** — Add the new cloud service to the `cloud-service` matrix in the workflow YAML
6. **Update `README.md`** — Change the support table cell from planned to complete

## Adding a New Language

1. Create a new top-level directory (e.g., `go/`)
2. Add `cookiecutter.json` with the standard variables (`project_name`, `project_endpoint`, `project_class_name`, `cloud_service`, etc.)
3. Implement the controller-service-repository pattern in the target language
4. Add `hooks/pre_gen_project.py` for input validation
5. Add `hooks/post_gen_project.py` if supporting multiple cloud providers
6. Create `.github/workflows/build-{language}-pipeline.yaml`
7. Update the root `README.md` support table

## Cookiecutter Variables

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

The `jinja2_strcase` extension provides `to_camel` and `to_lower_camel` filters for automatic case conversion.

## Testing

### CI Pipelines

Each language has a GitHub Actions workflow that:
1. Sets up Cookiecutter and generates a project with `--no-input`
2. Installs dependencies
3. Builds the project
4. Runs unit tests

The shared composite action at `.github/actions/setup-cookiecutter-template/action.yaml` handles steps 1-2.

Pipelines use a matrix strategy to test across:
- Multiple operating systems (ubuntu, macOS, Windows)
- Multiple runtime versions (Node 18/20, Python 3.12/3.13)
- All supported cloud services

### Local Verification

To verify changes locally, generate a template and test it:

```bash
# Install dependencies
pip install cookiecutter jinja2-strcase

# Generate a project
cookiecutter ./typescript --no-input \
  project_name="TestProject" \
  cloud_service="GCP Cloud Function"

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
- Template files use `{{cookiecutter.variable_name}}` in both filenames and content
- Keep controllers, services, and error types cloud-agnostic — only repositories and entry points should contain cloud-specific code
