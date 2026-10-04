# Plans one stack's variables (terraform test -test-directory=tests/stacks -var-file=<stack>.tfvars.json).
# Mocked providers: no Azure credentials, and mocked values are known at plan time.
mock_provider "azurerm" {
  source          = "./tests/mocks/azurerm"
  override_during = plan
}

mock_provider "azuread" {
  source          = "./tests/mocks/azuread"
  override_during = plan
}

mock_provider "azapi" {
  override_during = plan
}

run "stack_plans" {
  command = plan
}
