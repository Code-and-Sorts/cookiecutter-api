{%- set stacks = path_environments | map(attribute='name') | list -%}
## Deploy with the infrastructure

`infra/` deploys the API to {{ infra_cloud.name }} with [Atmos](https://atmos.tools/) and Terraform, one stack per environment
({% for s in stacks %}`{{ s }}`{{ ', ' if not loop.last }}{% endfor %}). The API gateway is the only public entry point: resource routes need its API key in the
`x-api-key` header{% if health_endpoint %} and the health check is open{% endif %}.
{%- if cloud_service == 'Azure Function App' %} The API itself is private, and API Management
calls it with an Entra ID token, so the functions use the `anonymous` auth level behind App Service
authentication. Cosmos DB is private and keyless: the app leaves the key setting empty and authenticates with its
managed identity (`AZURE_CLIENT_ID`).
{%- endif %}

`.github/workflows/deploy.yml` plans `{{ stacks[0] }}` on pull requests and, on `main`, deploys each stack in turn and
publishes the code. [infra/README.md](infra/README.md) covers the one-time setup, each stack's tiers, running
Atmos yourself and the Terraform tests.
