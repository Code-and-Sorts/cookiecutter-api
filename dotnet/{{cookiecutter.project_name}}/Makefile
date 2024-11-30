.PHONY: run
run: ## Run the function app
	@echo "🚀 Running Function App"
	@cd {{cookiecutter.project_name}}.Api && func start --dotnet

.PHONY: build
build: ## Build the code
	@echo "🎡 Building file"
	@dotnet build

.PHONY: clean-build
clean-build: ## Clean the build artifacts
	@echo "🧹 Cleaning build artifacts"
	@dotnet clean
	@dotnet build

.PHONY: test-unit
test-unit: ## Test the code with unit tests
	@echo "🧪 Testing code: Running unit tests"
	@dotnet test

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
