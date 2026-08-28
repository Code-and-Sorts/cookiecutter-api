.PHONY: install
install: ## Install dependencies
	@echo "🧑‍💻 Installing dependencies"
	@dotnet restore

.PHONY: run
run: ## Run the API locally
	@echo "🚀 Running Lambda API locally via SAM"
	@cd KittenClaws.Api && sam build && sam local start-api

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
