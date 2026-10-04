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
    containers = ["animals", "zoo-records"]
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

run "flex_consumption_defaults" {
  command = plan

  assert {
    condition     = length(azurerm_function_app_flex_consumption.this) == 1 && length(azurerm_linux_function_app.this) == 0 && length(azurerm_container_app.this) == 0
    error_message = "Flex Consumption hosting creates exactly one Flex Consumption app."
  }

  assert {
    condition     = azurerm_service_plan.this[0].sku_name == "FC1"
    error_message = "The plan uses the FC1 SKU."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].runtime_name == "python" && azurerm_function_app_flex_consumption.this[0].runtime_version == "{{ runtime.python }}"
    error_message = "The runtime comes from compute.runtime."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].virtual_network_subnet_id == azurerm_subnet.app.id
    error_message = "The app reaches Cosmos DB through the virtual network."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].site_config[0].ip_restriction_default_action == "Deny"
    error_message = "The API is not reachable publicly; only its private endpoint answers."
  }

  assert {
    condition     = length(azurerm_private_endpoint.app) == 1
    error_message = "The app gets a private endpoint for API Management."
  }

  assert {
    condition     = azurerm_storage_account.this.shared_access_key_enabled == false && azurerm_storage_account.this.network_rules[0].default_action == "Deny"
    error_message = "The storage account uses Entra ID only and admits only the app subnet."
  }

  assert {
    condition     = azurerm_storage_container.deployments[0].name == "deployments"
    error_message = "Flex Consumption gets a deployment container."
  }

  assert {
    condition     = toset(keys(azurerm_subnet_network_security_group_association.default)) == toset(["app", "endpoints"])
    error_message = "Every subnet has a network security group."
  }
}

run "app_settings_are_templated" {
  command = plan

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].app_settings["Cosmos_Db_Uri"] == "https://cosmos.documents.azure.com:443/"
    error_message = "$${database_endpoint} is replaced by the Cosmos DB endpoint."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].app_settings["Cosmos_Db_Database_Name"] == "kitten-claws"
    error_message = "$${database_name} is replaced by the database name."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].app_settings["ConnectionStrings__CosmosDb"] == "AccountEndpoint=https://cosmos.documents.azure.com:443/;"
    error_message = "Placeholders are replaced inside longer values."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].app_settings["Container_Name_zoo_records"] == "zoo-records"
    error_message = "Plain values pass through."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].app_settings["AZURE_CLIENT_ID"] == "22222222-2222-2222-2222-222222222222"
    error_message = "The app is told which managed identity to use."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].app_settings["AzureWebJobsStorage__credential"] == "managedidentity"
    error_message = "The host reaches storage with its managed identity."
  }
}

run "database_is_private_and_keyless" {
  command = plan

  assert {
    condition     = azurerm_cosmosdb_account.this.public_network_access_enabled == false && azurerm_cosmosdb_account.this.local_authentication_enabled == false
    error_message = "Cosmos DB has no public access and no keys."
  }

  assert {
    condition     = [for c in azurerm_cosmosdb_account.this.capabilities : c.name] == ["EnableServerless"]
    error_message = "Serverless capacity sets the EnableServerless capability."
  }

  assert {
    condition     = keys(azurerm_cosmosdb_sql_database.this) == [var.name] && length(azurerm_cosmosdb_sql_database.this[var.name].autoscale_settings) == 0
    error_message = "The API's database is named after the project and has no autoscale throughput when serverless."
  }

  assert {
    condition     = toset([for c in azurerm_cosmosdb_sql_container.this : c.name]) == toset(["animals", "zoo-records"]) && alltrue([for c in azurerm_cosmosdb_sql_container.this : c.database_name == var.name])
    error_message = "One container per container id, in the API's database."
  }

  assert {
    condition     = alltrue([for c in azurerm_cosmosdb_sql_container.this : c.partition_key_paths == tolist(["/id"])])
    error_message = "Containers are partitioned by /id, as the emulator bootstrap creates them."
  }

  assert {
    condition     = endswith(azurerm_cosmosdb_sql_role_assignment.api.role_definition_id, "/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002")
    error_message = "The API identity gets the Cosmos DB Built-in Data Contributor role."
  }

  assert {
    condition     = length(azurerm_management_lock.database) == 0
    error_message = "No delete lock unless asked for."
  }
}

