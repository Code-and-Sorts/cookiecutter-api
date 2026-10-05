{%- set state_name = (project_endpoint | replace('-', '') | replace('_', ''))[:16] -%}
## Azure

```
client ──x-api-key──▶ API Management ──Entra ID token──▶ API (private) ──managed identity──▶ Cosmos DB (private)
                      (virtual network)                   │
                                                          └──▶ storage account (virtual network only)
```

- **API Management** is the only public entry point. Resource routes need an API key in the `x-api-key` header
  (or the `api-key` query parameter){% if health_endpoint %}; `GET /api/{{ health_endpoint }}` is open{% endif %}. It calls the API with an
  Entra ID token for its own managed identity.
- **The API** runs on Azure Functions (Flex Consumption, an App Service plan or Premium) or on Azure Container Apps.
  It has no public endpoint: Functions answer only on a private endpoint, Container Apps run in an internal
  environment. App Service authentication rejects any request without a token for the API's Entra ID application
  from API Management's identity (401), so function keys are not used.
- **Cosmos DB** has public access and keys disabled; the API's managed identity reaches it through a private
  endpoint with the Cosmos DB Built-in Data Contributor role. One container per resource `container`, partitioned
  by `/id`.
- **Storage** for the Functions host admits only the API's subnet; the host uses its managed identity (Premium
  keeps the account key, because its Azure Files content share needs it).
- **Logs** (`logs.tf`): one Log Analytics workspace holds everything. Application Insights, which the API and API
  Management report to, keeps its data there, and diagnostic settings send the function app's, API Management's and
  Cosmos DB's platform logs to it (Container Apps log through their environment).
- **Resource groups:** one per part of the stack, so each has its own lifecycle and access:

  | Resource group | Holds |
  |---|---|
  | `rg-{{ project_endpoint }}-<stage>-network` | Virtual network, subnets, NSGs and private DNS zones |
  | `rg-{{ project_endpoint }}-<stage>-monitoring` | Log Analytics and Application Insights |
  | `rg-{{ project_endpoint }}-<stage>-data` | Cosmos DB, its private endpoint and its delete lock |
  | `rg-{{ project_endpoint }}-<stage>-app` | The API (plan, function app or container app), its storage, registry, private endpoint and identity |
  | `rg-{{ project_endpoint }}-<stage>-gateway` | API Management and its identity |

- Resource names come from the [Azure Verified Modules naming utility](https://github.com/Azure/terraform-azure-avm-utl-naming):
  the Cloud Adoption Framework abbreviation, the project and the stage (`rg-{{ project_endpoint }}-<stage>`), plus
  four characters derived from the subscription for names that must be globally unique.

### Tiers

- `compute.hosting`: `flex_consumption` (`sku: FC1`), `app_service` (a Linux SKU such as `B1`, `S1`, `P0v3`,
  `P1v3`), `premium` (`EP1`-`EP3`) or `container_app` (`Consumption`, or a dedicated workload profile such as `D4`
  or `E4`); `min_instances`, `max_instances` and `instance_memory_mb` (Flex Consumption) tune scaling, and `cpu`
  sets a container app's vCPUs (0.25 to 4 in steps of 0.25, default 0.5; memory is twice that in GiB).
- `database.capacity`: `serverless`, `provisioned` (`throughput` from 400 RU/s, in steps of 100) or `autoscale`
  (`throughput` is the maximum, from 1000 in steps of 1000), shared by the database's containers; `free_tier`
  and `delete_lock` (a CanNotDelete lock that stops deletes from outside Terraform, on by default for a stack
  named `prod`; managing locks needs Owner or User Access Administrator).
- `gateway.sku`: `Developer` (no SLA, one unit), `StandardV2` or `Premium`, with `capacity` units. These tiers can
  reach a private backend; Consumption, Basic, Standard and Basic v2 cannot.
- `network.address_space` (default `10.20.0.0/16`, at least a `/22`).

### One-time setup

1. Create the Terraform state store named in `stacks/catalog/defaults.yaml` (rename the storage account there if
   the name is taken; names are global):

    ```console
    az group create --name rg-{{ project_endpoint }}-tfstate --location {{ infra_region }}
    az storage account create --name st{{ state_name }}state --resource-group rg-{{ project_endpoint }}-tfstate \
      --sku Standard_LRS --min-tls-version TLS1_2 --allow-blob-public-access false --allow-shared-key-access false
    az storage container create --name tfstate --account-name st{{ state_name }}state --auth-mode login
    ```

2. Create an Entra ID application (or user-assigned identity) for deployments, and for each stack a federated
   credential with the subject `repo:<owner>/<repo>:environment:<stage>`. Give it, on the subscription,
   **Contributor** and **Role Based Access Control Administrator** (the stack assigns roles to its identities),
   **Storage Blob Data Contributor** on the state storage account, and the Microsoft Graph application permission
   **Application.ReadWrite.OwnedBy** (the stack creates the API's Entra ID application).

3. In GitHub, create an environment per stack ({% for s in stacks %}`{{ s }}`{{ ', ' if not loop.last }}{% endfor %}) with the variables
   `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID`. Add required reviewers to the stacks that
   need an approval.

### Deployments

The function app keeps its deployment (SCM) endpoint public so GitHub-hosted runners can publish to it; it accepts
only Entra ID, never basic credentials. With runners inside the virtual network, set `compute.public_deployments`
to `false` to close it too.

### Security scan exceptions

`components/terraform/api/.checkov.yaml` skips these Checkov checks on purpose:

| Check | Why |
|---|---|
| `CKV_AZURE_225`, `CKV_AZURE_212`, `CKV_AZURE_206` | Single-zone, single-instance and LRS defaults keep small stacks cheap; raise them per stack for production. |
| `CKV_AZURE_100`, `CKV2_AZURE_1` | Platform-managed keys encrypt at rest; customer-managed keys need a Key Vault the stack does not create. |
| `CKV_AZURE_174` | API Management is the API's only public entry point. |
| `CKV2_AZURE_33`, `CKV_AZURE_59` | The storage account admits only the API subnet, through a service endpoint. |
| `CKV_AZURE_33`, `CKV2_AZURE_21` | Diagnostic logging for the Functions host's storage is left to the team. |
| `CKV_AZURE_140`, `CKV2_AZURE_40`, `CKV2_AZURE_41` | Checkov reads azurerm 4 attribute names; local authentication, shared keys and the SAS expiry are set with their azurerm 5 names. |
| `CKV_TF_1` | The naming module comes from the Terraform Registry, pinned to an exact version; registry modules have no commit hash. |
