COVERAGE_THRESHOLD ?= 80

.PHONY: install
install: ## Install dependencies
	@echo "Installing dependencies"
	@go mod tidy

.PHONY: run
run: ## Run the function app
{%- if cloud_service == 'Azure Function App' %}
	@echo "Running Function App"
	@go build -o handler
	@func start
{%- elif cloud_service == 'GCP Cloud Function' %}
	@echo "Running the api function with the Functions Framework on port 8080"
	@FUNCTION_TARGET=api PORT=8080 go run ./cmd
{%- elif cloud_service == 'AWS Lambda' %}
	@echo "Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api
{%- endif %}

{% set emulator_seed = 'go run ./cmd/bootstrap' -%}
{% include 'shared/_Makefile.emulator' -%}
{%- if cloud_service == 'Azure Function App' %}
	@echo "Running Function App against the Cosmos DB emulator"
	@go build -o handler
	@set -a && . ./.env.emulator && set +a && func start
{%- elif cloud_service == 'GCP Cloud Function' %}
	@echo "Running the api function against the Firestore emulator on port 8080"
	@set -a && . ./.env.emulator && set +a && FUNCTION_TARGET=api PORT=8080 go run ./cmd
{%- elif cloud_service == 'AWS Lambda' %}
	@echo "Building and running Lambda API locally via SAM against DynamoDB Local"
	@sam build
	@set -a && . ./.env.emulator && set +a && sam local start-api --warm-containers LAZY --env-vars env.emulator.json --docker-network {{ project_endpoint }}-emulator
{%- endif %}

.PHONY: build
build: ## Build the code
{%- if cloud_service == 'Azure Function App' %}
	@echo "Building file"
	@go build -o handler
{%- elif cloud_service == 'GCP Cloud Function' %}
	@echo "Building the function and its local runner"
	@go build -o handler ./cmd
{%- elif cloud_service == 'AWS Lambda' %}
	@echo "Building Lambda binary"
	@GOOS=linux GOARCH=amd64 CGO_ENABLED=0 go build -o bootstrap
{%- endif %}

.PHONY: clean-build
clean-build: ## Clean the build artifacts
{%- if cloud_service == 'Azure Function App' %}
	@echo "Cleaning build artifacts"
	@rm -f handler
	@go build -o handler
{%- elif cloud_service == 'GCP Cloud Function' %}
	@echo "Cleaning build artifacts"
	@rm -f handler
	@go build -o handler ./cmd
{%- elif cloud_service == 'AWS Lambda' %}
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
	@go test ./... -v -count=1 -coverpkg=./controllers/...,./services/...,./handlers/...,./utils/... -coverprofile=coverage.out
	@total=$$(go tool cover -func=coverage.out | grep total | awk '{print $$3}' | tr -d '%'); \
	echo "Total coverage (logic packages): $$total%"; \
	awk "BEGIN { exit !($$total >= $(COVERAGE_THRESHOLD)) }" || \
		{ echo "FAIL: coverage $$total% is below threshold $(COVERAGE_THRESHOLD)%"; exit 1; }

.PHONY: audit
audit: ## Scan dependencies for known vulnerabilities
	@echo "Scanning dependencies for vulnerabilities"
	@go install golang.org/x/vuln/cmd/govulncheck@latest
	@govulncheck ./...

.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
