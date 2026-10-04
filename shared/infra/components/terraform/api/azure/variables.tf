variable "name" {
  type        = string
  description = "Project name in kebab-case; every resource name starts with it."

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]*$", var.name))
    error_message = "name must be lowercase kebab-case."
  }
}

variable "stage" {
  type        = string
  description = "Environment (stack) name, such as dev or prod."

  validation {
    condition     = can(regex("^[a-z][a-z0-9]{0,7}$", var.stage))
    error_message = "stage must be at most 8 lowercase letters and digits."
  }
}

variable "region" {
  type        = string
  description = "Azure region for every resource."
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "Tags added to every resource."
}

variable "network" {
  type = object({
    address_space = optional(string, "10.20.0.0/16")
  })
  default     = {}
  description = "Virtual network; the app, gateway and private endpoint subnets are /24s taken from it."

  validation {
    condition     = can(cidrhost(var.network.address_space, 0)) && tonumber(split("/", var.network.address_space)[1]) <= 22
    error_message = "network.address_space must be a CIDR block of /22 or larger."
  }
}

variable "compute" {
  type = object({
    hosting            = string
    sku                = string
    runtime            = object({ name = string, version = string })
    min_instances      = optional(number, 0)
    max_instances      = optional(number, 100)
    instance_memory_mb = optional(number, 2048)
    # Setting name to value; ${database_endpoint} and ${database_name} are filled in.
    app_settings = optional(map(string), {})
    # Lets CI deploy over the public SCM endpoint, which still requires Entra ID; the API itself stays private.
    public_deployments = optional(bool, true)
  })
  description = "How the API runs: flex_consumption, app_service, premium or container_app, and its plan SKU or workload profile."

  validation {
    condition     = contains(["flex_consumption", "app_service", "premium", "container_app"], var.compute.hosting)
    error_message = "compute.hosting must be flex_consumption, app_service, premium or container_app."
  }

  validation {
    condition = (
      var.compute.hosting == "flex_consumption" ? var.compute.sku == "FC1" :
      var.compute.hosting == "premium" ? contains(["EP1", "EP2", "EP3"], var.compute.sku) :
      var.compute.hosting == "app_service" ? can(regex("^(B[1-3]|S[1-3]|P[0-3]v3|P[1-5]mv3|P[0-5]v4|P[1-5]mv4)$", var.compute.sku)) :
      can(regex("^(Consumption|D(4|8|16|32)|E(4|8|16|32)|NC(24|48|96)-A100)$", var.compute.sku))
    )
    error_message = "compute.sku does not fit compute.hosting: FC1 for flex_consumption, EP1-EP3 for premium, a Linux App Service SKU for app_service, a workload profile for container_app."
  }
}

variable "database" {
  type = object({
    capacity   = string
    throughput = optional(number, 400)
    containers = list(string)
    free_tier  = optional(bool, false)
    # A CanNotDelete lock on the account; set it to false and apply before destroying the stack.
    delete_lock = optional(bool, false)
  })
  description = "Cosmos DB capacity (serverless, provisioned or autoscale), shared database throughput and one container per id."

  validation {
    condition     = contains(["serverless", "provisioned", "autoscale"], var.database.capacity)
    error_message = "database.capacity must be serverless, provisioned or autoscale."
  }

  validation {
    condition = (
      var.database.capacity == "serverless" ||
      (var.database.capacity == "provisioned" && var.database.throughput >= 400 && var.database.throughput % 100 == 0) ||
      (var.database.capacity == "autoscale" && var.database.throughput >= 1000 && var.database.throughput % 1000 == 0)
    )
    error_message = "database.throughput must be at least 400 RU/s in steps of 100 (provisioned) or 1000 in steps of 1000 (autoscale)."
  }
}

variable "gateway" {
  type = object({
    sku             = string
    capacity        = optional(number, 1)
    publisher_name  = string
    publisher_email = string
    # Public path prefix; the Functions host serves the API under /api too.
    path            = optional(string, "api")
    health_endpoint = optional(string, "")
    routes = list(object({
      name       = string
      endpoint   = string
      operations = list(string)
    }))
  })
  description = "API Management tier and the routes it publishes; routes need an API key, the health check does not."

  validation {
    condition     = contains(["Developer", "StandardV2", "Premium"], var.gateway.sku)
    error_message = "gateway.sku must be Developer, StandardV2 or Premium: the tiers that can reach a private backend."
  }

  validation {
    condition     = var.gateway.sku != "Developer" || var.gateway.capacity == 1
    error_message = "The Developer tier has exactly 1 unit."
  }
}
