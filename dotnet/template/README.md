{%- set prefix = '' if cloud_service == 'GCP Cloud Function' else '/api' -%}
{%- set containers = resources | map(attribute='container') | unique | list -%}
# {{ project_class_name }} API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview
{%- if cloud_service == 'Azure Function App' %}

This project is a Dotnet-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

This project is a Dotnet-based REST API built using [Cloud Run functions](https://cloud.google.com/functions) and the [.NET Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-dotnet). A single HTTP function routes every request to the matching resource, allowing you to deploy and scale the API effortlessly in the cloud.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

This project is a Dotnet-based REST API built using [AWS Lambda](https://aws.amazon.com/lambda/) with [API Gateway](https://aws.amazon.com/api-gateway/). The API leverages AWS's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The Lambda functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}

The REST API exposes the following resources and operations:
{% for resource in resources %}
- **`{{ prefix }}/{{ resource.endpoint }}`** (container: `{{ resource.container }}`)
{%- if "list" in resource.operations %}
  - `GET {{ prefix }}/{{ resource.endpoint }}` — list
{%- endif %}
{%- if "get_by_id" in resource.operations %}
  - `GET {{ prefix }}/{{ resource.endpoint }}/{id}` — get by ID
{%- endif %}
{%- if "create" in resource.operations %}
  - `POST {{ prefix }}/{{ resource.endpoint }}` — create
{%- endif %}
{%- if "update" in resource.operations %}
  - `PATCH {{ prefix }}/{{ resource.endpoint }}/{id}` — partial update
{%- endif %}
{%- if "replace" in resource.operations %}
  - `PUT {{ prefix }}/{{ resource.endpoint }}/{id}` — full replace
{%- endif %}
{%- if "delete" in resource.operations %}
  - `DELETE {{ prefix }}/{{ resource.endpoint }}/{id}` — soft delete
{%- endif %}
{%- endfor %}
- **`{{ prefix }}/health`**
  - `GET {{ prefix }}/health` — health check
{%- if cloud_service == 'Azure Function App' %}

Paths are relative to the Function App host (`http://localhost:7071` when running locally); `/api` is the Azure Functions default route prefix.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

Paths are relative to the function URL (`http://localhost:8080` when running locally). Methods that are not enabled for a resource return `405 Method Not Allowed`, and unknown endpoints return `404 Not Found`.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

Paths are relative to the API Gateway stage URL (`https://<api-id>.execute-api.<region>.amazonaws.com/Prod`, or `http://localhost:3000` with `sam local start-api`).
{%- endif %}

Dependency management is handled using [Nuget](https://www.nuget.org/), ensuring a streamlined and consistent environment for managing Dotnet packages and their dependencies.

## Storage containers
{%- if cloud_service == 'Azure Function App' %}

Each resource reads and writes the Cosmos DB container configured for its `container` id. The container names are read from these settings (see `local.settings.json`), falling back to the container id when a setting is missing:

| Container | Setting | Default value | Resources |
|---|---|---|---|
{%- for container in containers %}
| `{{ container }}` | `CosmosDbContainerName_{{ container | to_camel }}` | `{{ container }}` | {{ resources | selectattr('container', 'equalto', container) | map(attribute='name') | join(', ') }} |
{%- endfor %}

The Cosmos DB connection string is read from `ConnectionStrings:CosmosDb` and the database name from `CosmosDbDatabaseName`. Containers must use `/id` as their partition key.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

Each resource reads and writes the Firestore collection configured for its `container` id. The collection names are read from these environment variables, falling back to the container id when a variable is not set:

| Container | Environment variable | Default value | Resources |
|---|---|---|---|
{%- for container in containers %}
| `{{ container }}` | `FIRESTORE_COLLECTION_{{ container | upper | replace('-', '_') }}` | `{{ container }}` | {{ resources | selectattr('container', 'equalto', container) | map(attribute='name') | join(', ') }} |
{%- endfor %}

The Google Cloud project is read from `GCP_PROJECT_ID` (required) and the Firestore database from `FIRESTORE_DATABASE` (defaults to `(default)`).
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

Each resource reads and writes the DynamoDB table configured for its `container` id. The table names are read from these environment variables, which `template.yaml` sets from the table resources it creates, falling back to the container id when a variable is not set:

| Container | Environment variable | `template.yaml` table resource | Resources |
|---|---|---|---|
{%- for container in containers %}
| `{{ container }}` | `DYNAMODB_TABLE_NAME_{{ container | upper | replace('-', '_') }}` | `{{ container | to_camel }}Table` | {{ resources | selectattr('container', 'equalto', container) | map(attribute='name') | join(', ') }} |
{%- endfor %}
{%- endif %}

Resources that use the same container share its records: there is no type discriminator, so every resource on a shared container lists, reads, updates and deletes the same items.

{%- if cloud_service == 'Azure Function App' %}
{%- set old_setting, new_setting = 'CosmosDbContainerName', 'CosmosDbContainerName_<Container>' %}
{%- elif cloud_service == 'GCP Cloud Function' %}
{%- set old_setting, new_setting = 'FIRESTORE_COLLECTION', 'FIRESTORE_COLLECTION_<CONTAINER>' %}
{%- else %}
{%- set old_setting, new_setting = 'DYNAMODB_TABLE_NAME', 'DYNAMODB_TABLE_NAME_<CONTAINER>' %}
{%- endif %}

> **Upgrading from a single-resource project:** the storage setting name now includes the container id. `{{ old_setting }}` became `{{ new_setting }}` (see the table above), so rename it in every deployed environment.

## Features
{%- if cloud_service == 'Azure Function App' %}

- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

- Cloud Run functions: Utilizes Google Cloud's serverless platform to serve the API from a single HTTP function.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- Firestore: This project uses Firestore as its NoSQL database.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

- AWS Lambda: Utilizes AWS's serverless platform to create scalable and efficient endpoints with API Gateway integration.

- Dotnet-Based: Written entirely in Dotnet, leveraging its rich ecosystem and libraries for rapid development.

- Nuget for Dependency Management: Manages all Dotnet dependencies with Nuget, making the development environment consistent and easy to set up.

- DynamoDB: This project uses Amazon DynamoDB as its NoSQL database.
{%- endif %}

## Prerequisites

- Dotnet 8.x
{%- if cloud_service == 'Azure Function App' %}

- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- Azure Account: An active Azure subscription for deploying the Function App.

- Cosmos DB NoSQL Account either deployed in Azure or [emulated](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql).
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

- [Google Cloud CLI](https://cloud.google.com/sdk/docs/install): To deploy and manage Cloud Run functions.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- Google Cloud project: An active project with Firestore enabled, or the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator).
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html): To build and run the Lambda functions locally.

- [AWS CLI](https://aws.amazon.com/cli/): To deploy and manage AWS resources.

- [Dotnet](https://dotnet.microsoft.com/en-us/download): Dotnet SDK and CLI

- AWS Account: An active AWS account for deploying Lambda functions.

- DynamoDB table (created automatically via the SAM template or available via [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html)).
{%- endif %}

## Setup and Installation
{%- if cloud_service == 'Azure Function App' %}

1. Install Azure Functions Core Tools

    Follow the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-python#install-the-azure-functions-core-tools) to install Azure Function Core Tools based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

    To be able to run the project locally, set the environment variable values in the local.settings.json project file.

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the local development server using the Azure Function Core Tools, where you can interact with your API endpoints.
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

1. Install the Google Cloud CLI

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud CLI based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

    To be able to run the project locally, set `GCP_PROJECT_ID` (and the optional variables listed under [Storage containers](#storage-containers)) in your environment.

4. Run the API Locally

    ```console
    make run
    ```

    This command starts the function locally with the .NET Functions Framework, where you can interact with your API endpoints.

5. Deploy

    ```console
    gcloud functions deploy {{ project_endpoint }} --gen2 --runtime=dotnet8 --trigger-http --entry-point={{ project_class_name }}.Api.Function --source={{ project_class_name }}.Api --set-env-vars=GCP_PROJECT_ID=<project-id>
    ```
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

1. Install AWS SAM CLI

    Follow the [documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) to install AWS SAM CLI based on your operating system.

2. Install Dotnet SDK

    If you haven't already installed Dotnet SDK, you can do so by following the [official installation guide](https://dotnet.microsoft.com/en-us/download).

3. Install Dependencies

    Install all dependencies:

    ```console
    make install
    ```

4. Run the API Locally

    ```console
    make run
    ```

    This command builds the project and starts a local API Gateway using SAM CLI, where you can interact with your API endpoints.
{%- endif %}

{% if cloud_service == 'GCP Cloud Function' %}6{% else %}5{% endif %}. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Development Workflow

### Adding a New Dependency

```bash
dotnet add package <package-name>
```

### Removing a Dependency

```bash
dotnet remove package <package-name>
```

## Running Tests

Ensure your code is working as expected by running unit tests using dotnet test:

```bash
make test-unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities:

```bash
make audit
```

This uses `dotnet list package --vulnerable --include-transitive` to check for packages with known CVEs. It is also run automatically in CI on every PR and push to main.

## Repository structure

```text
├── {{ project_class_name }}.Api
│   ├── Controllers
{%- if cloud_service != 'GCP Cloud Function' %}
│   ├── Functions
{%- endif %}
│   ├── Interfaces
│   ├── Models
│   │   ├── Dtos
│   │   ├── Entities
│   │   └── Schemas
{%- if cloud_service == 'Azure Function App' %}
│   ├── Properties
{%- endif %}
│   ├── Repositories
│   ├── Services
│   └── Utils
└── {{ project_class_name }}.Api.Tests.Unit
    ├── Controllers
    ├── Functions
    ├── Repositories
    ├── Services
    ├── Utils
    └── tests
```

## License

This project is licensed under the {% if open_source_license == 'MIT license' -%}MIT License{% elif open_source_license == 'BSD license' %}
BSD License{% elif open_source_license == 'ISC license' -%}ISC License{% elif open_source_license == 'Apache Software License 2.0' -%}Apache Software License 2.0{% elif open_source_license == 'GNU General Public License v3' -%}GNU General Public License v3
{% endif %}. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
