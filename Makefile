.PHONY: install
install: ## Install the poetry environment
	@echo "🧑‍💻 Creating virtual environment using pyenv and poetry"
	@poetry install --no-interaction

.PHONY: run
run: ## Run function app.
	@echo "🚀 Running Function App"
	@poetry run func start

COMPOSE ?= docker compose

.PHONY: emulator-up
emulator-up: ## Start the local Cosmos DB emulator and wait until it is healthy
	@$(COMPOSE) up -d --wait

.PHONY: emulator-seed
emulator-seed: ## Create the emulator's database and containers; safe to re-run
	@set -a && . ./.env.emulator && set +a && poetry run python -m scripts.bootstrap_emulator

.PHONY: emulator-down
emulator-down: ## Stop the emulator and discard its data
	@$(COMPOSE) down -v

.PHONY: emulator-logs
emulator-logs: ## Follow the emulator's logs
	@$(COMPOSE) logs -f

.PHONY: run-emulator
run-emulator: ## Run the API against the emulator (after emulator-up and emulator-seed)
	@echo "🚀 Running Function App against the Cosmos DB emulator"
	@set -a && . ./.env.emulator && set +a && poetry run func start

.PHONY: requirements
requirements: ## Export the main dependencies from poetry.lock to requirements.txt for deployment
	@poetry export --help >/dev/null 2>&1 || { echo "Run 'make install' first: it installs the poetry-plugin-export plugin declared in pyproject.toml." >&2; exit 1; }
	@poetry export --only main --format requirements.txt --output requirements.txt
	@echo "📦 Wrote requirements.txt"

.PHONY: build
build: clean-build ## Build wheel file using poetry
	@echo "🎡 Creating wheel file"
	@poetry build

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