run "provisioned_database" {
  command = plan

  variables {
    database = {
      capacity    = "provisioned"
      throughput  = 800
      containers  = ["animals"]
      delete_lock = true
    }
  }

  assert {
    condition     = length(azurerm_cosmosdb_account.this.capabilities) == 0
    error_message = "Provisioned capacity is not serverless."
  }

  assert {
    condition     = azurerm_cosmosdb_sql_database.this[var.name].throughput == 800
    error_message = "Provisioned throughput is shared by the database."
  }

  assert {
    condition     = length(azurerm_management_lock.database) == 1
    error_message = "delete_lock locks the account."
  }
}

run "autoscale_database" {
  command = plan

  variables {
    database = {
      capacity   = "autoscale"
      throughput = 4000
      containers = ["animals"]
    }
  }

  assert {
    condition     = azurerm_cosmosdb_sql_database.this[var.name].autoscale_settings[0].max_throughput == 4000
    error_message = "Autoscale sets the maximum throughput."
  }
}

run "additional_databases" {
  command = plan

  variables {
    database = {
      capacity   = "provisioned"
      throughput = 400
      containers = ["animals"]
      databases = {
        reporting = { throughput = 1000, containers = ["events", "animals"] }
        archive   = {}
      }
    }
  }

  assert {
    condition     = toset(keys(azurerm_cosmosdb_sql_database.this)) == toset([var.name, "reporting", "archive"])
    error_message = "Every database in database.databases is created beside the API's."
  }

  assert {
    condition     = azurerm_cosmosdb_sql_database.this["reporting"].throughput == 1000 && azurerm_cosmosdb_sql_database.this["archive"].throughput == 400
    error_message = "A database's throughput defaults to database.throughput."
  }

  assert {
    condition     = toset(keys(azurerm_cosmosdb_sql_container.this)) == toset(["${var.name}/animals", "reporting/events", "reporting/animals"])
    error_message = "Containers belong to their database, so two databases can hold the same container id."
  }

  assert {
    condition     = azurerm_cosmosdb_sql_container.this["reporting/events"].database_name == "reporting"
    error_message = "A container is created in its own database."
  }
}

run "rejects_a_database_named_like_the_api" {
  command = plan

  variables {
    database = {
      capacity   = "serverless"
      containers = ["animals"]
      databases  = { "kitten-claws" = {} }
    }
  }

  expect_failures = [var.database]
}

run "rejects_too_little_throughput_in_another_database" {
  command = plan

  variables {
    database = {
      capacity   = "provisioned"
      throughput = 400
      containers = ["animals"]
      databases  = { reporting = { throughput = 450 } }
    }
  }

  expect_failures = [var.database]
}

