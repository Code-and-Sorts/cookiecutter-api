.PHONY: install
install: ## Install the uv environment
	@echo "🧑‍💻 Creating virtual environment using uv"
	@uv sync

.PHONY: run
run: ## Run function app.
{%- if cloud_service == 'Azure Function App' %}
	@echo "🚀 Running Function App"
	@uv run func start
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	@echo "🚀 Running the api Cloud Function on http://localhost:8080"
	@uv run functions-framework --target=api --source=main.py --port=8080
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	@echo "🚀 Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api
{%- endif %}

{% set emulator_seed = 'uv run python -m scripts.bootstrap_emulator' -%}
{% include 'shared/_Makefile.emulator' -%}
{%- if cloud_service == 'Azure Function App' %}
	@echo "🚀 Running Function App against the Cosmos DB emulator"
	@set -a && . ./.env.emulator && set +a && uv run func start
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	@echo "🚀 Running the api Cloud Function against the Firestore emulator on http://localhost:8080"
	@set -a && . ./.env.emulator && set +a && uv run functions-framework --target=api --source=main.py --port=8080
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	@echo "🚀 Building and running Lambda API locally via SAM against DynamoDB Local"
	@sam build
	@set -a && . ./.env.emulator && set +a && sam local start-api --warm-containers LAZY --env-vars env.emulator.json --docker-network {{ project_endpoint }}-emulator
{%- endif %}

{% if cloud_service == 'AWS Lambda' -%}
.PHONY: build
build: install ## Build the Lambda package with SAM
	@echo "🎡 Building the Lambda package"
	@sam build

# SAM's builder can't read uv, so `sam build` runs this target (BuildMethod: makefile).
# The python3.14 Lambda runtime runs on Amazon Linux 2023, whose glibc is 2.34.
.PHONY: build-{{ project_class_name }}Function
build-{{ project_class_name }}Function:
	uv export --quiet --no-dev --format requirements-txt --output-file "$(ARTIFACTS_DIR)/requirements.txt"
	uv pip install --quiet --requirement "$(ARTIFACTS_DIR)/requirements.txt" --target "$(ARTIFACTS_DIR)" \
		--python-platform x86_64-manylinux_2_34 --python-version 3.14 --only-binary :all:
	rm "$(ARTIFACTS_DIR)/requirements.txt"
	find . -name '*.py' ! -name '*_test.py' ! -name conftest.py ! -path './.*' ! -path './scripts/*' | while read -r file; do \
		mkdir -p "$(ARTIFACTS_DIR)/$$(dirname "$$file")" && cp "$$file" "$(ARTIFACTS_DIR)/$$file"; \
	done
{%- else -%}
.PHONY: requirements
requirements: ## Export the main dependencies from uv.lock to requirements.txt for deployment
	@uv export --quiet --no-dev --format requirements-txt --output-file requirements.txt
	@echo "📦 Wrote requirements.txt"
{%- endif %}

.PHONY: lint
lint: ## Lint the code with ruff
	@echo "🔍 Linting code: Running ruff"
	@uv run ruff check .

.PHONY: format
format: ## Format the code with ruff
	@echo "🎨 Formatting code: Running ruff"
	@uv run ruff check --fix .
	@uv run ruff format .

.PHONY: test-unit
test-unit: ## Test the code with pytest unit tests.
	@echo "🧪 Testing code: Running pytest unit tests"
	@uv run pytest --cov

.PHONY: audit
audit: ## Scan dependencies for known vulnerabilities
	@echo "🔍 Scanning dependencies for vulnerabilities"
	@uv run --with pip-audit pip-audit

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
