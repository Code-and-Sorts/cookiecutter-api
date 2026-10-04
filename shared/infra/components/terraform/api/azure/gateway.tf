locals {
  operations = {
    list      = { method = "GET", item = false }
    get_by_id = { method = "GET", item = true }
    create    = { method = "POST", item = false }
    update    = { method = "PATCH", item = true }
    replace   = { method = "PUT", item = true }
    delete    = { method = "DELETE", item = true }
  }

  routes = merge([
    for route in var.gateway.routes : {
      for op in route.operations : "${route.endpoint}-${op}" => {
        display_name = "${route.name} ${op}"
        method       = local.operations[op].method
        url_template = local.operations[op].item ? "/${route.endpoint}/{id}" : "/${route.endpoint}"
        item         = local.operations[op].item
      }
    }
  ]...)

  backend_policy = <<-XML
    <policies>
      <inbound>
        <base />%s
        <authentication-managed-identity resource="${azuread_application.api.client_id}" client-id="${azurerm_user_assigned_identity.gateway.client_id}" />
        <set-header name="x-api-key" exists-action="delete" />
      </inbound>
      <backend>
        <base />
      </backend>
      <outbound>
        <base />
      </outbound>
      <on-error>
        <base />
      </on-error>
    </policies>
  XML
}

resource "azurerm_api_management" "this" {
  name                 = local.names.api_management_service.name_unique
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  publisher_name       = var.owner.name
  publisher_email      = var.owner.email
  sku_name             = "${var.gateway.sku}_${var.gateway.capacity}"
  virtual_network_type = "External"
  tags                 = local.tags

  virtual_network_configuration {
    subnet_id = azurerm_subnet.gateway.id
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.gateway.id]
  }

  depends_on = [azurerm_subnet_network_security_group_association.gateway]
}

resource "azurerm_api_management_logger" "this" {
  name                = "appi"
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
  resource_id         = azurerm_application_insights.this.id

  application_insights {
    connection_string = azurerm_application_insights.this.connection_string
  }
}

resource "azurerm_api_management_api" "api" {
  name                  = var.name
  resource_group_name   = azurerm_resource_group.this.name
  api_management_name   = azurerm_api_management.this.name
  revision              = "1"
  display_name          = var.name
  path                  = var.gateway.path
  protocols             = ["https"]
  service_url           = "https://${local.app_hostname}/api"
  subscription_required = true

  subscription_key_parameter_names {
    header = "x-api-key"
    query  = "api-key"
  }
}

resource "azurerm_api_management_api_operation" "api" {
  for_each = local.routes

  operation_id        = each.key
  api_name            = azurerm_api_management_api.api.name
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
  display_name        = each.value.display_name
  method              = each.value.method
  url_template        = each.value.url_template

  dynamic "template_parameter" {
    for_each = each.value.item ? ["id"] : []
    content {
      name     = template_parameter.value
      type     = "string"
      required = true
    }
  }
}

resource "azurerm_api_management_api_policy" "api" {
  api_name            = azurerm_api_management_api.api.name
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
  xml_content         = format(local.backend_policy, "")
}

resource "azurerm_api_management_api_diagnostic" "api" {
  identifier               = "applicationinsights"
  api_name                 = azurerm_api_management_api.api.name
  api_management_name      = azurerm_api_management.this.name
  resource_group_name      = azurerm_resource_group.this.name
  api_management_logger_id = azurerm_api_management_logger.this.id
  sampling_percentage      = 100
  always_log_errors        = true
  verbosity                = "information"
}

resource "azurerm_api_management_api" "health" {
  count = var.gateway.health_endpoint == "" ? 0 : 1

  name                  = "${var.name}-health"
  resource_group_name   = azurerm_resource_group.this.name
  api_management_name   = azurerm_api_management.this.name
  revision              = "1"
  display_name          = "${var.name} health"
  path                  = "${var.gateway.path}/${var.gateway.health_endpoint}"
  protocols             = ["https"]
  service_url           = "https://${local.app_hostname}/api"
  subscription_required = false
}

resource "azurerm_api_management_api_operation" "health" {
  count = var.gateway.health_endpoint == "" ? 0 : 1

  operation_id        = "health"
  api_name            = azurerm_api_management_api.health[0].name
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
  display_name        = "Health check"
  method              = "GET"
  url_template        = "/"
}

resource "azurerm_api_management_api_policy" "health" {
  count = var.gateway.health_endpoint == "" ? 0 : 1

  api_name            = azurerm_api_management_api.health[0].name
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
  xml_content         = format(local.backend_policy, "\n    <rewrite-uri template=\"/${var.gateway.health_endpoint}\" />")
}

resource "azurerm_api_management_product" "api" {
  product_id            = var.name
  resource_group_name   = azurerm_resource_group.this.name
  api_management_name   = azurerm_api_management.this.name
  display_name          = var.name
  subscription_required = true
  approval_required     = false
  published             = true
}

resource "azurerm_api_management_product_api" "api" {
  product_id          = azurerm_api_management_product.api.product_id
  api_name            = azurerm_api_management_api.api.name
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
}

resource "azurerm_api_management_subscription" "api" {
  display_name        = "${local.prefix} default"
  api_management_name = azurerm_api_management.this.name
  resource_group_name = azurerm_resource_group.this.name
  product_id          = azurerm_api_management_product.api.id
  state               = "active"
  allow_tracing       = false
}
