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
