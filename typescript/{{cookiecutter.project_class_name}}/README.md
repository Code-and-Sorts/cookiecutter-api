# {{ cookiecutter.project_name }} API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
This project is a Typescript NodeJS-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
This project is a Typescript NodeJS-based REST API built using [Google Cloud Functions](https://cloud.google.com/functions). The API leverages GCP's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.
{%- endif %}

The REST API has the following endpoints:
- GET (by ID)
- GET (list)
- POST
- PATCH
- DELETE (soft-delete)

Dependency management is handled using [Yarn](https://yarnpkg.com/), ensuring a streamlined and consistent environment for managing node packages and their dependencies.

## Features

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Typescript NodeJS-Based: Written entirely in Typescript NodeJS, leveraging its rich ecosystem and libraries for rapid development.

- Yarn for Dependency Management: Manages all node dependencies with Yarn, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
- GCP Cloud Functions: Utilizes Google Cloud Platform's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Typescript NodeJS-Based: Written entirely in Typescript NodeJS, leveraging its rich ecosystem and libraries for rapid development.

- Yarn for Dependency Management: Manages all node dependencies with Yarn, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore for data storage.
{%- endif %}

## Prerequisites

- NodeJS >=18.x, <=20.x

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Yarn](https://yarnpkg.com/): For dependency management.

- Azure Account: An active Azure subscription for deploying the Function App.

- Cosmos DB NoSQL Account either deployed in Azure or [emulated](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql).
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage GCP Cloud Functions.

- [Yarn](https://yarnpkg.com/): For dependency management.

- Google Cloud Account: An active GCP account for deploying Cloud Functions.

- Firestore Database: A Firestore database instance in your GCP project or [emulator](https://cloud.google.com/firestore/docs/emulator).
{%- endif %}

## Setup and Installation

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
1. Install Azure Functions Core Tools

    Follow the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=linux%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-typescript#install-the-azure-functions-core-tools) to install Azure Function Core Tools based on your operating system.

2. Install Yarn

    If you haven't already installed Yarn, you can do so by following the [official installation guide](https://yarnpkg.com/getting-started/install).

3. Install Dependencies

    Install all dependencies:

    ```console
    yarn
    ```

    To be able to run the project locally, set the environment variable values in the .env project file.

4. Run the API Locally

    ```console
    yarn build
    yarn start
    ```

    This command starts the local development server using the Azure Function Core Tools, where you can interact with your API endpoints.
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
1. Install Google Cloud SDK

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud SDK (gcloud CLI) based on your operating system.

2. Install Yarn

    If you haven't already installed Yarn, you can do so by following the [official installation guide](https://yarnpkg.com/getting-started/install).

3. Install Dependencies

    Install all dependencies:

    ```console
    yarn
    ```

    To be able to run the project locally, set the environment variable values in the .env or local.settings.json project file:
    - `GCP_PROJECT_ID`: Your GCP project ID
    - `FIRESTORE_DATABASE_ID`: Your Firestore database ID (default is "(default)")

4. Run the API Locally

    ```console
    yarn build
    yarn start
    ```

    This command starts the local development server using the Functions Framework, where you can interact with your API endpoints.

5. Deploy to GCP

    To deploy your Cloud Functions:

    ```console
    gcloud functions deploy create{{cookiecutter.project_class_name}} \
      --runtime nodejs20 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point create{{cookiecutter.project_class_name}} \
      --source ./dist
    ```
{%- endif %}

5. Thunderclient

    Included in the project is a [Thunderclient](https://www.thunderclient.com/) collection in the .thunderclient directory to easily test the locally hosted APIs.

## Development Workflow

### Adding a New Dependency

```bash
yarn add <package-name>
```

### Removing a Dependency

```bash
yarn remove <package-name>
```

## Running Tests

Ensure your code is working as expected by running unit tests using pytest:

```bash
yarn test:unit
```

## Repository structure

```text
└── cookiecutter-template-typescript
    ├── .thunderclient     - Thunderclient collection
    ├── config             - Depency injection config
    ├── controller         - Controllers
    ├── functions          - {% if cookiecutter.cloud_service == 'Azure Function App' %}Function App methods{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}Cloud Function methods{% endif %}
    ├── repository         - {% if cookiecutter.cloud_service == 'Azure Function App' %}Cosmos DB repository{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}Firestore repository{% endif %}
    ├── service            - Services
    ├── types              - Zod models and errors
    └── utils              - Error detect & response generator utilities
```

## License

This project is licensed under the {% if cookiecutter.open_source_license == 'MIT license' -%}MIT License{% elif cookiecutter.open_source_license == 'BSD license' %}
BSD License{% elif cookiecutter.open_source_license == 'ISC license' -%}ISC License{% elif cookiecutter.open_source_license == 'Apache Software License 2.0' -%}Apache Software License 2.0{% elif cookiecutter.open_source_license == 'GNU General Public License v3' -%}GNU General Public License v3
{% endif %}. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
