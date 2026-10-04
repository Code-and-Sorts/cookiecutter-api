resource "azurerm_cosmosdb_account" "this" {
  name                               = "cosmos-${local.short}-${local.suffix}"
  resource_group_name                = azurerm_resource_group.this.name
  location                           = azurerm_resource_group.this.location
  offer_type                         = "Standard"
  kind                               = "GlobalDocumentDB"
  free_tier_enabled                  = var.database.free_tier
  minimal_tls_version                = "Tls12"
  access_key_metadata_writes_enabled = false
  local_authentication_enabled       = false
  public_network_access_enabled      = false
  tags                               = local.tags

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = azurerm_resource_group.this.location
    failover_priority = 0
  }

  dynamic "capabilities" {
    for_each = var.database.capacity == "serverless" ? ["EnableServerless"] : []
    content {
      name = capabilities.value
    }
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_cosmosdb_sql_database" "this" {
  name                = var.name
  resource_group_name = azurerm_resource_group.this.name
  account_name        = azurerm_cosmosdb_account.this.name
  throughput          = var.database.capacity == "provisioned" ? var.database.throughput : null

  dynamic "autoscale_settings" {
    for_each = var.database.capacity == "autoscale" ? [var.database.throughput] : []
    content {
      max_throughput = autoscale_settings.value
    }
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_cosmosdb_sql_container" "this" {
  for_each = toset(var.database.containers)

  name                  = each.value
  resource_group_name   = azurerm_resource_group.this.name
  account_name          = azurerm_cosmosdb_account.this.name
  database_name         = azurerm_cosmosdb_sql_database.this.name
  partition_key_paths   = ["/id"]
  partition_key_version = 2

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_management_lock" "database" {
  count = var.database.delete_lock ? 1 : 0

  name       = "cosmos-delete-lock"
  scope      = azurerm_cosmosdb_account.this.id
  lock_level = "CanNotDelete"
  notes      = "Holds the API's data; set database.delete_lock to false to remove."
}

resource "azurerm_cosmosdb_sql_role_assignment" "api" {
  resource_group_name = azurerm_resource_group.this.name
  account_name        = azurerm_cosmosdb_account.this.name
  role_definition_id  = "${azurerm_cosmosdb_account.this.id}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002"
  principal_id        = azurerm_user_assigned_identity.api.principal_id
  scope               = azurerm_cosmosdb_account.this.id
}

resource "azurerm_private_endpoint" "database" {
  name                = "pe-${local.prefix}-cosmos"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  subnet_id           = azurerm_subnet.endpoints.id
  tags                = local.tags

  private_service_connection {
    name                           = "cosmos"
    private_connection_resource_id = azurerm_cosmosdb_account.this.id
    subresource_names              = ["Sql"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "cosmos"
    private_dns_zone_ids = [azurerm_private_dns_zone.this["privatelink.documents.azure.com"].id]
  }
}
