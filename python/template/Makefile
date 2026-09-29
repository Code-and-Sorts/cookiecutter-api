.PHONY: install
install: ## Install the poetry environment
	@echo "🧑‍💻 Creating virtual environment using pyenv and poetry"
	@poetry install --no-interaction

.PHONY: run
run: ## Run function app.
{%- if cloud_service == 'Azure Function App' %}
	@echo "🚀 Running Function App"
	@poetry run func start
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	@echo "🚀 Running the api Cloud Function on http://localhost:8080"
	@poetry run functions-framework --target=api --source=main.py --port=8080
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	@echo "🚀 Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api
{%- endif %}

{% if cloud_service == 'AWS Lambda' -%}
.PHONY: build
build: install ## Build the Lambda package with SAM
	@echo "🎡 Building the Lambda package"
	@sam build

# `sam build` runs this target (template.yaml sets BuildMethod: makefile) to
# package the function into $(ARTIFACTS_DIR): the Poetry main dependencies,
# exported from poetry.lock and installed as wheels for the Lambda platform, and
# every module of this project except the tests. pip runs on the project's
# Poetry environment (Python 3.14), which dependency markers are evaluated for.
LAMBDA_PYTHON = $(shell poetry env info --executable)
LAMBDA_PLATFORMS := manylinux2014_x86_64 manylinux_2_17_x86_64 manylinux_2_28_x86_64 manylinux_2_34_x86_64

.PHONY: build-{{ project_class_name }}Function
build-{{ project_class_name }}Function:
	@poetry export --help >/dev/null 2>&1 && [ -n "$(LAMBDA_PYTHON)" ] || { echo "Run 'make install' first: it creates the Poetry environment and installs the poetry-plugin-export plugin declared in pyproject.toml." >&2; exit 1; }
	poetry export --only main --format requirements.txt --output "$(ARTIFACTS_DIR)/requirements.txt"
	"$(LAMBDA_PYTHON)" -m pip install --quiet --disable-pip-version-check --no-compile \
		--requirement "$(ARTIFACTS_DIR)/requirements.txt" --target "$(ARTIFACTS_DIR)" \
		--implementation cp --python-version 3.14 --only-binary=:all: \
		$(addprefix --platform ,$(LAMBDA_PLATFORMS))
	rm "$(ARTIFACTS_DIR)/requirements.txt"
	find . -name '*.py' ! -name '*_test.py' ! -path './.*' | while read -r file; do \
		mkdir -p "$(ARTIFACTS_DIR)/$$(dirname "$$file")" && cp "$$file" "$(ARTIFACTS_DIR)/$$file"; \
	done
{%- else -%}
.PHONY: build
build: clean-build ## Build wheel file using poetry
	@echo "🎡 Creating wheel file"
	@poetry build
{%- endif %}

.PHONY: clean-build
clean-build: ## Clean build artifacts
	@echo "🧹 Cleaning build artifacts"
	@rm -rf dist

.PHONY: lint
lint: ## Lint the code with ruff
	@echo "🔍 Linting code: Running ruff"
	@poetry run ruff check .

.PHONY: format
format: ## Format the code with ruff
	@echo "🎨 Formatting code: Running ruff"
	@poetry run ruff check --fix .
	@poetry run ruff format .

.PHONY: test-unit
test-unit: ## Test the code with pytest unit tests.
	@echo "🧪 Testing code: Running pytest unit tests"
	@poetry run pytest --cov

.PHONY: audit
audit: ## Scan dependencies for known vulnerabilities
	@echo "🔍 Scanning dependencies for vulnerabilities"
	@poetry run pip install pip-audit
	@poetry run pip-audit

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
