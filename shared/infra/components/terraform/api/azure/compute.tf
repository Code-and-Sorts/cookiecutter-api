locals {
  setting_values = {
    database_endpoint = azurerm_cosmosdb_account.this.endpoint
    database_name     = azurerm_cosmosdb_sql_database.this[var.name].name
  }

  storage_identity_settings = local.storage_key_access ? {} : {
    AzureWebJobsStorage__accountName = azurerm_storage_account.this.name
    AzureWebJobsStorage__credential  = "managedidentity"
    AzureWebJobsStorage__clientId    = azurerm_user_assigned_identity.api.client_id
  }

  telemetry_settings = {
    OTEL_SERVICE_NAME = var.name
    OTEL_RESOURCE_ATTRIBUTES = join(",", [
      "deployment.environment.name=${var.stage}",
      "cloud.region=${var.region}",
      "cloud.platform=${local.functions_hosting ? "azure_functions" : "azure_container_apps"}",
    ])
  }

  app_settings = merge(
    { for name, value in var.compute.app_settings : name => templatestring(value, local.setting_values) },
    local.storage_identity_settings,
    local.telemetry_settings,
    { AZURE_CLIENT_ID = azurerm_user_assigned_identity.api.client_id },
  )

  app_name     = local.functions_hosting ? local.names.site_function_app.name_unique : local.names.container_app.name
  scm_action   = var.compute.public_deployments ? "Allow" : "Deny"
  runtime_name = var.compute.runtime.name

  container_cpu    = var.compute.cpu
  container_memory = "${local.container_cpu * 2}Gi"

  app_hostname = one(concat(
    azurerm_function_app_flex_consumption.this[*].default_hostname,
    azurerm_linux_function_app.this[*].default_hostname,
    [for app in azurerm_container_app.this : app.ingress[0].fqdn],
  ))
  app_id = one(concat(
    azurerm_function_app_flex_consumption.this[*].id,
    azurerm_linux_function_app.this[*].id,
    azurerm_container_app.this[*].id,
  ))
}

resource "azurerm_service_plan" "this" {
  count = local.functions_hosting ? 1 : 0

  name                         = local.names.server_farm.name
  resource_group_name          = azurerm_resource_group.this["app"].name
  location                     = azurerm_resource_group.this["app"].location
  os_type                      = "Linux"
  sku_name                     = var.compute.sku
  maximum_elastic_worker_count = var.compute.hosting == "premium" ? var.compute.max_instances : null
  tags                         = local.tags
}

resource "azurerm_function_app_flex_consumption" "this" {
  count = var.compute.hosting == "flex_consumption" ? 1 : 0

  name                                           = local.app_name
  resource_group_name                            = azurerm_resource_group.this["app"].name
  location                                       = azurerm_resource_group.this["app"].location
  service_plan_id                                = azurerm_service_plan.this[0].id
  runtime_name                                   = var.compute.runtime.name
  runtime_version                                = var.compute.runtime.version
  storage_container_type                         = "blobContainer"
  storage_container_endpoint                     = "${azurerm_storage_account.this.primary_blob_endpoint}${azurerm_storage_container.deployments[0].name}"
  storage_authentication_type                    = "UserAssignedIdentity"
  storage_user_assigned_identity_id              = azurerm_user_assigned_identity.api.id
  maximum_instance_count                         = var.compute.max_instances
  instance_memory_in_mb                          = var.compute.instance_memory_mb
  virtual_network_subnet_id                      = azurerm_subnet.app.id
  public_network_access_enabled                  = var.compute.public_deployments
  https_only                                     = true
  webdeploy_publish_basic_authentication_enabled = false
  app_settings                                   = local.app_settings
  tags                                           = local.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  site_config {
    application_insights_connection_string = azurerm_application_insights.this.connection_string
    minimum_tls_version                    = "1.2"
    ip_restriction_default_action          = "Deny"
    scm_use_main_ip_restriction            = false
    scm_ip_restriction_default_action      = local.scm_action
  }

  dynamic "always_ready" {
    for_each = var.compute.min_instances > 0 ? [var.compute.min_instances] : []
    content {
      name           = "http"
      instance_count = always_ready.value
    }
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    require_https          = true
    unauthenticated_action = "Return401"
    default_provider       = "azureactivedirectory"

    active_directory_v2 {
      client_id            = azuread_application.api.client_id
      tenant_auth_endpoint = local.entra_issuer
      allowed_audiences    = local.entra_audiences
      allowed_applications = [azurerm_user_assigned_identity.gateway.client_id]
    }

    login {
      token_store_enabled = false
    }
  }

  depends_on = [azurerm_role_assignment.api_storage, azurerm_cosmosdb_sql_role_assignment.api]
}

