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

variables {
  name   = "kitten-claws"
  stage  = "dev"
  region = "eastus"

  owner = {
    name  = "Kitten Claws"
    email = "admin@example.com"
  }

  compute = {
    hosting = "flex_consumption"
    sku     = "FC1"
    runtime = { name = "python", version = "{{ runtime.python }}" }
    app_settings = {
      Cosmos_Db_Uri               = "$${database_endpoint}"
      Cosmos_Db_Database_Name     = "$${database_name}"
      ConnectionStrings__CosmosDb = "AccountEndpoint=$${database_endpoint};"
      Container_Name_zoo_records  = "zoo-records"
    }
  }

  database = {
    capacity   = "serverless"
    containers = [{ name = "animals" }, { name = "zoo-records" }]
  }

  gateway = {
    sku             = "Developer"
    health_endpoint = "health"
    routes = [
      { name = "Cat", endpoint = "cats", operations = ["list", "get_by_id", "create", "update", "delete"] },
      { name = "Puma", endpoint = "pumas", operations = ["replace"] },
    ]
  }
}

run "names_come_from_the_naming_module" {
  command = plan

  assert {
    condition     = azurerm_virtual_network.this.name == "vnet-kitten-claws-dev" && can(regex("^apim-kitten-claws-dev-[a-z0-9]{4}$", azurerm_api_management.this.name))
    error_message = "Names are the CAF abbreviation, the project and the stage."
  }

  assert {
    condition     = { for role, group in azurerm_resource_group.this : role => group.name } == { for role in ["app", "data", "gateway", "monitoring", "network"] : role => "rg-kitten-claws-dev-${role}" }
    error_message = "Each part of the stack has its own resource group, named after what it holds."
  }

  assert {
    condition = alltrue([
      azurerm_function_app_flex_consumption.this[0].resource_group_name == azurerm_resource_group.this["app"].name,
      azurerm_storage_account.this.resource_group_name == azurerm_resource_group.this["app"].name,
      azurerm_user_assigned_identity.api.resource_group_name == azurerm_resource_group.this["app"].name,
      azurerm_cosmosdb_account.this.resource_group_name == azurerm_resource_group.this["data"].name,
      azurerm_private_endpoint.database.resource_group_name == azurerm_resource_group.this["data"].name,
      azurerm_api_management.this.resource_group_name == azurerm_resource_group.this["gateway"].name,
      azurerm_user_assigned_identity.gateway.resource_group_name == azurerm_resource_group.this["gateway"].name,
      azurerm_log_analytics_workspace.this.resource_group_name == azurerm_resource_group.this["monitoring"].name,
      azurerm_application_insights.this.resource_group_name == azurerm_resource_group.this["monitoring"].name,
      azurerm_virtual_network.this.resource_group_name == azurerm_resource_group.this["network"].name,
      alltrue([for zone in azurerm_private_dns_zone.this : zone.resource_group_name == azurerm_resource_group.this["network"].name]),
    ])
    error_message = "Every resource lives in the resource group for its part of the stack."
  }

  assert {
    condition     = azurerm_subnet.app.name == "snet-kitten-claws-dev-app" && azurerm_private_endpoint.database.name == "pep-kitten-claws-dev-data"
    error_message = "A type the component creates more than once is named after its role."
  }

  assert {
    condition     = can(regex("^stkittenclawsdev[a-z0-9]{4}$", azurerm_storage_account.this.name)) && can(regex("^cosno-kitten-claws-dev-[a-z0-9]{4}$", azurerm_cosmosdb_account.this.name))
    error_message = "Globally unique names end in four characters derived from the subscription."
  }
}

run "names_fit_azure_limits" {
  command = plan

  variables {
    name  = "a-very-long-project-name-for-testing"
    stage = "staging1"
  }

  assert {
    condition     = length(azurerm_storage_account.this.name) <= 24 && can(regex("^[a-z0-9]+$", azurerm_storage_account.this.name))
    error_message = "Storage account names are at most 24 lowercase letters and digits."
  }

  assert {
    condition     = length(azurerm_cosmosdb_account.this.name) <= 44
    error_message = "Cosmos DB account names are at most 44 characters."
  }

  assert {
    condition     = length(azurerm_api_management.this.name) <= 50 && !endswith(azurerm_api_management.this.name, "-")
    error_message = "API Management names are at most 50 characters."
  }

  assert {
    condition     = length(local.app_name) <= 60
    error_message = "Function app names are at most 60 characters."
  }
}
