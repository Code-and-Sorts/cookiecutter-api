.PHONY: install
install: ## Install the poetry environment
	@echo "🧑‍💻 Creating virtual environment using pyenv and poetry"
	@poetry install
	@poetry shell

.PHONY: run
run: ## Test the code with pytest.
	@echo "🚀 Running Function App"
	@poetry run func start

.PHONY: build
build: clean-build ## Build wheel file using poetry
	@echo "🎡 Creating wheel file"
	@poetry build

.PHONY: clean-build
clean-build: ## Clean build artifacts
	@echo "🧹 Cleaning build artifacts"
	@rm -rf dist

.PHONY: test
test: ## Test the code with pytest unit tests.
	@echo "🧪 Testing code: Running pytest unit tests"
	@poetry run pytest --cov

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