resource "azurerm_linux_function_app" "this" {
  count = contains(["app_service", "premium"], var.compute.hosting) ? 1 : 0

  name                                           = local.app_name
  resource_group_name                            = azurerm_resource_group.this["app"].name
  location                                       = azurerm_resource_group.this["app"].location
  service_plan_id                                = azurerm_service_plan.this[0].id
  storage_account_name                           = azurerm_storage_account.this.name
  storage_uses_managed_identity                  = local.storage_key_access ? null : true
  storage_account_access_key                     = local.storage_key_access ? azurerm_storage_account.this.primary_access_key : null
  virtual_network_subnet_id                      = azurerm_subnet.app.id
  public_network_access_enabled                  = var.compute.public_deployments
  https_only                                     = true
  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false
  tags                                           = local.tags

  app_settings = merge(local.app_settings, local.storage_key_access ? {
    WEBSITE_CONTENTOVERVNET = "1"
    WEBSITE_CONTENTSHARE    = azurerm_storage_share.content[0].name
  } : {})

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  site_config {
    always_on                              = var.compute.hosting == "app_service"
    elastic_instance_minimum               = var.compute.hosting == "premium" ? max(1, var.compute.min_instances) : null
    vnet_route_all_enabled                 = true
    application_insights_connection_string = azurerm_application_insights.this.connection_string
    minimum_tls_version                    = "1.2"
    ftps_state                             = "Disabled"
    ip_restriction_default_action          = "Deny"
    scm_use_main_ip_restriction            = false
    scm_ip_restriction_default_action      = local.scm_action

    application_stack {
      python_version              = local.runtime_name == "python" ? var.compute.runtime.version : null
      node_version                = local.runtime_name == "node" ? var.compute.runtime.version : null
      dotnet_version              = local.runtime_name == "dotnet-isolated" ? var.compute.runtime.version : null
      use_dotnet_isolated_runtime = local.runtime_name == "dotnet-isolated" ? true : null
      use_custom_runtime          = local.runtime_name == "custom" ? true : null
    }
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    require_https          = true
    unauthenticated_action = "Return401"
    default_provider       = "azureactivedirectory"

    active_directory_v2 {
      client_id            = azuread_application.api.client_id
      tenant_auth_endpoint = local.entra_issuer
      allowed_audiences    = local.entra_audiences
      allowed_applications = [azurerm_user_assigned_identity.gateway.client_id]
    }

    login {
      token_store_enabled = false
    }
  }

  depends_on = [azurerm_role_assignment.api_storage, azurerm_cosmosdb_sql_role_assignment.api]
}

