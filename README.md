# Cookiecutter API

This is a modern Cookiecutter template to create REST APIs for multiple cloud platforms in multiple languages. This template currently supports the following cloud platforms and languages:

- [x] Azure Function Apps
    - [x] Python
    - [ ] Typescript
- [ ] AWS Lambdas
    - [ ] Python
    - [ ] Typescript
- [ ] Google Cloud Functions
    - [ ] Python
    - [ ] Typescript

Each project follows the controller-service-repository pattern. It supports the following features:
- Python
    - [Poetry](https://python-poetry.org/) for dependency management
    - [pytest](https://docs.pytest.org/en/stable/) and [codecov](https://about.codecov.io/) for testing
    - [mypy](https://mypy.readthedocs.io/en/stable/) for code quality
    - [pydantic](https://docs.pydantic.dev/latest/) for data validation

## Examples

[Python Example](https://github.com/Code-and-Sorts/cookiecutter-api-python-example)

## Installation

Install Cookiecutter using pip package manager:

```console
# pipx is strongly recommended.
pipx install cookiecutter

# If pipx is not an option,
# you can install Cookiecutter in your Python user directory.
python -m pip install --user cookiecutter
```

## Quickstart

To create a Cookiecutter API project, run the following:

```console
# Create using the GH CLI
cookiecutter gh:Code-and-Sorts/cookiecutter-api

# Create using the GH URL
cookiecutter https://github.com/Code-and-Sorts/cookiecutter-api.git
```

Run the following to install the environment:

```console
make install
```

To run the newly created project, run:

```console
make run
```

## Acknowledgements

Florian Maas' [cookiecutter-poetry](https://github.com/fpgmaas/cookiecutter-poetry) repository was a very helpful resource for building out this Cookiecutter template. 
