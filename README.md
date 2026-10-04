<img src="./.docs/imgs/cookiecutter_api_header.jpg">

<div align="center">
  <h1>
    <img src="./.docs/imgs/stars.gif" width="32"> Cookiecutter API <img src="./.docs/imgs/stars.gif" width="32">
  </h1>
</div>

![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-python-pipeline.yaml?branch=main&label=Python-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-typescript-pipeline.yaml?branch=main&label=Typescript-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-dotnet-pipeline.yaml?branch=main&label=Dotnet-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-go-pipeline.yaml?branch=main&label=Go-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/integration-tests.yaml?branch=main&label=Integration-Tests&style=for-the-badge)

[![](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](./LICENSE)

[![](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/copier-org/copier/master/img/badge/badge-grayscale-inverted-border-purple.json&style=for-the-badge)](https://github.com/copier-org/copier)


A [Copier](https://github.com/copier-org/copier) template for generating REST APIs across multiple cloud platforms and languages.

> [!WARNING]
> This project is still in development. Things may change or break between versions, so
> pin a template version if you depend on it.

## Usage

Install Copier 9.18.2 or newer (and the Jinja extensions the templates use) with pip or pipx:

```console
# pipx is strongly recommended.
pipx install copier
pipx inject copier jinja2-strcase jinja2-time

# If pipx is not an option, install into your Python user directory.
python -m pip install --user copier jinja2-strcase jinja2-time
```

The repository is one Copier template. Its first question is `language` (`python`,
`typescript`, `dotnet` or `go`); answer it at the prompt or pass it with `--data`:

```console
copier copy --trust gh:Code-and-Sorts/cookiecutter-api ./my-api

# Or from a clone, choosing the language up front
git clone https://github.com/Code-and-Sorts/cookiecutter-api.git
copier copy --trust --data language=python ./cookiecutter-api ./my-api
```

`--trust` is needed because the template uses Jinja extensions. Follow the prompts to
configure your project. The generated project includes a `.copier-answers.yml` file that
records the template and the commit it came from, so you can pull in future template
changes with:

```console
cd my-api
copier update --trust
```

### Multiple resources

By default a generated project exposes one REST resource named after the project. To
expose several, answer the `resources` prompt or pass them in a YAML file:

```yaml
resources:
  - name: "Cat"
    endpoint: "cats"
    container: "animals"
    operations: ["list", "get_by_id", "create", "update", "delete"]
  - name: "Dog"
    endpoint: "dogs"
    container: "animals"
    operations: ["list", "get_by_id", "create", "replace", "delete"]
```

```console
copier copy --trust --data language=typescript --data-file resources.yml gh:Code-and-Sorts/cookiecutter-api ./my-api
```

`name` is the PascalCase type name, `endpoint` the URL segment, and `container` the storage
container, collection or table. Resources that share a `container` share their records.
`operations` is any subset of `list`, `get_by_id`, `create`, `update` (PATCH), `replace`
(PUT) and `delete`; only those routes are generated.

Every project also gets a `GET` health check named `health`, served under the same route
prefix as its resources (for example `/api/health` on Azure Functions). Answer
`health_endpoint` (or pass `--data health_endpoint=status`) to use another name, or leave it
empty to skip the health check.

Each resource gets its own files in every layer, for example `CatController.cs` and
`DogController.cs` in .NET, or `controllers/cat.controller.ts` and
`controllers/dog.controller.ts` in TypeScript.

### Run locally against an emulator

Every generated project can run without a cloud account. Its `docker-compose.yml` starts
only the database emulator for the chosen cloud, `.env.emulator` holds the public emulator
settings, and a bootstrap command creates one container or table per `container`:

| Cloud | Emulator | Host port |
| --- | --- | --- |
| Azure | [Cosmos DB vNext emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) (plain HTTP, x64 and arm64) | 8081, Data Explorer on 1234 |
| AWS | [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) | 8000 |
| GCP | [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) | 8085 |

```console
make emulator-up && make emulator-seed && make run-emulator   # Python, .NET and Go
yarn emulator:up && yarn emulator:seed && yarn start:emulator  # TypeScript
```

The SDK clients switch to the emulator only when its settings are present (a
Cosmos DB emulator flag, `AWS_ENDPOINT_URL_DYNAMODB` or `FIRESTORE_EMULATOR_HOST`), so
deployed code paths are unchanged. AWS projects run through `sam local start-api` on the
emulator's Docker network with `env.emulator.json`. Each generated README covers ports,
credentials, limitations and troubleshooting. The compose file, `.env.emulator`,
`env.emulator.json`, the shared Make targets and that README section are kept once in
`shared/` for every language.

### Integration tests

`tests/integration` is one black-box pytest suite that checks the [API contract](./AGENTS.md#api-contract)
over HTTP for every language and cloud: each enabled operation, validation, soft deletes, shared
containers, `?limit=`, the health check and the stored record format (read straight from the
emulator). It reads the resources from the project's `.copier-answers.yml`, so every fixture is
covered without changes. The `Integration Tests` workflow runs it for every language and cloud with
the `single` and `edge` resources fixtures on pushes to `main`; run it on a branch with **Run workflow**,
choosing a language, cloud and fixture or `all`.

To run it locally, render a project, start it with its emulator commands from
[Run locally against an emulator](#run-locally-against-an-emulator), and point the suite at it with
[uv](https://docs.astral.sh/uv/):

```console
copier copy --defaults --trust --vcs-ref HEAD --data language=go --data cloud_service="GCP Cloud Function" \
  --data-file .github/actions/setup-copier-template/fixtures/edge-resources.yml . ../KittenClaws

uv sync --project tests/integration
uv run --project tests/integration pytest tests/integration --project-dir ../KittenClaws --base-url http://localhost:8080
```

The base URL includes the route prefix: `http://localhost:7071/api` on Azure, `http://localhost:8080` on
GCP and `http://localhost:3000` on AWS. The suite waits up to `--ready-timeout` seconds (180) for the API
to answer, writes to the emulator and leaves its records there; `make emulator-down` discards them.

## Supported Templates

Pick a row with the `language` answer (`python`, `typescript`, `dotnet` or `go`) and a
column with `cloud_service`. Each ✅ links to that combination's generated example.

<table width="100%">
  <tr>
    <th width="10%" rowspan="2"></th>
    <td width="30%" align="center"><img src="./.docs/imgs/azure.svg" height="18"> Azure</td>
    <td width="30%" align="center"><img src="./.docs/imgs/aws.svg" height="18"> AWS</td>
    <td width="30%" align="center"><img src="./.docs/imgs/google-cloud.svg" height="18"> GCP</td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/function-app.svg" height="18" title="Function App"> Function App</td>
    <td align="center"><img src="./.docs/imgs/lambda.svg" height="18" title="Lambda"> Lambda</td>
    <td align="center"><img src="./.docs/imgs/cloud-function.svg" height="18" title="Cloud Functions"> Cloud Function</td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/python.svg" height="18" title="Python"></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/azure-python" title="Complete: browse the azure-python example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/aws-python" title="Complete: browse the aws-python example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/gcp-python" title="Complete: browse the gcp-python example">✅</a></td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/typescript.svg" height="18" title="NodeJS"></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/azure-typescript" title="Complete: browse the azure-typescript example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/aws-typescript" title="Complete: browse the aws-typescript example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/gcp-typescript" title="Complete: browse the gcp-typescript example">✅</a></td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/dotnet.svg" height="18" title="dotnet"></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/azure-dotnet" title="Complete: browse the azure-dotnet example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/aws-dotnet" title="Complete: browse the aws-dotnet example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/gcp-dotnet" title="Complete: browse the gcp-dotnet example">✅</a></td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/golang.svg" height="18" title="Golang"></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/azure-go" title="Complete: browse the azure-go example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/aws-go" title="Complete: browse the aws-go example">✅</a></td>
    <td align="center"><a href="https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/gcp-go" title="Complete: browse the gcp-go example">✅</a></td>
  </tr>
</table>

---
> [!NOTE]
> Each project follows the controller-service-repository pattern.

## Examples

Browse generated projects for every language and cloud in
[cookiecutter-api-examples](https://github.com/Code-and-Sorts/cookiecutter-api-examples).
Each combination lives on its own `<cloud>-<language>` branch (for example
[`aws-typescript`](https://github.com/Code-and-Sorts/cookiecutter-api-examples/tree/aws-typescript)),
rendered with two resources (`Cat` and `Dog`) and republished on every push to `main`
by the `Publish Example Branches` workflow.

## Resources

Below are the SDKs and frameworks used in the various templates.

### Python
- [uv](https://docs.astral.sh/uv/) for dependency management
- [pytest](https://docs.pytest.org/en/stable/) for testing
- [pydantic](https://docs.pydantic.dev/latest/) for schema validation

### Typescript NodeJS
- [Yarn](https://yarnpkg.com/) for dependency management
- [Jest](https://jestjs.io/) for testing
- [Zod](https://zod.dev/) for schema validation

## Dotnet
- [Nuget](https://www.nuget.org/) for dependency management
- [xUnit](https://xunit.net/) for testing
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/) for schema validation

### Go
- [Go Modules](https://go.dev/ref/mod) for dependency management
- [testing](https://pkg.go.dev/testing) and [testify](https://github.com/stretchr/testify) for testing
- [jsonschema](https://github.com/santhosh-tekuri/jsonschema) for schema validation
- [Azure SDK for Go (azcosmos)](https://github.com/Azure/azure-sdk-for-go/tree/main/sdk/data/azcosmos) for Cosmos DB

### Azure
- [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) for hosting the APIs
- [Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/) for data storage
- [Cosmos DB Linux emulator (vNext)](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) for local development

### AWS
- [Lambda docs](https://docs.aws.amazon.com/lambda/) for hosting the APIs
- [DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/GettingStartedDynamoDB.html) for data storage
- [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) for local development

### Google Cloud
- [Cloud Functions](https://cloud.google.com/functions/docs) for hosting the APIs
- [Firestore](https://cloud.google.com/firestore#documentation) for data storage
- [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) for local development