resource "azurerm_private_endpoint" "app" {
  count = local.functions_hosting ? 1 : 0

  name                = local.role_names.app.private_endpoint.name
  resource_group_name = azurerm_resource_group.this["app"].name
  location            = azurerm_resource_group.this["app"].location
  subnet_id           = azurerm_subnet.endpoints.id
  tags                = local.tags

  private_service_connection {
    name                           = "app"
    private_connection_resource_id = local.app_id
    subresource_names              = ["sites"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "app"
    private_dns_zone_ids = [azurerm_private_dns_zone.this["privatelink.azurewebsites.net"].id]
  }
}

resource "azurerm_container_registry" "this" {
  count = local.functions_hosting ? 0 : 1

  name                = local.names.registry.name_unique
  resource_group_name = azurerm_resource_group.this["app"].name
  location            = azurerm_resource_group.this["app"].location
  sku                 = "Basic"
  admin_enabled       = false
  tags                = local.tags
}

resource "azurerm_role_assignment" "api_registry" {
  count = local.functions_hosting ? 0 : 1

  scope                = azurerm_container_registry.this[0].id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_container_app_environment" "this" {
  count = local.functions_hosting ? 0 : 1

  name                           = local.names.managed_environment.name
  resource_group_name            = azurerm_resource_group.this["app"].name
  location                       = azurerm_resource_group.this["app"].location
  log_analytics_workspace_id     = azurerm_log_analytics_workspace.this.id
  infrastructure_subnet_id       = azurerm_subnet.app.id
  internal_load_balancer_enabled = true
  tags                           = local.tags

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }

  dynamic "workload_profile" {
    for_each = var.compute.sku == "Consumption" ? [] : [var.compute.sku]
    content {
      name                  = "dedicated"
      workload_profile_type = workload_profile.value
      minimum_count         = max(1, var.compute.min_instances)
      maximum_count         = max(1, var.compute.max_instances)
    }
  }
}

resource "azurerm_container_app" "this" {
  count = local.functions_hosting ? 0 : 1

  name                         = local.app_name
  resource_group_name          = azurerm_resource_group.this["app"].name
  container_app_environment_id = azurerm_container_app_environment.this[0].id
  revision_mode                = "Single"
  workload_profile_name        = var.compute.sku == "Consumption" ? "Consumption" : "dedicated"
  tags                         = local.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  registry {
    server   = azurerm_container_registry.this[0].login_server
    identity = azurerm_user_assigned_identity.api.id
  }

  ingress {
    external_enabled = true
    target_port      = 80
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.compute.min_instances
    max_replicas = var.compute.max_instances

    container {
      name   = "api"
      image  = "mcr.microsoft.com/k8se/quickstart:latest"
      cpu    = local.container_cpu
      memory = local.container_memory

      dynamic "env" {
        for_each = merge(local.app_settings, {
          APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.this.connection_string
        })
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    http_scale_rule {
      name                = "http"
      concurrent_requests = "100"
    }
  }

  lifecycle {
    ignore_changes = [template[0].container[0].image]
  }

  depends_on = [azurerm_role_assignment.api_registry, azurerm_role_assignment.api_storage, azurerm_cosmosdb_sql_role_assignment.api]
}

resource "azapi_resource" "container_app_auth" {
  count = local.functions_hosting ? 0 : 1

  type      = "Microsoft.App/containerApps/authConfigs@2024-03-01"
  name      = "current"
  parent_id = azurerm_container_app.this[0].id

  body = {
    properties = {
      platform = { enabled = true }
      globalValidation = {
        unauthenticatedClientAction = "Return401"
      }
      httpSettings = { requireHttps = true }
      identityProviders = {
        azureActiveDirectory = {
          enabled = true
          registration = {
            clientId     = azuread_application.api.client_id
            openIdIssuer = local.entra_issuer
          }
          validation = {
            allowedAudiences = local.entra_audiences
            defaultAuthorizationPolicy = {
              allowedApplications = [azurerm_user_assigned_identity.gateway.client_id]
            }
          }
        }
      }
    }
  }
}

resource "azurerm_private_dns_zone" "container_apps" {
  count = local.functions_hosting ? 0 : 1

  name                = azurerm_container_app_environment.this[0].default_domain
  resource_group_name = azurerm_resource_group.this["network"].name
  tags                = local.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "container_apps" {
  count = local.functions_hosting ? 0 : 1

  name                = local.names.private_dns_zone_virtual_network_link.name
  private_dns_zone_id = azurerm_private_dns_zone.container_apps[0].id
  virtual_network_id  = azurerm_virtual_network.this.id
  tags                = local.tags
}

resource "azurerm_private_dns_a_record" "container_apps" {
  count = local.functions_hosting ? 0 : 1

  name                = "*"
  private_dns_zone_id = azurerm_private_dns_zone.container_apps[0].id
  ttl                 = 300
  records             = [azurerm_container_app_environment.this[0].static_ip_address]
  tags                = local.tags
}
