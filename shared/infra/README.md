{%- set stacks = path_environments | map(attribute='name') | list -%}
{%- set state_name = (project_endpoint | replace('-', '') | replace('_', ''))[:16] -%}
# Infrastructure

[Atmos](https://atmos.tools/) stacks and a Terraform component that deploy {{ project_name }} to Azure.
Every stack (environment) gets the same resources; only its region and tiers differ.

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
- Log Analytics and Application Insights collect the API's and API Management's telemetry.

## Layout

```
infra/
├── atmos.yaml                     # run atmos from infra/
├── stacks/
│   ├── catalog/defaults.yaml      # Terraform state backend and shared tags
│   ├── catalog/api.yaml           # the api component: runtime, app settings, containers, routes
│   └── deploy/<stage>.yaml        # one stack per environment: region and tiers
└── components/terraform/api/      # the Terraform component, with its tests in tests/
```

## Stacks

| Stack | Region | Hosting | SKU | Cosmos DB | API Management |
|---|---|---|---|---|---|
{%- for e in path_environments %}
| `{{ e.name }}` | {{ e.region }} | {{ e.compute_hosting }} | {{ e.compute_sku }} | {{ e.database_capacity }}{% if e.database_capacity != 'serverless' %} ({{ e.database_throughput }} RU/s{% if e.database_capacity == 'autoscale' %} max{% endif %}){% endif %} | {{ e.gateway_sku }} ×{{ e.gateway_capacity }} |
{%- endfor %}

Change a stack's tiers in `stacks/deploy/<stage>.yaml`, or add a stack by copying one. The `api` component's
variables accept:

- `compute.hosting`: `flex_consumption` (`sku: FC1`), `app_service` (a Linux SKU such as `B1`, `S1`, `P0v3`,
  `P1v3`), `premium` (`EP1`-`EP3`) or `container_app` (`Consumption`, or a dedicated workload profile such as `D4`
  or `E4`); `min_instances`, `max_instances` and `instance_memory_mb` (Flex Consumption) tune scaling.
- `database.capacity`: `serverless`, `provisioned` (`throughput` from 400 RU/s, in steps of 100) or `autoscale`
  (`throughput` is the maximum, from 1000 in steps of 1000), shared by the database's containers; `free_tier`
  and `delete_lock` (a CanNotDelete lock on the account, on by default for a stack named `prod`; managing locks
  needs Owner or User Access Administrator).
- `gateway.sku`: `Developer` (no SLA, one unit), `StandardV2` or `Premium`, with `capacity` units. These tiers can
  reach a private backend; Consumption, Basic, Standard and Basic v2 cannot.
- `network.address_space` (default `10.20.0.0/16`, at least a `/22`).

## One-time setup

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

`.github/workflows/deploy.yml` then plans `{{ stacks[0] }}` on pull requests, and on `main` deploys
{% for s in stacks %}`{{ s }}`{{ ' then ' if not loop.last }}{% endfor %}: it applies the stack, publishes the code (or builds and
pushes the container image), {% if health_endpoint %}and calls the health check{% else %}and ends{% endif %}. The first apply of a stack takes about an hour,
mostly API Management joining the virtual network.

## Run it yourself

With [Atmos](https://atmos.tools/install/) and Terraform installed and `az login` done (Terraform reads
`ARM_SUBSCRIPTION_ID`):

```console
cd infra
ARM_SUBSCRIPTION_ID=<subscription-id> atmos terraform plan api -s {{ stacks[0] }}
ARM_SUBSCRIPTION_ID=<subscription-id> atmos terraform deploy api -s {{ stacks[0] }}
```

Read the API URL and key:

```console
cd infra/components/terraform/api
terraform output -raw api_url
terraform output -raw api_key
```

```console
curl -H "x-api-key: <key>" "<api_url>/{{ resources[0].endpoint }}"
```

The function app keeps its deployment (SCM) endpoint public so GitHub-hosted runners can publish to it; it accepts
only Entra ID, never basic credentials. With runners inside the virtual network, set `compute.public_deployments`
to `false` to close it too.

## Tests

The component's tests plan against mocked providers, so they need no Azure account:

```console
cd infra/components/terraform/api
terraform init -backend=false
terraform test
```

`tests/stacks` plans one stack's real variables the same way:

```console
cd infra
atmos terraform generate varfile api -s {{ stacks[0] }} -f /tmp/{{ stacks[0] }}.tfvars.json
cd components/terraform/api
terraform test -test-directory=tests/stacks -var-file=/tmp/{{ stacks[0] }}.tfvars.json
```

## Destroy

Turn off `delete_lock` and apply first if it is on, then:

```console
cd infra
atmos terraform destroy api -s <stage>
```
