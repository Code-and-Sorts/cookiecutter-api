data "azuread_client_config" "current" {}

resource "azurerm_user_assigned_identity" "api" {
  name                = local.role_names.app.user_assigned_identity.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "gateway" {
  name                = local.role_names.gateway.user_assigned_identity.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags                = local.tags
}

resource "azuread_application" "api" {
  display_name     = "${local.prefix}-api"
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADMyOrg"

  api {
    requested_access_token_version = 2
  }
}

resource "azuread_application_identifier_uri" "api" {
  application_id = azuread_application.api.id
  identifier_uri = "api://${azuread_application.api.client_id}"
}

resource "azuread_service_principal" "api" {
  client_id = azuread_application.api.client_id
  owners    = [data.azuread_client_config.current.object_id]
}

locals {
  entra_audiences = [azuread_application.api.client_id, "api://${azuread_application.api.client_id}"]
  entra_issuer    = "https://login.microsoftonline.com/${data.azuread_client_config.current.tenant_id}/v2.0"
}