run "premium_hosting" {
  command = plan

  variables {
    compute = {
      hosting       = "premium"
      sku           = "EP2"
      runtime       = { name = "dotnet-isolated", version = "{{ runtime.dotnet }}.0" }
      min_instances = 2
    }
  }

  assert {
    condition     = length(azurerm_linux_function_app.this) == 1 && length(azurerm_function_app_flex_consumption.this) == 0
    error_message = "Premium hosting creates a Linux function app."
  }

  assert {
    condition     = azurerm_service_plan.this[0].sku_name == "EP2"
    error_message = "The plan uses the chosen Elastic Premium SKU."
  }

  assert {
    condition     = azurerm_linux_function_app.this[0].site_config[0].application_stack[0].use_dotnet_isolated_runtime == true
    error_message = "dotnet-isolated selects the isolated worker."
  }

  assert {
    condition     = azurerm_linux_function_app.this[0].site_config[0].elastic_instance_minimum == 2
    error_message = "min_instances keeps Premium instances warm."
  }

  assert {
    condition     = azurerm_linux_function_app.this[0].app_settings["WEBSITE_CONTENTOVERVNET"] == "1" && azurerm_linux_function_app.this[0].app_settings["WEBSITE_CONTENTSHARE"] == "content"
    error_message = "Premium reads its content share over the virtual network."
  }

  assert {
    condition     = azurerm_storage_account.this.shared_access_key_enabled == true
    error_message = "The Premium content share needs the account key."
  }

  assert {
    condition     = !contains(keys(azurerm_linux_function_app.this[0].app_settings), "AzureWebJobsStorage__credential")
    error_message = "Premium uses the key-based storage connection."
  }

  assert {
    condition     = azurerm_subnet.app.delegation[0].service_delegation[0].name == "Microsoft.Web/serverFarms"
    error_message = "Plan-based apps integrate through a Microsoft.Web/serverFarms subnet."
  }
}

run "app_service_hosting" {
  command = plan

  variables {
    compute = {
      hosting = "app_service"
      sku     = "P1v3"
      runtime = { name = "custom", version = "1.0" }
    }
  }

  assert {
    condition     = azurerm_linux_function_app.this[0].site_config[0].always_on == true
    error_message = "Dedicated plans keep the app loaded."
  }

  assert {
    condition     = azurerm_linux_function_app.this[0].site_config[0].application_stack[0].use_custom_runtime == true
    error_message = "custom selects a custom handler."
  }

  assert {
    condition     = azurerm_linux_function_app.this[0].storage_uses_managed_identity == true
    error_message = "Dedicated plans reach storage with the managed identity."
  }
}

run "container_app_hosting" {
  command = plan

  variables {
    compute = {
      hosting       = "container_app"
      sku           = "D4"
      runtime       = { name = "node", version = "{{ runtime.node }}" }
      max_instances = 5
    }
  }

  assert {
    condition     = length(azurerm_container_app.this) == 1 && length(azurerm_service_plan.this) == 0 && length(azurerm_private_endpoint.app) == 0
    error_message = "Container Apps hosting replaces the plan, function app and its private endpoint."
  }

  assert {
    condition     = azurerm_container_app_environment.this[0].internal_load_balancer_enabled == true
    error_message = "The Container Apps environment is internal."
  }

  assert {
    condition     = azurerm_container_app.this[0].workload_profile_name == "dedicated" && azurerm_container_app.this[0].template[0].max_replicas == 5
    error_message = "A dedicated workload profile runs the app, scaled by compute."
  }

  assert {
    condition     = length(azapi_resource.container_app_auth) == 1
    error_message = "Container Apps authentication requires Entra ID."
  }

  assert {
    condition     = length(azurerm_container_registry.this) == 1 && length(azurerm_role_assignment.api_registry) == 1
    error_message = "The app pulls its image from a registry with its identity."
  }

  assert {
    condition     = azurerm_subnet.app.delegation[0].service_delegation[0].name == "Microsoft.App/environments"
    error_message = "Container Apps environments need a Microsoft.App/environments subnet."
  }
}

