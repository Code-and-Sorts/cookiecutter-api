.PHONY: install
install: ## Install dependencies
	@echo "🧑‍💻 Installing dependencies"
	@dotnet restore

.PHONY: run
run: ## Run the API locally
	@echo "🚀 Running Cloud Function"
	@cd KittenClaws.Api && dotnet run

COMPOSE ?= docker compose

.PHONY: emulator-up
emulator-up: ## Start the local Firestore emulator and wait until it is healthy
	@$(COMPOSE) up -d --wait

.PHONY: emulator-seed
emulator-seed: ## Wait until the emulator answers; safe to re-run
	@set -a && . ./.env.emulator && set +a && dotnet run --project KittenClaws.Bootstrap

.PHONY: emulator-down
emulator-down: ## Stop the emulator and discard its data
	@$(COMPOSE) down -v

.PHONY: emulator-logs
emulator-logs: ## Follow the emulator's logs
	@$(COMPOSE) logs -f

.PHONY: run-emulator
run-emulator: ## Run the API against the emulator (after emulator-up and emulator-seed)
	@echo "🚀 Running Cloud Function against the Firestore emulator"
	@set -a && . ./.env.emulator && set +a && cd KittenClaws.Api && dotnet run

.PHONY: build
build: ## Build the code
	@echo "🎡 Building file"
	@dotnet build

.PHONY: clean-build
clean-build: ## Clean the build artifacts
	@echo "🧹 Cleaning build artifacts"
	@dotnet clean
	@dotnet build

.PHONY: lint
lint: ## Lint the code with dotnet format
	@echo "🔍 Linting code: Running dotnet format"
	@dotnet format style --verify-no-changes --severity error

.PHONY: format
format: ## Format the code with dotnet format
	@echo "🎨 Formatting code: Running dotnet format"
	@dotnet format

.PHONY: test-unit
test-unit: ## Test the code with unit tests
	@echo "🧪 Testing code: Running unit tests"
	@dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

.PHONY: audit
audit: ## Scan dependencies for known vulnerabilities
	@echo "🔍 Scanning dependencies for vulnerabilities"
	@dotnet list package --vulnerable --include-transitive

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
