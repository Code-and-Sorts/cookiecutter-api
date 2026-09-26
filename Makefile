COVERAGE_THRESHOLD ?= 80

.PHONY: install
install: ## Install dependencies
	@echo "Installing dependencies"
	@go mod tidy

.PHONY: run
run: ## Run the function app
	@echo "Running Function App"
	@go build -o handler
	@func start

.PHONY: build
build: ## Build the code
	@echo "Building file"
	@go build -o handler

.PHONY: clean-build
clean-build: ## Clean the build artifacts
	@echo "Cleaning build artifacts"
	@rm -f handler
	@go build -o handler

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
