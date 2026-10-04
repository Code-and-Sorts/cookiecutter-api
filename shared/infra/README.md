{%- set stacks = path_environments | map(attribute='name') | list -%}
# Infrastructure

[Atmos](https://atmos.tools/) stacks and a Terraform component that deploy {{ project_name }} to {{ infra_cloud.name }}.
Every stack (environment) gets the same resources; only its region and tiers differ. API keys go in the
`x-api-key` header{% if health_endpoint %}; the health check is open{% endif %}.

## Layout

```
infra/
├── atmos.yaml
├── stacks/
│   ├── catalog/defaults.yaml      # Terraform state backend and shared tags
│   ├── catalog/api.yaml           # the api component: runtime, app settings, containers, routes
│   └── deploy/<stage>.yaml        # one stack per environment: region and tiers
└── components/terraform/api/      # the Terraform component, with its tests in tests/
```

Run Atmos from `infra/`. The `api` component takes the same variables on every cloud: `name`, `stage`, `region`,
`owner`, `network`, `compute` (`hosting`, `sku`, `runtime`, scaling, `app_settings`), `database` (`capacity`,
`throughput`, `containers`, `delete_lock`) and `gateway` (`sku`, `capacity`, `routes`, `health_endpoint`). Only their
values differ per cloud.

## Stacks

| Stack | Region | Hosting | SKU | Database | Gateway |
|---|---|---|---|---|---|
{%- for e in path_environments %}
| `{{ e.name }}` | {{ e.region }} | {{ e.compute_hosting }} | {{ e.compute_sku }} | {{ e.database_capacity }}{% if e.database_throughput %} ({{ e.database_throughput }}){% endif %} | {{ e.gateway_sku }} ×{{ e.gateway_capacity }} |
{%- endfor %}

Change a stack's tiers in `stacks/deploy/<stage>.yaml`, or add a stack by copying one. `database.delete_lock`
also stops deletes from outside Terraform, such as the portal, and is on by default for a stack named `prod`.

`.github/workflows/deploy.yml` plans `{{ stacks[0] }}` on pull requests, and on `main` deploys
{% for s in stacks %}`{{ s }}`{{ ' then ' if not loop.last }}{% endfor %}: it applies the stack, publishes the code (or builds and
pushes the container image){% if health_endpoint %} and calls the health check{% endif %}. Each stack is a GitHub environment
holding the deployment identity's variables (see the one-time setup below); add required reviewers to the stacks
that need an approval.

## Run it yourself

With [Atmos](https://atmos.tools/install/) and Terraform installed and signed in to {{ infra_cloud.name }}:

```console
cd infra
{%- if cloud_service == 'Azure Function App' %}
export ARM_SUBSCRIPTION_ID=<subscription-id>
{%- endif %}
atmos terraform plan api -s {{ stacks[0] }}
atmos terraform deploy api -s {{ stacks[0] }}
```

Read the API URL and key, then call it:

```console
cd infra/components/terraform/api
terraform output -raw api_url
terraform output -raw api_key
curl -H "x-api-key: <key>" "<api_url>/{{ resources[0].endpoint }}"
```

## Tests

The component's tests plan against mocked providers, so they need no cloud account:

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

The database, its containers and the storage account carry `lifecycle { prevent_destroy = true }`, so Terraform
refuses any plan that would delete or replace them, including one that drops a container no resource uses any
more. To tear a stack down on purpose, delete those `lifecycle` blocks in `components/terraform/api` in a local
checkout, never in a commit, then:

```console
cd infra
atmos terraform destroy api -s <stage>
```
{%- if cloud_service == 'Azure Function App' %}
{%- set cloud_section %}{% include 'shared/infra/_README.azure.md' %}{% endset %}

{{ cloud_section.strip() }}
{%- endif %}
