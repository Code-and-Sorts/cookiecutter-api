<img src="./.docs/imgs/cookiecutter_api_header.jpg">

<div align="center">
  <h1>
    <img src="./.docs/imgs/stars.gif" width="32"> Cookiecutter API <img src="./.docs/imgs/stars.gif" width="32">
  </h1>
</div>

![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-python-pipeline.yaml?branch=main&label=Python-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-typescript-pipeline.yaml?branch=main&label=Typescript-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-dotnet-pipeline.yaml?branch=main&label=Dotnet-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/build-go-pipeline.yaml?branch=main&label=Go-Build&style=for-the-badge)
![](https://img.shields.io/github/actions/workflow/status/Code-and-Sorts/cookiecutter-api/integration-tests.yaml?branch=main&label=Integration-Tests&style=for-the-badge)

[![](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](./LICENSE)

[![](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/copier-org/copier/master/img/badge/badge-grayscale-inverted-border-purple.json&style=for-the-badge)](https://github.com/copier-org/copier)


A [Copier](https://github.com/copier-org/copier) template for generating REST APIs across multiple cloud platforms and languages.

## Usage

Install Copier 9.18.2 or newer (and the Jinja extensions the templates use) with pip or pipx:

```console
# pipx is strongly recommended.
pipx install copier
pipx inject copier jinja2-strcase jinja2-time

# If pipx is not an option, install into your Python user directory.
python -m pip install --user copier jinja2-strcase jinja2-time
```

The repository is one Copier template. Its first question is `language` (`python`,
`typescript`, `dotnet` or `go`); answer it at the prompt or pass it with `--data`:

```console
copier copy --trust gh:Code-and-Sorts/cookiecutter-api ./my-api

# Or from a clone, choosing the language up front
git clone https://github.com/Code-and-Sorts/cookiecutter-api.git
copier copy --trust --data language=python ./cookiecutter-api ./my-api
```

`--trust` is needed because the template uses Jinja extensions. Follow the prompts to
configure your project. The generated project includes a `.copier-answers.yml` file that
records the template and the commit it came from, so you can pull in future template
changes with:

```console
cd my-api
copier update --trust
```

### Migrating a project generated from a language folder

Projects generated before the single template (from `./cookiecutter-api/python` and the
like) record the language folder as `_src_path` and no `_commit`, because the folder was
not a Git repository root. `copier update` stops on them with "Cannot update because cannot
obtain old template references". To move such a project to the single template, edit its
`.copier-answers.yml`:

1. Point `_src_path` at the repository, not the language folder (for example
   `gh:Code-and-Sorts/cookiecutter-api`, or the root of your clone).
2. Add `language: python` (or `typescript`, `dotnet`, `go`).
3. Then either:
   - Add `_commit:` set to the first commit of the single template, which renders the same
     files the language folders did (`git log --diff-filter=A --format=%H -1 -- copier.yml`
     in a clone prints it), commit, and run `copier update --trust`. Copier applies every
     template change since that commit and keeps your own edits. Template changes made
     between your project's generation and that commit are not replayed.
   - Or run `copier recopy --trust --overwrite`, which renders the template again from your
     answers over the project, then review `git diff` and restore your own edits.

### Multiple resources

By default a generated project exposes one REST resource named after the project. To
expose several, answer the `resources` prompt or pass them in a YAML file:

```yaml
resources:
  - name: "Cat"
    endpoint: "cats"
    container: "animals"
    operations: ["list", "get_by_id", "create", "update", "delete"]
  - name: "Dog"
    endpoint: "dogs"
    container: "animals"
    operations: ["list", "get_by_id", "create", "replace", "delete"]
```

```console
copier copy --trust --data language=typescript --data-file resources.yml gh:Code-and-Sorts/cookiecutter-api ./my-api
```

`name` is the PascalCase type name, `endpoint` the URL segment, and `container` the storage
container, collection or table. Resources that share a `container` share their records.
`operations` is any subset of `list`, `get_by_id`, `create`, `update` (PATCH), `replace`
(PUT) and `delete`; only those routes are generated.

Every project also gets a `GET` health check named `health`, served under the same route
prefix as its resources (for example `/api/health` on Azure Functions). Answer
`health_endpoint` (or pass `--data health_endpoint=status`) to use another name, or leave it
empty to skip the health check.

Each resource gets its own files in every layer, for example `CatController.cs` and
`DogController.cs` in .NET, or `controllers/cat.controller.ts` and
`controllers/dog.controller.ts` in TypeScript.

### Resource models

Every resource stores the fields of the `base_model` answer plus its own `fields`, and every
language renders its entity, request, response and validation types from those answers, so all
four produce the same request bodies, responses, rules and stored records. The default answers
give each resource one required, non-empty string `name`, which reproduces the classic
`{"id", "name"}` item:

```yaml
resources:
  - name: "Cat"
    endpoint: "cats"
    container: "cats"
    operations: ["list", "get_by_id", "create", "update", "replace", "delete"]
    fields:
      - {name: name, type: string, required: true, rules: {min_length: 1, max_length: 100}}
      - {name: breed, type: enum, values: [siamese, persian, tabby], default: tabby}
      - {name: ageYears, type: integer, default: 0, rules: {minimum: 0, maximum: 40}}
      - {name: weightKg, type: number, nullable: true, rules: {exclusive_minimum: 0}}
      - {name: birthDate, type: date, default: $today}
      - {name: microchipId, type: uuid, immutable: true, default: $uuid}
      - {name: tags, type: array, items: string, default: [], rules: {max_items: 10, unique_items: true}}
    requests:
      update: [name, ageYears, weightKg, tags]
```

| Key | Meaning |
|---|---|
| `name` | lowerCamelCase JSON and database name. Names that break a target language are reserved: Python keywords, a few builtins and members such as `str`, `json` or `toString`, and names of generated members such as `sent` or `applyTo`. |
| `type` | `string`, `integer` (a whole number, also when sent as `1.0` or `1e3`), `number`, `boolean`, `date-time` (any RFC 3339 offset up to ±23:59, stored in UTC with milliseconds; years 0001 to 9999 in UTC), `date` (years 0001 to 9999), `uuid`, `enum` (with `values`) or `array` (with `items`, any type but `enum` and `array`). |
| `required` | The client must send it on create and replace unless it has a `default`. |
| `nullable` | `null` is a valid value; on update it clears the field. A record without a value for a nullable field reads as `null`, even when the field has a default. |
| `default` | A value, `null`, or `$now`, `$today`, `$uuid` evaluated on every write (`$$` escapes a literal `$`). Applied on create and replace, never on update; a stored record without the field reads as its static default. |
| `immutable` | Accepted on create only; replace keeps the stored value. |
| `hidden` | Stored but never returned. |
| `rules` | `min_length`, `max_length`, `pattern`, `format` (`email`, `uri`) for strings; `minimum`, `maximum`, `exclusive_minimum`, `exclusive_maximum` for numbers; `min_items`, `max_items`, `unique_items` for arrays. Lengths count characters (code points), so an emoji is one. Patterns are limited to what every target regex engine reads the same way, counting code points: literal characters up to U+FFFE, `.` (any character but a line feed), classes such as `[a-z]` or `[^,]`, `^`, `$`, `|`, groups `( )` and `(?: )`, the repeats `*`, `+`, `?`, `{n}`, `{n,}` and `{n,m}` (at most 1000 counted repeats, nested ones multiplied), each optionally made lazy with one `?`, and the escapes `\n`, `\t` and `\` before a metacharacter (`\.`, and `\-` inside a class). Shorthands such as `\d`, `\w`, `\s` and `\b`, other escapes, lookarounds, backreferences, named groups, POSIX classes, possessive or stacked quantifiers, `&&`, `~~` or `--` inside a class, and unmatched `]` or `}` are rejected. |
| `example` | A valid value for generated tests and sample requests; needed for a `pattern` the default cannot satisfy. |
| `description` | Documentation, on one line: a comment on the field in the Python, TypeScript and .NET models, a JSON Schema description in Go, and a column in the generated README's data model. |

`requests.create`, `requests.replace` and `requests.update` narrow the fields each body accepts
(by default create accepts every field, replace and update every field that is not immutable).
`base_model` is pre-populated with the built-in fields `id`, `isDeleted`, `createdTimestamp`,
`updatedTimestamp`, `createdBy` and `updatedBy`. They are locked (written exactly as
pre-populated), set by the server and never accepted in a body; `createdBy`/`updatedBy` come from the
`X-User-Id` header. Fields you append to `base_model`, such as `{name: tenantId, type: string,
required: true}`, appear on every resource. Responses hold `id` and every field that is not
hidden. Unknown fields, wrongly typed values and broken rules are rejected with a 400. Copier
checks every answer before it renders and names the resource and field at fault.

> [!NOTE]
> **Updating an existing project** (`copier update`): answers without `fields` or `base_model`
> get the defaults, so the API keeps its `{"id", "name"}` contract. Generated code changes shape:
> every language now has `Base*` model types and per-resource `*CreateRequest`, `*ReplaceRequest`,
> `*UpdateRequest` and `*Response` types; Python renames `Base<Resource>`/`<Resource>Update` and
> TypeScript `<Resource>RequestSchema`/`<Resource>UpdateSchema` accordingly; Go stores fields as
> pointers and replace now keeps fields its body does not accept; a field without a value is no
> longer stored as `null`. Re-apply any hand edits to the generated models in the answers instead.

### Run locally against an emulator

Every generated project can run without a cloud account. Its `docker-compose.yml` starts
only the database emulator for the chosen cloud, `.env.emulator` holds the public emulator
settings, and a bootstrap command creates one container or table per `container`:

| Cloud | Emulator | Host port |
| --- | --- | --- |
| Azure | [Cosmos DB vNext emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) (plain HTTP, x64 and arm64) | 8081, Data Explorer on 1234 |
| AWS | [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) | 8000 |
| GCP | [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) | 8085 |

```console
make emulator-up && make emulator-seed && make run-emulator   # Python, .NET and Go
yarn emulator:up && yarn emulator:seed && yarn start:emulator  # TypeScript
```

The SDK clients switch to the emulator only when its settings are present (a
Cosmos DB emulator flag, `AWS_ENDPOINT_URL_DYNAMODB` or `FIRESTORE_EMULATOR_HOST`), so
deployed code paths are unchanged. AWS projects run through `sam local start-api` on the
emulator's Docker network with `env.emulator.json`. Each generated README covers ports,
credentials, limitations and troubleshooting. The compose file, `.env.emulator`,
`env.emulator.json`, the shared Make targets and that README section are kept once in
`shared/` for every language.

### Integration tests

`tests/integration` is one black-box pytest suite that checks the [API contract](./AGENTS.md#api-contract)
over HTTP for every language and cloud: each enabled operation, validation, soft deletes, shared
containers, `?limit=`, the health check, the stored record format (read straight from the
emulator) and every declared field's types, rules, defaults and nullability. It reads `base_model`
and the resources from the project's `.copier-answers.yml`, so every fixture is covered without
changes. The `Integration Tests` workflow runs it for every language and cloud with the `single`
and `edge` fixtures on pushes to `main`; run it on a branch with
**Run workflow**, choosing a language, cloud and fixture or `all`.

The `Template Checks` workflow runs on pull requests: `.github/scripts/check_invalid_answers.py`
asserts each answers file in `.github/actions/setup-copier-template/invalid/` is rejected with the
message its first line names, `defaults_guard.py` renders every language, cloud and fixture with
and without the default `fields` and `base_model` and diffs the projects, and `base_consistency.py`
checks every language renders the same base types.

To run it locally, render a project, start it with its emulator commands from
[Run locally against an emulator](#run-locally-against-an-emulator), and point the suite at it with
[uv](https://docs.astral.sh/uv/):

```console
copier copy --defaults --trust --vcs-ref HEAD --data language=go --data cloud_service="GCP Cloud Function" \
  --data-file .github/actions/setup-copier-template/fixtures/edge-resources.yml . ../KittenClaws

uv sync --project tests/integration
uv run --project tests/integration pytest tests/integration --project-dir ../KittenClaws --base-url http://localhost:8080
```

The base URL includes the route prefix: `http://localhost:7071/api` on Azure, `http://localhost:8080` on
GCP and `http://localhost:3000` on AWS. The suite waits up to `--ready-timeout` seconds (180) for the API
to answer, writes to the emulator and leaves its records there; `make emulator-down` discards them.

## Supported Templates

Pick a row with the `language` answer (`python`, `typescript`, `dotnet` or `go`) and a
column with `cloud_service`.

<table width="100%">
  <tr>
    <th width="10%" rowspan="2"></th>
    <td width="30%" align="center"><img src="./.docs/imgs/azure.svg" height="18"> Azure</td>
    <td width="30%" align="center"><img src="./.docs/imgs/aws.svg" height="18"> AWS</td>
    <td width="30%" align="center"><img src="./.docs/imgs/google-cloud.svg" height="18"> GCP</td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/function-app.svg" height="18" title="Function App"> Function App</td>
    <td align="center"><img src="./.docs/imgs/lambda.svg" height="18" title="Lambda"> Lambda</td>
    <td align="center"><img src="./.docs/imgs/cloud-function.svg" height="18" title="Cloud Functions"> Cloud Function</td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/python.svg" height="18" title="Python"></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/typescript.svg" height="18" title="NodeJS"></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/dotnet.svg" height="18" title="dotnet"></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
  </tr>
  <tr>
    <td align="center"><img src="./.docs/imgs/golang.svg" height="18" title="Golang"></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
    <td align="center"><span title="Complete">✅</span></td>
  </tr>
</table>

---
> [!NOTE]
> Each project follows the controller-service-repository pattern.

## Examples

Python
- [Function App Example](https://github.com/Code-and-Sorts/cookie-py-az-func-api)

Typescript
- [Function App Example](https://github.com/Code-and-Sorts/cookie-ts-az-func-api)

Dotnet
- [Function App Example](https://github.com/Code-and-Sorts/cookie-cs-az-func-api)

Go
- [Function App Example](https://github.com/Code-and-Sorts/cookie-go-az-func-api)

Every language and cloud is also rendered on each push to `main` and published to
`example/<language>-<cloud>-<single|multi|model>` branches (for example
`example/dotnet-azure-multi`), so you can browse the generated code without running Copier.

## Resources

Below are the SDKs and frameworks used in the various templates.

### Python
- [uv](https://docs.astral.sh/uv/) for dependency management
- [pytest](https://docs.pytest.org/en/stable/) for testing
- [pydantic](https://docs.pydantic.dev/latest/) for schema validation

### Typescript NodeJS
- [Yarn](https://yarnpkg.com/) for dependency management
- [Jest](https://jestjs.io/) for testing
- [Zod](https://zod.dev/) for schema validation

## Dotnet
- [Nuget](https://www.nuget.org/) for dependency management
- [xUnit](https://xunit.net/) for testing
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/) for schema validation

### Go
- [Go Modules](https://go.dev/ref/mod) for dependency management
- [testing](https://pkg.go.dev/testing) and [testify](https://github.com/stretchr/testify) for testing
- [jsonschema](https://github.com/santhosh-tekuri/jsonschema) for schema validation
- [Azure SDK for Go (azcosmos)](https://github.com/Azure/azure-sdk-for-go/tree/main/sdk/data/azcosmos) for Cosmos DB

### Azure
- [Azure Function Apps](https://learn.microsoft.com/en-us/azure/azure-functions/) for hosting the APIs
- [Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/) for data storage
- [Cosmos DB Linux emulator (vNext)](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) for local development

### AWS
- [Lambda docs](https://docs.aws.amazon.com/lambda/) for hosting the APIs
- [DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/GettingStartedDynamoDB.html) for data storage
- [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) for local development

### Google Cloud
- [Cloud Functions](https://cloud.google.com/functions/docs) for hosting the APIs
- [Firestore](https://cloud.google.com/firestore#documentation) for data storage
- [Firestore emulator](https://cloud.google.com/firestore/docs/emulator) for local development

## Acknowledgements

Florian Maas' [cookiecutter-poetry](https://github.com/fpgmaas/cookiecutter-poetry) repository was a helpful resource for building out this template.
