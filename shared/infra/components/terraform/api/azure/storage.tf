locals {
  storage_key_access = var.compute.hosting == "premium"
}

resource "azurerm_storage_account" "this" {
  name                            = local.names.storage_account.name_unique
  resource_group_name             = azurerm_resource_group.this["app"].name
  location                        = azurerm_resource_group.this["app"].location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = local.storage_key_access
  default_to_oauth_authentication = true
  tags                            = local.tags

  blob_properties {
    delete_retention_policy {
      days = 7
    }
    container_delete_retention_policy {
      days = 7
    }
  }

  sas_policy {
    expiration_period = "01.00:00:00"
  }

  network_rules {
    default_action             = "Deny"
    bypass                     = ["AzureServices"]
    virtual_network_subnet_ids = [azurerm_subnet.app.id]
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_container" "deployments" {
  count = var.compute.hosting == "flex_consumption" ? 1 : 0

  name                  = "deployments"
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = "private"
}

resource "azurerm_storage_share" "content" {
  count = var.compute.hosting == "premium" ? 1 : 0

  name               = "content"
  storage_account_id = azurerm_storage_account.this.id
  quota              = 50
}

resource "azurerm_role_assignment" "api_storage" {
  for_each = toset(["Storage Blob Data Owner", "Storage Queue Data Contributor", "Storage Table Data Contributor"])

  scope                = azurerm_storage_account.this.id
  role_definition_name = each.value
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  principal_type       = "ServicePrincipal"
}
