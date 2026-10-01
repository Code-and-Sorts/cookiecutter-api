.PHONY: install
install: ## Install dependencies
	@echo "🧑‍💻 Installing dependencies"
	@dotnet restore

.PHONY: run
run: ## Run the API locally
{%- if cloud_service == 'Azure Function App' %}
	@echo "🚀 Running Function App"
	@cd {{project_class_name}}.Api && func start
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	@echo "🚀 Running Cloud Function"
	@cd {{project_class_name}}.Api && dotnet run
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	@echo "🚀 Running Lambda API locally via SAM"
	@cd {{project_class_name}}.Api && sam build && sam local start-api
{%- endif %}

COMPOSE ?= docker compose

.PHONY: emulator-up
emulator-up: ## Start the local {{ {'Azure Function App': 'Cosmos DB', 'GCP Cloud Function': 'Firestore', 'AWS Lambda': 'DynamoDB'}[cloud_service] }} emulator and wait until it is healthy
	@$(COMPOSE) up -d --wait

.PHONY: emulator-seed
emulator-seed: ## {{ {'Azure Function App': "Create the emulator's database and containers", 'GCP Cloud Function': 'Wait until the emulator answers', 'AWS Lambda': "Create the emulator's tables"}[cloud_service] }}; safe to re-run
	@set -a && . ./.env.emulator && set +a && dotnet run --project {{project_class_name}}.Bootstrap

.PHONY: emulator-down
emulator-down: ## Stop the emulator and discard its data
	@$(COMPOSE) down -v

.PHONY: emulator-logs
emulator-logs: ## Follow the emulator's logs
	@$(COMPOSE) logs -f

.PHONY: run-emulator
run-emulator: ## Run the API against the emulator (after emulator-up and emulator-seed)
{%- if cloud_service == 'Azure Function App' %}
	@echo "🚀 Running Function App against the Cosmos DB emulator"
	@set -a && . ./.env.emulator && set +a && cd {{project_class_name}}.Api && func start
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	@echo "🚀 Running Cloud Function against the Firestore emulator"
	@set -a && . ./.env.emulator && set +a && cd {{project_class_name}}.Api && dotnet run
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	@echo "🚀 Running Lambda API locally via SAM against DynamoDB Local"
	@set -a && . ./.env.emulator && set +a && cd {{project_class_name}}.Api && sam build && sam local start-api --env-vars ../env.emulator.json --docker-network {{ project_endpoint }}-emulator
{%- endif %}

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
