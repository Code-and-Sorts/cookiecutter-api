.PHONY: install
install: ## Install dependencies
	@echo "🧑‍💻 Installing dependencies"
	@dotnet restore

.PHONY: run
run: ## Run the API locally
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	@echo "🚀 Running Function App"
	@cd {{cookiecutter.project_class_name}}.Api && func start
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
	@echo "🚀 Running Cloud Function"
	@cd {{cookiecutter.project_class_name}}.Api && dotnet run
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
	@echo "🚀 Running Lambda API locally via SAM"
	@cd {{cookiecutter.project_class_name}}.Api && sam build && sam local start-api
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
