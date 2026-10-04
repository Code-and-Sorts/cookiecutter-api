plugin "terraform" {
  enabled = true
  preset  = "recommended"
}

plugin "azurerm" {
  enabled = true
  version = "0.32.0"
  source  = "github.com/terraform-linters/tflint-ruleset-azurerm"
}

# Stacks must be destroyable; production data is protected by database.delete_lock instead.
rule "azurerm_resources_missing_prevent_destroy" {
  enabled = false
}
