locals {
  diagnostic_logs = merge(
    local.functions_hosting ? { app = { id = local.app_id, categories = ["FunctionAppLogs"] } } : {},
    {
      gateway  = { id = azurerm_api_management.this.id, categories = ["GatewayLogs"] }
      database = { id = azurerm_cosmosdb_account.this.id, categories = ["DataPlaneRequests", "ControlPlaneRequests"] }
    },
  )
}

resource "azurerm_log_analytics_workspace" "this" {
  name                = local.names.operational_insights_workspace.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}

resource "azurerm_application_insights" "this" {
  name                = local.names.component.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  workspace_id        = azurerm_log_analytics_workspace.this.id
  application_type    = "web"
  tags                = local.tags
}

resource "azurerm_monitor_diagnostic_setting" "logs" {
  for_each = local.diagnostic_logs

  name                       = local.names.monitor_diagnostic_setting.name
  target_resource_id         = each.value.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id

  dynamic "enabled_log" {
    for_each = each.value.categories
    content {
      category = enabled_log.value
    }
  }
}
