.PHONY: install
install: ## Install dependencies
	@echo "🧑‍💻 Installing dependencies"
	@dotnet restore

.PHONY: run
run: ## Run the function app
	@echo "🚀 Running Function App"
	@cd {{cookiecutter.project_class_name}}.Api && func start

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
	@dotnet test

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
