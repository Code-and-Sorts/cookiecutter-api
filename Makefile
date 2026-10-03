.PHONY: install
install: ## Install the uv environment
	@echo "🧑‍💻 Creating virtual environment using uv"
	@uv sync

.PHONY: run
run: ## Run function app.
	@echo "🚀 Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api

COMPOSE ?= docker compose

.PHONY: emulator-up
emulator-up: ## Start the local DynamoDB emulator and wait until it is healthy
	@$(COMPOSE) up -d --wait

.PHONY: emulator-seed
emulator-seed: ## Create the emulator's tables; safe to re-run
	@set -a && . ./.env.emulator && set +a && uv run python -m scripts.bootstrap_emulator

.PHONY: emulator-down
emulator-down: ## Stop the emulator and discard its data
	@$(COMPOSE) down -v

.PHONY: emulator-logs
emulator-logs: ## Follow the emulator's logs
	@$(COMPOSE) logs -f

.PHONY: run-emulator
run-emulator: ## Run the API against the emulator (after emulator-up and emulator-seed)
	@echo "🚀 Building and running Lambda API locally via SAM against DynamoDB Local"
	@sam build
	@set -a && . ./.env.emulator && set +a && sam local start-api --warm-containers LAZY --env-vars env.emulator.json --docker-network kittenclaws-emulator

.PHONY: build
build: install ## Build the Lambda package with SAM
	@echo "🎡 Building the Lambda package"
	@sam build

# SAM's builder can't read uv, so `sam build` runs this target (BuildMethod: makefile).
# The python3.14 Lambda runtime runs on Amazon Linux 2023, whose glibc is 2.34.
.PHONY: build-KittenClawsFunction
build-KittenClawsFunction:
	uv export --quiet --no-dev --format requirements-txt --output-file "$(ARTIFACTS_DIR)/requirements.txt"
	uv pip install --quiet --requirement "$(ARTIFACTS_DIR)/requirements.txt" --target "$(ARTIFACTS_DIR)" \
		--python-platform x86_64-manylinux_2_34 --python-version 3.14 --only-binary :all:
	rm "$(ARTIFACTS_DIR)/requirements.txt"
	find . -name '*.py' ! -name '*_test.py' ! -name conftest.py ! -path './.*' ! -path './scripts/*' | while read -r file; do \
		mkdir -p "$(ARTIFACTS_DIR)/$$(dirname "$$file")" && cp "$$file" "$(ARTIFACTS_DIR)/$$file"; \
	done

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
