terraform {
  required_version = ">= 1.9"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.8"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.10"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.13"
    }
  }
}

# Credentials and the subscription come from the environment (ARM_* variables, OIDC in CI).
provider "azurerm" {
  features {}
  # Shared keys stay off wherever the hosting allows it.
  storage_use_azuread = true
}

provider "azuread" {}

provider "azapi" {}
