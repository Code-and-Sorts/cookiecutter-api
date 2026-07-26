# KittenClaws API

[![](https://img.shields.io/badge/made%20using%20cookiecutter%20api-grey?style=for-the-badge&logo=cookiecutter)](https://github.com/Code-and-Sorts/cookiecutter-api)

## Overview


This project is a Python-based REST API built using [Google Cloud Functions](https://cloud.google.com/functions/docs). The API leverages GCP's serverless architecture, allowing you to deploy and scale functions effortlessly in the cloud. The HTTP-triggered functions serve as the endpoints for the API, providing a seamless way to handle client requests.


The REST API exposes the following resources and operations:

- **`/kitties`** (container: `kitty_cats`)
  - `GET /kitties` — list
  - `GET /kitties/{item_id}` — get by ID
  - `POST /kitties` — create
  - `PATCH /kitties/{item_id}` — partial update
  - `DELETE /kitties/{item_id}` — soft delete


Dependency management can be handled using either [Poetry](https://python-poetry.org/) for development or requirements.txt for GCP deployment.


## Features


- GCP Cloud Functions: Utilizes Google Cloud's serverless platform to create scalable and efficient endpoints with HTTP triggers.

- Python-Based: Written entirely in Python, leveraging its rich ecosystem and libraries for rapid development.

- Poetry for Development: Manages all Python dependencies with Poetry, making the development environment consistent and easy to set up.

- Firestore Database: This project uses Google Cloud Firestore as the NoSQL database.


## Prerequisites

- Python >=3.9, <3.12


- [Google Cloud SDK (gcloud CLI)](https://cloud.google.com/sdk/docs/install): To deploy and manage GCP Cloud Functions.

- [Functions Framework](https://github.com/GoogleCloudPlatform/functions-framework-python): To run Cloud Functions locally.

- [Poetry](https://python-poetry.org/): For dependency management and virtual environment setup.

- GCP Account: An active Google Cloud Platform account with billing enabled.

- Firestore Database: Set up a Firestore database in your GCP project.


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

    Set the following environment variables for local development:
    - `GCP_PROJECT_ID`: Your GCP project ID
    - `FIRESTORE_DATABASE`: Firestore database name (defaults to "(default)")
    - `FIRESTORE_COLLECTION_KITTY_CATS`: Firestore collection for the `kitty_cats` container (defaults to "kitty_cats")

5. Run the API Locally

    Each operation deploys as its own function. Run a specific one locally using Functions Framework:

    ```console
    poetry run functions-framework --target=get_list_kitten_claws --source=main.py --port=8080
    poetry run functions-framework --target=get_by_id_kitten_claws --source=main.py --port=8080
    poetry run functions-framework --target=create_kitten_claws --source=main.py --port=8080
    poetry run functions-framework --target=update_kitten_claws --source=main.py --port=8080
    poetry run functions-framework --target=delete_kitten_claws --source=main.py --port=8080
    ```

6. Deploy to GCP

    Deploy each function to GCP Cloud Functions:

    ```console
    gcloud functions deploy get_list_kitten_claws \
      --runtime python313 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_list_kitten_claws \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_KITTY_CATS=kitty_cats
    gcloud functions deploy get_by_id_kitten_claws \
      --runtime python313 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point get_by_id_kitten_claws \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_KITTY_CATS=kitty_cats
    gcloud functions deploy create_kitten_claws \
      --runtime python313 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point create_kitten_claws \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_KITTY_CATS=kitty_cats
    gcloud functions deploy update_kitten_claws \
      --runtime python313 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point update_kitten_claws \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_KITTY_CATS=kitty_cats
    gcloud functions deploy delete_kitten_claws \
      --runtime python313 \
      --trigger-http \
      --allow-unauthenticated \
      --entry-point delete_kitten_claws \
      --source . \
      --set-env-vars GCP_PROJECT_ID=your-project-id,FIRESTORE_COLLECTION_KITTY_CATS=kitty_cats
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
├── main.py                - Cloud Functions entry point
└── requirements.txt       - Production dependencies for GCP deployment
```


## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

Repository generated with [Code-and-Sorts/cookiecutter-api](https://github.com/Code-and-Sorts/cookiecutter-api).
