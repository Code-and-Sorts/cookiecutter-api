.PHONY: install
install: ## Install dependencies
	@echo "Installing dependencies"
	@go mod tidy

.PHONY: run
run: ## Run the function app
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	@echo "Running Function App"
	@go build -o handler
	@func start
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' %}
	@echo "Running Cloud Function"
	@go build -o handler
	@PORT=8080 ./handler
{%- elif cookiecutter.cloud_service == 'AWS Lambda' %}
	@echo "Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api
{%- endif %}

.PHONY: build
build: ## Build the code
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	@echo "Building file"
	@go build -o handler
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' %}
	@echo "Building file"
	@go build -o handler
{%- elif cookiecutter.cloud_service == 'AWS Lambda' %}
	@echo "Building Lambda binary"
	@GOOS=linux GOARCH=amd64 CGO_ENABLED=0 go build -o bootstrap
{%- endif %}

.PHONY: clean-build
clean-build: ## Clean the build artifacts
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	@echo "Cleaning build artifacts"
	@rm -f handler
	@go build -o handler
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' %}
	@echo "Cleaning build artifacts"
	@rm -f handler
	@go build -o handler
{%- elif cookiecutter.cloud_service == 'AWS Lambda' %}
	@echo "Cleaning build artifacts"
	@rm -f bootstrap
	@GOOS=linux GOARCH=amd64 CGO_ENABLED=0 go build -o bootstrap
{%- endif %}

.PHONY: lint
lint: ## Lint the code with golangci-lint
	@echo "Linting code: Running golangci-lint"
	@golangci-lint run ./...

.PHONY: format
format: ## Format the code with gofmt
	@echo "Formatting code: Running gofmt"
	@gofmt -w .

.PHONY: test-unit
test-unit: ## Test the code with unit tests
	@echo "Testing code: Running unit tests"
	@go test ./... -v -count=1

.PHONY: audit
audit: ## Scan dependencies for known vulnerabilities
	@echo "Scanning dependencies for vulnerabilities"
	@go install golang.org/x/vuln/cmd/govulncheck@latest
	@govulncheck ./...

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
