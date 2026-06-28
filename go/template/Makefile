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
	@echo "Running Cloud Function"
	@go build -o handler
	@PORT=8080 ./handler
{%- elif cloud_service == 'AWS Lambda' %}
	@echo "Building and running Lambda API locally via SAM"
	@sam build
	@sam local start-api
{%- endif %}

.PHONY: build
build: ## Build the code
{%- if cloud_service == 'Azure Function App' %}
	@echo "Building file"
	@go build -o handler
{%- elif cloud_service == 'GCP Cloud Function' %}
	@echo "Building file"
	@go build -o handler
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
	@go build -o handler
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
	@go test ./... -v -count=1 -coverpkg=./controllers/...,./services/...,./utils/... -coverprofile=coverage.out
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
