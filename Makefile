.PHONY: install
install: ## Install the poetry environment
	@echo "🧑‍💻 Creating virtual environment using pyenv and poetry"
	@poetry install --no-interaction

.PHONY: run
run: ## Run function app.
	@echo "🚀 Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api

.PHONY: build
build: install ## Build the Lambda package with SAM
	@echo "🎡 Building the Lambda package"
	@sam build

# SAM's builder can't read Poetry, so `sam build` runs this target (BuildMethod: makefile).
# pip runs on the Poetry env's Python so dependency markers match the Lambda runtime.
LAMBDA_PYTHON = $(shell poetry env info --executable)
LAMBDA_PLATFORMS := manylinux2014_x86_64 manylinux_2_17_x86_64 manylinux_2_28_x86_64 manylinux_2_34_x86_64

.PHONY: build-KittenClawsFunction
build-KittenClawsFunction:
	@poetry export --help >/dev/null 2>&1 && [ -n "$(LAMBDA_PYTHON)" ] || { echo "Run 'make install' first: it creates the Poetry environment and installs the poetry-plugin-export plugin declared in pyproject.toml." >&2; exit 1; }
	poetry export --only main --format requirements.txt --output "$(ARTIFACTS_DIR)/requirements.txt"
	"$(LAMBDA_PYTHON)" -m pip install --quiet --disable-pip-version-check --no-compile \
		--requirement "$(ARTIFACTS_DIR)/requirements.txt" --target "$(ARTIFACTS_DIR)" \
		--implementation cp --python-version 3.14 --only-binary=:all: \
		$(addprefix --platform ,$(LAMBDA_PLATFORMS))
	rm "$(ARTIFACTS_DIR)/requirements.txt"
	find . -name '*.py' ! -name '*_test.py' ! -name conftest.py ! -path './.*' | while read -r file; do \
		mkdir -p "$(ARTIFACTS_DIR)/$$(dirname "$$file")" && cp "$$file" "$(ARTIFACTS_DIR)/$$file"; \
	done

.PHONY: clean-build
clean-build: ## Clean build artifacts
	@echo "🧹 Cleaning build artifacts"
	@rm -rf dist

.PHONY: lint
lint: ## Lint the code with ruff
	@echo "🔍 Linting code: Running ruff"
	@poetry run ruff check .

.PHONY: format
format: ## Format the code with ruff
	@echo "🎨 Formatting code: Running ruff"
	@poetry run ruff check --fix .
	@poetry run ruff format .

.PHONY: test-unit
test-unit: ## Test the code with pytest unit tests.
	@echo "🧪 Testing code: Running pytest unit tests"
	@poetry run pytest --cov

.PHONY: audit
audit: ## Scan dependencies for known vulnerabilities
	@echo "🔍 Scanning dependencies for vulnerabilities"
	@poetry run pip install pip-audit
	@poetry run pip-audit

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
