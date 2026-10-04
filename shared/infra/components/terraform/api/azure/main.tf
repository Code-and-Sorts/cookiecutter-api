data "azurerm_client_config" "current" {}

locals {
  prefix  = "${var.name}-${var.stage}"
  suffix  = substr(sha1("${data.azurerm_client_config.current.subscription_id}/${local.prefix}"), 0, 6)
  short   = replace(substr(local.prefix, 0, 24), "/-+$/", "")
  compact = "${substr(replace(var.name, "-", ""), 0, 8)}${var.stage}${local.suffix}"

  tags = merge({ project = var.name, stage = var.stage, managed-by = "terraform" }, var.tags)

  functions_hosting = var.compute.hosting != "container_app"
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.prefix}"
  location = var.region
  tags     = local.tags
}

resource "azurerm_log_analytics_workspace" "this" {
  name                = "log-${local.prefix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}

resource "azurerm_application_insights" "this" {
  name                = "appi-${local.prefix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  workspace_id        = azurerm_log_analytics_workspace.this.id
  application_type    = "web"
  tags                = local.tags
}
