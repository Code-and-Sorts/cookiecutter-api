{#- "Run locally against the emulator" section of every README. The including README sets `emulator`
    (tool, core_tools, run_note, secret_files, cosmos_key, cosmos_flag, cosmos_https,
    missing_container, firestore_client, sdk_note) and `emulator_settings`. -#}
{%- set cmd = {
    'make': {'install': 'make install', 'up': 'make emulator-up', 'seed': 'make emulator-seed', 'run': 'make run-emulator', 'container': 'make run-container', 'down': 'make emulator-down', 'logs': 'make emulator-logs',
             'podman': 'for Podman, add `COMPOSE="podman compose"` to each `make` command'},
    'yarn': {'install': 'yarn install', 'up': 'yarn emulator:up', 'seed': 'yarn emulator:seed', 'run': 'yarn start:emulator', 'container': 'yarn start:container', 'down': 'yarn emulator:down', 'logs': 'yarn emulator:logs',
             'podman': 'with Podman, run the `podman compose` equivalents of the `emulator:*` scripts'},
}[emulator.tool] -%}
## Run locally against the emulator

`docker-compose.yml` runs {% if cloud_service == 'Azure Function App' %}the [Azure Cosmos DB emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux){% elif cloud_service == 'GCP Cloud Function' %}the [Firestore emulator](https://cloud.google.com/firestore/docs/emulator){% else %}[DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html){% endif %} in Docker, so the API can serve requests without a cloud account. You need Docker with Compose v2{% if cloud_service == 'Azure Function App' and emulator.core_tools %} and Azure Functions Core Tools{% elif cloud_service == 'AWS Lambda' %} and the SAM CLI{% endif %}; {{ cmd.podman }}.

```console
{{ cmd.install }}
{{ "%-22s" | format(cmd.up) }}# docker compose up -d --wait: returns once the emulator is healthy
{{ "%-22s" | format(cmd.seed) }}# {% if cloud_service == 'Azure Function App' %}creates the database and one container per storage container{% elif cloud_service == 'GCP Cloud Function' %}waits until the emulator answers (collections are created on first write){% else %}creates one table per storage container{% endif %}; safe to re-run
{{ "%-22s" | format(cmd.run) }}# {{ emulator.run_note }} with the emulator settings
{{ "%-22s" | format(cmd.down) }}# docker compose down -v: stops the emulator and discards its data
```

{%- if infra_containers %}

To run the `Dockerfile` that Container Apps deploy instead of the local host, use `{{ cmd.container }}` in place of
`{{ cmd.run }}`: it builds the image and starts it on `http://localhost:7071` beside the emulator
(`docker compose --profile api up --build api`), reaching Cosmos DB as `http://cosmos:8081/` on the compose network.
{%- endif %}

`{{ cmd.logs }}` follows the emulator's logs. {{ emulator_settings }} `.env.emulator` is committed and holds only public emulator values; keep real credentials in untracked files such as {{ emulator.secret_files }}, which git ignores. The emulator keeps no data outside its container, so `{{ cmd.down }}` (or removing the container) discards every record.
{%- if cloud_service == 'Azure Function App' %}

| Port | Purpose |
| --- | --- |
| 8081 | Cosmos DB gateway (`http://localhost:8081/`) |
| 1234 | Data Explorer: open `http://localhost:1234` to browse databases and items |

- The image is the Linux [vNext emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) (preview). It serves plain HTTP, so there is no certificate to trust, and it runs natively on x64 and arm64, including Apple Silicon. Partition key `/id`, conditional patch (soft delete), `OFFSET`/`LIMIT` and parameterized queries all work against it.
- {{ emulator.cosmos_key }} is the emulator's well-known account key, published by Microsoft; it is not a secret and only works against the emulator.
- {{ emulator.cosmos_flag }} Never set it outside local development; when it is unset or false the client is configured exactly as in production.
- To use the older HTTPS-only emulator (`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest`) instead, swap the image in `docker-compose.yml` (its readiness probe is `https://localhost:8081/_explorer/emulator.pem`) and {{ emulator.cosmos_https }}. That image runs on x64 only (not on Apple Silicon) and takes 1 to 3 minutes to start.
{%- elif cloud_service == 'GCP Cloud Function' %}

| Port | Purpose |
| --- | --- |
| 8085 | Firestore emulator (8080 in the container; 8085 on the host leaves 8080 to the Functions Framework) |

- {{ emulator.firestore_client }} `GCP_PROJECT_ID` uses a `demo-` project id, which can never reach a real project.
- The emulator keeps data in memory, enforces no IAM or security rules for server SDKs and does not require composite indexes. Keep `FIRESTORE_DATABASE` at `(default)` locally.
{%- else %}

| Port | Purpose |
| --- | --- |
| 8000 | DynamoDB Local (`-inMemory -sharedDb`, so tables do not depend on the region or access key) |

- `{{ cmd.seed }}` runs on your machine and reaches DynamoDB Local at `http://localhost:8000` (`.env.emulator`). `sam local start-api` runs the function in a container on the `{{ project_endpoint }}-emulator` Docker network, so `env.emulator.json` points it at `http://dynamodb:8000` and sets the table names, which `sam local` would otherwise take from the template's logical ids. `--warm-containers LAZY` keeps each function's container running between requests and restarts it when the build changes.
- `template.yaml` declares `AWS_ENDPOINT_URL_DYNAMODB` only so `sam local` can set it; deployed stacks leave it out unless you pass the `DynamoDbEndpoint` parameter. {{ emulator.sdk_note }}
- `{{ cmd.seed }}` and `{{ cmd.run }}` use dummy credentials, which replace your own for these commands. DynamoDB Local 3 accepts only letters and digits in an access key id.
{%- endif %}

Startup takes {% if cloud_service == 'Azure Function App' %}about 10 to 60 seconds; the healthcheck allows up to 4 minutes for slow machines and CI runners{% elif cloud_service == 'GCP Cloud Function' %}about 10 to 20 seconds; the healthcheck allows up to 2 minutes{% else %}a few seconds; the healthcheck allows up to 2 minutes{% endif %}. `{{ cmd.seed }}` retries for up to 2 minutes and exits non-zero if the emulator never becomes ready, so it also works as a readiness gate in scripts.

Troubleshooting:

- **Port already in use:** another emulator or service holds a port above. Stop it, or change the host port in `docker-compose.yml` and in `.env.emulator`.
- **`{{ cmd.up }}` fails or never turns healthy:** check `{{ cmd.logs }}`, and that Docker has enough free memory for the emulator.
- **Requests fail right after starting:** {% if cloud_service == 'GCP Cloud Function' %}check that `{{ cmd.seed }}` passes and that the API was started with `{{ cmd.run }}`, which sets `FIRESTORE_EMULATOR_HOST`{% else %}run `{{ cmd.seed }}`; the {% if cloud_service == 'Azure Function App' %}database and containers{% else %}tables{% endif %} do not exist until it has run, and `{{ cmd.down }}` deletes them{% endif %}.
{%- if cloud_service == 'Azure Function App' %}
- **A missing container returns 404 instead of 500:** the emulator reports a missing container without the sub-status a Cosmos DB account sends{{ emulator.missing_container }}; run `{{ cmd.seed }}`.
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
- **`network {{ project_endpoint }}-emulator not found`:** run `{{ cmd.up }}` before `{{ cmd.run }}`.
- **`UnrecognizedClientException`:** the function did not get the dummy credentials; run it through `{{ cmd.run }}`, which passes them to `sam local`.
{%- endif %}
