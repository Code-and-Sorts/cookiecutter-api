mock_data "azuread_client_config" {
  defaults = {
    object_id = "33333333-3333-3333-3333-333333333333"
    tenant_id = "11111111-1111-1111-1111-111111111111"
  }
}

mock_resource "azuread_application" {
  defaults = {
    id        = "/applications/44444444-4444-4444-4444-444444444444"
    client_id = "55555555-5555-5555-5555-555555555555"
  }
}
