data "azurerm_client_config" "current" {}

locals {
  prefix     = "${var.name}-${var.stage}"
  words      = concat(split("-", var.name), [var.stage])
  names      = module.naming.names
  role_names = { for role, naming in module.role_naming : role => naming.names }

  tags = merge({ project = var.name, stage = var.stage, managed-by = "terraform" }, var.tags)

  functions_hosting = var.compute.hosting != "container_app"
}

module "naming" {
  source  = "Azure/avm-utl-naming/azure"
  version = "0.2.0"

  suffix      = local.words
  unique_seed = sha1("${data.azurerm_client_config.current.subscription_id}/${local.prefix}")
}

module "role_naming" {
  source   = "Azure/avm-utl-naming/azure"
  version  = "0.2.0"
  for_each = toset(["app", "cosmos", "endpoints", "gateway"])

  suffix        = concat(local.words, [each.key])
  unique_length = 0
}

resource "azurerm_resource_group" "this" {
  name     = local.names.resource_group.name
  location = var.region
  tags     = local.tags
}
