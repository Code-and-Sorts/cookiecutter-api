locals {
  subnet_bits = 24 - tonumber(split("/", var.network.address_space)[1])
  subnets = {
    app       = cidrsubnet(var.network.address_space, local.subnet_bits, 1)
    gateway   = cidrsubnet(var.network.address_space, local.subnet_bits, 2)
    endpoints = cidrsubnet(var.network.address_space, local.subnet_bits, 3)
  }

  app_delegation = contains(["flex_consumption", "container_app"], var.compute.hosting) ? {
    name    = "Microsoft.App/environments"
    actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    } : {
    name    = "Microsoft.Web/serverFarms"
    actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
  }

  gateway_injected = contains(["Developer", "Premium"], var.gateway.sku)
}

resource "azurerm_virtual_network" "this" {
  name                = local.names.virtual_network.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  address_space       = [var.network.address_space]
  tags                = local.tags
}

resource "azurerm_subnet" "app" {
  name                 = local.role_names.app.virtual_network_subnet.name
  resource_group_name  = azurerm_resource_group.this.name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = [local.subnets.app]

  service_endpoint {
    service = "Microsoft.Storage"
  }

  delegation {
    name = "app"
    service_delegation {
      name    = local.app_delegation.name
      actions = local.app_delegation.actions
    }
  }
}

resource "azurerm_subnet" "gateway" {
  name                 = local.role_names.gateway.virtual_network_subnet.name
  resource_group_name  = azurerm_resource_group.this.name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = [local.subnets.gateway]

  dynamic "delegation" {
    for_each = local.gateway_injected ? [] : [1]
    content {
      name = "gateway"
      service_delegation {
        name    = "Microsoft.Web/serverFarms"
        actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
    }
  }
}

resource "azurerm_subnet" "endpoints" {
  name                 = local.role_names.endpoints.virtual_network_subnet.name
  resource_group_name  = azurerm_resource_group.this.name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = [local.subnets.endpoints]
}

resource "azurerm_network_security_group" "gateway" {
  name                = local.role_names.gateway.network_security_group.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags                = local.tags

  security_rule {
    name                       = "allow-https-inbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "Internet"
    destination_address_prefix = "VirtualNetwork"
  }

  security_rule {
    name                       = "allow-apim-management"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "3443"
    source_address_prefix      = "ApiManagement"
    destination_address_prefix = "VirtualNetwork"
  }

  security_rule {
    name                       = "allow-load-balancer"
    priority                   = 120
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "6390"
    source_address_prefix      = "AzureLoadBalancer"
    destination_address_prefix = "VirtualNetwork"
  }
}

resource "azurerm_subnet_network_security_group_association" "gateway" {
  subnet_id                 = azurerm_subnet.gateway.id
  network_security_group_id = azurerm_network_security_group.gateway.id
}

resource "azurerm_network_security_group" "default" {
  name                = local.names.network_security_group.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags                = local.tags
}

resource "azurerm_subnet_network_security_group_association" "default" {
  for_each = {
    app       = azurerm_subnet.app.id
    endpoints = azurerm_subnet.endpoints.id
  }

  subnet_id                 = each.value
  network_security_group_id = azurerm_network_security_group.default.id
}

resource "azurerm_private_dns_zone" "this" {
  for_each = toset(concat(
    ["privatelink.documents.azure.com"],
    local.functions_hosting ? ["privatelink.azurewebsites.net"] : [],
  ))

  name                = each.value
  resource_group_name = azurerm_resource_group.this.name
  tags                = local.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "this" {
  for_each = azurerm_private_dns_zone.this

  name                = local.names.private_dns_zone_virtual_network_link.name
  private_dns_zone_id = each.value.id
  virtual_network_id  = azurerm_virtual_network.this.id
  tags                = local.tags
}
