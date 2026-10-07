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

### Supported infrastructure

`include_infrastructure` works with every language. Each cloud deploys the same shape: a public API gateway with
API keys, private compute, a private database and the network between them. A stack can pick any tier below;
the defaults are what a stack gets unless it overrides them.

#### <img src="./.docs/imgs/azure.svg" height="18"> Azure ✅

##### Compute

Each stack runs the API on one hosting, `flex_consumption` unless it picks another, and one of that hosting's SKUs.

| Hosting | Service | SKUs | Default SKU |
| --- | --- | --- | --- |
| `flex_consumption` | Azure Functions, Flex Consumption | `FC1` | `FC1` |
| `app_service` | Azure Functions, App Service plan | Linux `B1`-`B3`, `S1`-`S3`, `P0v3`-`P3v3`, `P1mv3`-`P5mv3`, `P0v4`-`P5v4`, `P1mv4`-`P5mv4` | `B1` |
| `premium` | Azure Functions, Elastic Premium | `EP1`-`EP3` | `EP1` |
| `container_app` | Azure Container Apps | Workload profiles `Consumption`, `D4`-`D32`, `E4`-`E32`, `NC24-A100`-`NC96-A100` | `Consumption` |

##### Database

Cosmos DB for NoSQL, with one capacity mode per stack, `serverless` unless it picks another.

| Capacity | Throughput |
| --- | --- |
| `serverless` | Billed per request |
| `provisioned` | From 400 RU/s, in steps of 100 |
| `autoscale` | Maximum from 1000 RU/s, in steps of 1000 |

##### API gateway

API Management, `Developer` unless the stack picks another tier.

| Tier | Units |
| --- | --- |
| `Developer` | 1 (no SLA) |
| `StandardV2` | 1-10 |
| `Premium` | 1-31 |

API Management's Consumption, Basic, Standard and Basic v2 tiers are not offered: they cannot reach a backend
that only has a private endpoint.

##### Network

| Resource | Details | Default |
| --- | --- | --- |
| Virtual network | Any address space of `/22` or larger, split into app, gateway and private endpoint subnets | `10.20.0.0/16` |
| Private endpoints | Cosmos DB, and the API on Azure Functions (Container Apps run in an internal environment) | |
| Private DNS | A `privatelink` zone per private endpoint, or the Container Apps environment's zone, linked to the virtual network | |

##### Identity

| Identity | Used by | For |
| --- | --- | --- |
| Entra ID application | The API | App Service authentication admits only tokens issued for it |
| User-assigned managed identity | The API | Cosmos DB data, host storage (Premium keeps the account key for its content share) and the container registry |
| User-assigned managed identity | API Management | The Entra ID token it sends to the API |

##### Monitoring

| Resource | Collects |
| --- | --- |
| Log Analytics workspace | Every log below, in one place |
| Application Insights | Requests, dependencies and traces from the API and API Management |
| Diagnostic settings | Platform logs of the function app, API Management and Cosmos DB |

#### <img src="./.docs/imgs/aws.svg" height="18"> AWS 🚧

Planned: Lambda, DynamoDB and API Gateway with the same stacks.

#### <img src="./.docs/imgs/google-cloud.svg" height="18"> GCP 🚧

Planned: Cloud Run functions, Firestore and API Gateway with the same stacks.

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

### Infrastructure (Atmos and Terraform)

Answer `include_infrastructure` (Azure for now; GCP and AWS follow with the same stacks) to get an
`infra/` folder with [Atmos](https://atmos.tools/) stacks and a Terraform component, and a
`.github/workflows/deploy.yml` that plans on pull requests and deploys every stack in order on `main`
(OIDC only, no stored cloud keys). On Azure each stack deploys:

- **API Management**, the only public entry point. Resource routes need an API key (`x-api-key`); the
  health check is open. It calls the API with an Entra ID token for its managed identity.
- **The API**, private: Azure Functions behind a private endpoint, or an internal Container Apps
  environment. App Service authentication admits only API Management's identity, so function keys are
  not used.
- **Cosmos DB**, private and keyless: only the API's managed identity reaches it, through a private endpoint.
- A storage account for the Functions host (virtual network only), a virtual network, and Log Analytics with
  Application Insights, which also receives the platform logs of the API, API Management and Cosmos DB.
- Each part in its own resource group (`network`, `monitoring`, `data`, `app`, `gateway`), with every name from
  the [Azure Verified Modules naming utility](https://github.com/Azure/terraform-azure-avm-utl-naming).

The defaults, used by every stack unless it overrides them, are asked once. Their choices and defaults
depend on the cloud (see [Supported infrastructure](#supported-infrastructure)) and are data in
`infra_clouds` in `copier.yml`, so GCP and AWS add their own without new questions:

| Question | Sets |
| --- | --- |
| `infra_region` | Region |
| `infra_compute_hosting`, `infra_compute_sku` | Compute hosting and its plan SKU (the SKU follows the hosting unless set) |
| `infra_database_capacity`, `infra_database_throughput` | Database capacity mode and, where the mode has one, its throughput |
| `infra_gateway_sku`, `infra_gateway_capacity` | API gateway tier and units |
| `infra_admin_email` | Contact email (on Azure, the API Management publisher email); default `admin@example.com` |

`infra_environments` lists the stacks, one file each in `infra/stacks/deploy/`. Each item has a `name`
and may override any default for that stack alone, so stacks can be identical or differ:

```yaml
include_infrastructure: true
infra_environments:
  - name: dev
  - name: qa
  - name: stg
    compute_hosting: premium
  - name: prod
    compute_hosting: premium
    compute_sku: EP2
    database_capacity: autoscale
    database_throughput: 4000
    gateway_sku: Premium
```

The component's Terraform tests plan against mocked providers (`terraform test`), so they run without an
Azure account. The generated `infra/README.md` covers the one-time setup (state storage, the deployment
identity and GitHub environments), running Atmos yourself and calling the API. This repository's CI renders,
lints and tests the infrastructure for every language but never deploys it.

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
