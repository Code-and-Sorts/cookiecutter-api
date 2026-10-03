.PHONY: install
install: ## Install the uv environment
	@echo "🧑‍💻 Creating virtual environment using uv"
	@uv sync

.PHONY: run
run: ## Run function app.
	@echo "🚀 Running the api Cloud Function on http://localhost:8080"
	@uv run functions-framework --target=api --source=main.py --port=8080

COMPOSE ?= docker compose

.PHONY: emulator-up
emulator-up: ## Start the local Firestore emulator and wait until it is healthy
	@$(COMPOSE) up -d --wait

.PHONY: emulator-seed
emulator-seed: ## Wait until the emulator answers; safe to re-run
	@set -a && . ./.env.emulator && set +a && uv run python -m scripts.bootstrap_emulator

.PHONY: emulator-down
emulator-down: ## Stop the emulator and discard its data
	@$(COMPOSE) down -v

.PHONY: emulator-logs
emulator-logs: ## Follow the emulator's logs
	@$(COMPOSE) logs -f

.PHONY: run-emulator
run-emulator: ## Run the API against the emulator (after emulator-up and emulator-seed)
	@echo "🚀 Running the api Cloud Function against the Firestore emulator on http://localhost:8080"
	@set -a && . ./.env.emulator && set +a && uv run functions-framework --target=api --source=main.py --port=8080

.PHONY: requirements
requirements: ## Export the main dependencies from uv.lock to requirements.txt for deployment
	@uv export --quiet --no-dev --format requirements-txt --output-file requirements.txt
	@echo "📦 Wrote requirements.txt"

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