run "gateway_publishes_the_routes" {
  command = plan

  assert {
    condition = toset(keys(azurerm_api_management_api_operation.api)) == toset([
      "cats-list", "cats-get_by_id", "cats-create", "cats-update", "cats-delete", "pumas-replace",
    ])
    error_message = "One operation per enabled resource operation."
  }

  assert {
    condition     = azurerm_api_management_api_operation.api["cats-update"].method == "PATCH" && azurerm_api_management_api_operation.api["cats-update"].url_template == "/cats/{id}"
    error_message = "update is PATCH on the item."
  }

  assert {
    condition     = azurerm_api_management_api_operation.api["pumas-replace"].method == "PUT"
    error_message = "replace is PUT."
  }

  assert {
    condition     = azurerm_api_management_api_operation.api["cats-list"].url_template == "/cats" && length(azurerm_api_management_api_operation.api["cats-list"].template_parameter) == 0
    error_message = "list is on the collection."
  }

  assert {
    condition     = azurerm_api_management_api.api.subscription_required == true && azurerm_api_management_api.api.subscription_key_parameter_names[0].header == "x-api-key"
    error_message = "Resource routes need an API key in x-api-key."
  }

  assert {
    condition     = azurerm_api_management_api.api.service_url == "https://func.azurewebsites.net/api"
    error_message = "The gateway forwards to the app's /api routes."
  }

  assert {
    condition     = strcontains(azurerm_api_management_api_policy.api.xml_content, "<authentication-managed-identity resource=\"55555555-5555-5555-5555-555555555555\" client-id=\"22222222-2222-2222-2222-222222222222\" />")
    error_message = "The gateway authenticates to the app with an Entra ID token for its identity."
  }

  assert {
    condition     = azurerm_api_management_api.health[0].subscription_required == false && azurerm_api_management_api.health[0].path == "api/health"
    error_message = "The health check is open."
  }

  assert {
    condition     = strcontains(azurerm_api_management_api_policy.health[0].xml_content, "<rewrite-uri template=\"/health\" />")
    error_message = "The health API forwards to the app's health route."
  }

  assert {
    condition     = azurerm_api_management.this.sku_name == "Developer_1" && azurerm_api_management.this.virtual_network_type == "External"
    error_message = "The gateway runs in the virtual network."
  }

  assert {
    condition     = length(azurerm_subnet.gateway.delegation) == 0
    error_message = "Developer is injected into an undelegated subnet."
  }
}

run "standard_v2_gateway_without_health" {
  command = plan

  variables {
    gateway = {
      sku      = "StandardV2"
      capacity = 2
      routes   = [{ name = "Owl", endpoint = "owls", operations = ["delete"] }]
    }
  }

  assert {
    condition     = azurerm_api_management.this.sku_name == "StandardV2_2"
    error_message = "The tier and capacity form the SKU."
  }

  assert {
    condition     = azurerm_subnet.gateway.delegation[0].service_delegation[0].name == "Microsoft.Web/serverFarms"
    error_message = "v2 tiers integrate through a delegated subnet."
  }

  assert {
    condition     = length(azurerm_api_management_api.health) == 0
    error_message = "No health API without a health endpoint."
  }
}

run "entra_id_guards_the_app" {
  command = plan

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].auth_settings_v2[0].require_authentication == true && azurerm_function_app_flex_consumption.this[0].auth_settings_v2[0].unauthenticated_action == "Return401"
    error_message = "Requests without a token get 401."
  }

  assert {
    condition     = azurerm_function_app_flex_consumption.this[0].auth_settings_v2[0].active_directory_v2[0].allowed_applications == tolist(["22222222-2222-2222-2222-222222222222"])
    error_message = "Only the gateway's identity is admitted."
  }

  assert {
    condition     = azuread_application.api.api[0].requested_access_token_version == 2
    error_message = "Tokens are v2, so their audience is the client id."
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

run "rejects_a_sku_that_does_not_fit_the_hosting" {
  command = plan

  variables {
    compute = {
      hosting = "flex_consumption"
      sku     = "EP1"
      runtime = { name = "python", version = "{{ runtime.python }}" }
    }
  }

  expect_failures = [var.compute]
}

run "rejects_a_gateway_tier_without_private_networking" {
  command = plan

  variables {
    gateway = {
      sku    = "Consumption"
      routes = []
    }
  }

  expect_failures = [var.gateway]
}

run "rejects_too_little_autoscale_throughput" {
  command = plan

  variables {
    database = {
      capacity   = "autoscale"
      throughput = 400
      containers = ["animals"]
    }
  }

  expect_failures = [var.database]
}
