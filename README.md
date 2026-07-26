# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview

This project is a Typescript NodeJS-based REST API built using [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/). The API leverages Azure's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

- **`/kitties`** (container: `kitties`)
  - `GET /kitties` — list
  - `GET /kitties/{id}` — get by ID
  - `POST /kitties` — create
  - `PATCH /kitties/{id}` — partial update
  - `DELETE /kitties/{id}` — soft delete

Dependency management is handled using [Yarn](https://yarnpkg.com/), ensuring a streamlined and consistent environment for managing node packages and their dependencies.

## Features

- Azure Function Apps: Utilizes Azure's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Typescript NodeJS-Based: Written entirely in Typescript NodeJS, leveraging its rich ecosystem and libraries for rapid development.

- Yarn for Dependency Management: Manages all node dependencies with Yarn, making the development environment consistent and easy to set up.

- Cosmos DB NoSQL Account: This project uses Cosmos DB NoSQL database.

## Prerequisites

- NodeJS >=18.x, <=20.x

- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): To run the Function Apps locally.

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/): To deploy and manage Azure Function Apps.

- [Yarn](https://yarnpkg.com/): For dependency management.

- Azure Account: An active Azure subscription for deploying the Function App.

- Cosmos DB NoSQL Account either deployed in Azure or [emulated](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql).

## Setup and Installation

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

Ensure your code is working as expected by running unit tests using jest:

```bash
yarn test:unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities using [yarn npm audit](https://yarnpkg.com/cli/npm/audit):

```bash
yarn audit
```

This is also run automatically in CI on every PR and push to main.

## Repository structure

```text
└── cookiecutter-template-typescript
    ├── .thunderclient     - Thunderclient collection
    ├── config             - Depency injection config
    ├── controller         - Controllers
    ├── functions          - Function App methods
    ├── repository         - Cosmos DB repository
    ├── service            - Services
    ├── types              - Zod models and errors
    └── utils              - Error detect & response generator utilities
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
