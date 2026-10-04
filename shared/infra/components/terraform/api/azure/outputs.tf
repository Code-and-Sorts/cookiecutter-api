output "resource_group_name" {
  value       = azurerm_resource_group.this.name
  description = "Resource group holding every resource of the stack."
}

output "api_url" {
  value       = "${azurerm_api_management.this.gateway_url}/${var.gateway.path}"
  description = "Public base URL of the API (API Management)."
}

output "api_key" {
  value       = azurerm_api_management_subscription.api.primary_key
  sensitive   = true
  description = "API key, sent as the x-api-key header."
}

output "app_name" {
  value       = local.app_name
  description = "Function app or container app the deploy workflow publishes to."
}

output "app_hosting" {
  value       = var.compute.hosting
  description = "Compute hosting, which picks the deploy method."
}

output "container_registry" {
  value       = one(azurerm_container_registry.this[*].login_server)
  description = "Registry for the API image (container_app hosting only)."
}

output "database_endpoint" {
  value       = azurerm_cosmosdb_account.this.endpoint
  description = "Cosmos DB endpoint; reachable only through the private endpoint."
}

output "entra_client_id" {
  value       = azuread_application.api.client_id
  description = "Client ID of the Entra ID application API Management authenticates to."
}
