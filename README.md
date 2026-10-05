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

## Supported Templates

Pick a row with the `language` answer (`python`, `typescript`, `dotnet` or `go`) and a
column with `cloud_service`. Each ✅ links to that combination's generated example in
[cookiecutter-api-examples](https://github.com/Code-and-Sorts/cookiecutter-api-examples),
rendered with two resources (`Cat` and `Dog`) and republished on every push to `main`.

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

## Usage

Install [Copier](https://copier.readthedocs.io/) 9.18.2+ with the template's Jinja extensions, then generate a project and answer the prompts:

```console
pipx install copier
pipx inject copier jinja2-strcase jinja2-time

copier copy --trust gh:Code-and-Sorts/cookiecutter-api ./my-api
```

Run `copier update --trust` inside the project later to pull in template changes.

### Multiple resources

A project exposes one REST resource named after it by default. To add more, answer the
`resources` prompt or pass a YAML file with `--data-file resources.yml`:

```yaml
resources:
  - name: "Cat"
    endpoint: "cats"
    container: "animals"
    operations: ["list", "get_by_id", "create", "update", "delete"]
```

`operations` can be any of `list`, `get_by_id`, `create`, `update` (PATCH), `replace` (PUT)
and `delete`. Resources that share a `container` share their records.

### Run locally

Every project runs against a local database emulator in Docker, no cloud account needed:

```console
make emulator-up emulator-seed run-emulator                     # Python, .NET, Go
yarn emulator:up && yarn emulator:seed && yarn start:emulator   # TypeScript
```

The generated README covers ports, settings and troubleshooting.

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

### Dotnet
- [Nuget](https://www.nuget.org/) for dependency management
- [xUnit](https://xunit.net/) for testing
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/) for schema validation

### Go
- [Go Modules](https://go.dev/ref/mod) for dependency management
- [testing](https://pkg.go.dev/testing) and [testify](https://github.com/stretchr/testify) for testing
- [jsonschema](https://github.com/santhosh-tekuri/jsonschema) for schema validation

### Azure
- [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) for hosting the APIs
- [Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/) for data storage
- [Cosmos DB Linux emulator (vNext)](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) for local development
- Cosmos DB SDKs:
  - [azure-cosmos](https://pypi.org/project/azure-cosmos/) (Python)
  - [@azure/cosmos](https://www.npmjs.com/package/@azure/cosmos) (TypeScript)
  - [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos) (.NET)
  - [azcosmos](https://pkg.go.dev/github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos) (Go)

### AWS
- [Lambda docs](https://docs.aws.amazon.com/lambda/) for hosting the APIs
- [DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/GettingStartedDynamoDB.html) for data storage
- [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) for local development
- DynamoDB SDKs:
  - [aioboto3](https://pypi.org/project/aioboto3/) (Python)
  - [@aws-sdk/client-dynamodb](https://www.npmjs.com/package/@aws-sdk/client-dynamodb) (TypeScript)
  - [AWSSDK.DynamoDBv2](https://www.nuget.org/packages/AWSSDK.DynamoDBv2) (.NET)
  - [aws-sdk-go-v2/service/dynamodb](https://pkg.go.dev/github.com/aws/aws-sdk-go-v2/service/dynamodb) (Go)

### Google Cloud
- [Cloud Functions](https://cloud.google.com/functions/docs) for hosting the APIs
- [Firestore](https://cloud.google.com/firestore#documentation) for data storage
- [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) for local development
- Firestore SDKs:
  - [google-cloud-firestore](https://pypi.org/project/google-cloud-firestore/) (Python)
  - [@google-cloud/firestore](https://www.npmjs.com/package/@google-cloud/firestore) (TypeScript)
  - [Google.Cloud.Firestore](https://www.nuget.org/packages/Google.Cloud.Firestore) (.NET)
  - [cloud.google.com/go/firestore](https://pkg.go.dev/cloud.google.com/go/firestore) (Go)
