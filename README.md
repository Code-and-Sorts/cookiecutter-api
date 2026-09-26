# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview


This project is a Python-based REST API built using [Google Cloud Functions](https://cloud.google.com/functions/docs). The API leverages GCP's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.

The REST API exposes the following resources and operations:

Each operation is deployed as its own Cloud Run function, so every path starts with the function name (for example `https://REGION-PROJECT_ID.cloudfunctions.net/<function>`, or `http://localhost:8080/<function>` locally).

- **Cat** (container: `animals`)
  - `GET /get_list_cat` — list
  - `GET /get_by_id_cat/{item_id}` — get by ID
  - `POST /create_cat` — create
  - `PATCH /update_cat/{item_id}` — partial update
  - `DELETE /delete_cat/{item_id}` — soft delete
- **Dog** (container: `animals`)
  - `GET /get_list_dog` — list
  - `GET /get_by_id_dog/{item_id}` — get by ID
  - `POST /create_dog` — create
  - `PUT /replace_dog/{item_id}` — full replace
  - `DELETE /delete_dog/{item_id}` — soft delete

A health check is available at `GET /health`.

> **Shared container:** Cat and Dog read and write the `animals` container. There is no type discriminator, so they share the same records: an item created through one resource is visible, and can be changed or deleted, through the others.


Dependency management is handled using [Poetry](https://python-poetry.org/), ensuring a streamlined and consistent environment for managing Python packages and their dependencies.


## Features


- GCP Cloud Functions: Utilizes Google Cloud's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- Poetry for Development: Manages all Python dependencies with Poetry, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.


## Prerequisites

- Python 3.14


- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage GCP Cloud Functions.

- [Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-python): To run Cloud Functions locally.

- [Poetry](https://python-poetry.org/): For dependency management and virtual environment setup.

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project.


## Configuration

Settings are read from environment variables (case-insensitive).

| Variable | Description | Default |
| --- | --- | --- |
| `GCP_PROJECT_ID` | GCP project ID | required |
| `FIRESTORE_DATABASE` | Firestore database name | `(default)` |
| `FIRESTORE_COLLECTION_ANIMALS` | Firestore collection for `animals` | `animals` |

> **Note:** earlier versions of this template used a single `FIRESTORE_COLLECTION` variable. It has been replaced by one variable per container, listed above.

## Setup and Installation


1. Install Google Cloud SDK

    Follow the [documentation](https://cloud.google.com/sdk/docs/install) to install the Google Cloud SDK based on your operating system.

2. Install Poetry

    If you haven't already installed Poetry, you can do so by following the [official installation guide](https://python-poetry.org/docs/).

3. Install Dependencies

    Install all dependencies and set up the virtual environment:

    ```console
    make install
    ```

4. Set Environment Variables

    Set the environment variables listed under [Configuration](#configuration) for local development.

5. Run the API Locally

    Each operation deploys as its own function. Run a specific one locally using Functions Framework:

    ```console
    poetry run functions-framework --target=get_list_cat --source=main.py --port=8080
    poetry run functions-framework --target=get_by_id_cat --source=main.py --port=8080
    poetry run functions-framework --target=create_cat --source=main.py --port=8080
    poetry run functions-framework --target=update_cat --source=main.py --port=8080
    poetry run functions-framework --target=delete_cat --source=main.py --port=8080
    poetry run functions-framework --target=get_list_dog --source=main.py --port=8080
    poetry run functions-framework --target=get_by_id_dog --source=main.py --port=8080
    poetry run functions-framework --target=create_dog --source=main.py --port=8080
    poetry run functions-framework --target=replace_dog --source=main.py --port=8080
    poetry run functions-framework --target=delete_dog --source=main.py --port=8080
    ```

6. Deploy to GCP

    Cloud Run functions install dependencies from a `requirements.txt`. Export one from Poetry first (this needs the [poetry-plugin-export](https://github.com/python-poetry/poetry-plugin-export) plugin):

    ```console
    poetry export --without dev --output requirements.txt
    ```

    Then deploy each function:

    ```console
    gcloud functions deploy get_list_cat \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_list_cat \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy get_by_id_cat \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_by_id_cat \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy create_cat \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point create_cat \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy update_cat \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point update_cat \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy delete_cat \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point delete_cat \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy get_list_dog \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_list_dog \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy get_by_id_dog \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_by_id_dog \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy create_dog \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point create_dog \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy replace_dog \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point replace_dog \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    gcloud functions deploy delete_dog \
      --runtime python314 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point delete_dog \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_ANIMALS=animals
    ```


## Development Workflow

### Adding a New Dependency

```bash
poetry add <package-name>
```

### Removing a Dependency

```bash
poetry remove <package-name>
```

## Running Tests

Ensure your code is working as expected by running unit tests using pytest:

```bash
make test-unit
```

## Vulnerability Scanning

Scan project dependencies for known security vulnerabilities using [pip-audit](https://pypi.org/project/pip-audit/):

```bash
make audit
```

This is also run automatically in CI on every PR and push to main.

## Repository structure


```text
├── cookiecutter-template-python
│   ├── blueprints         - Cloud Function methods
│   ├── controllers        - Controllers
│   ├── errors             - Custom errors
│   ├── models             - Pydantic models
│   ├── repositories       - Firestore repository
│   ├── services           - Services
│   └── utils              - Error detect & response generator utilities
│
└── main.py                - Cloud Functions entry point
```


## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
