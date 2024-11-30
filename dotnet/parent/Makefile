.PHONY: install
install: ## Install the poetry environment
	@echo "🧑‍💻 Installing packages"
	@cd Kitty.API
	@dotnet install

.PHONY: run
run: ## Test the code with pytest.
	@echo "🚀 Running Function App"
	@cd Kitty.API
	@func start --dotnet

.PHONY: build
build: ## Build the code
	@echo "🎡 Building file"
	@cd Kitty.API
	@dotnet build

.PHONY: clean-build
clean-build: ## Clean the build artifacts
	@echo "🧹 Cleaning build artifacts"
	@cd Kitty.API
	@dotnet clean
	@dotnet build

.PHONY: test-unit
test: ## Test the code with unit tests.
	@echo "🧪 Testing code: Running unit tests"
	@cd Kitty.API.Tests.Unit
	@dotnet test

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
