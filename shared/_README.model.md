{#- The "Data model" README section, rendered from base_model and each resource's fields. -#}
{%- macro cell(f) -%}
{%- set rules = [] -%}
{%- for key, value in f.rules.items() %}{% set _ = rules.append(key ~ ": " ~ (value | tojson)) %}{% endfor -%}
{%- if f.enum_values %}{% set _ = rules.append("one of " ~ (f.enum_values | join(", "))) %}{% endif -%}
{%- set bodies = [] -%}
{%- for op, label in [("create", "POST"), ("replace", "PUT"), ("update", "PATCH")] if f["in_" ~ op] %}{% set _ = bodies.append(label) %}{% endfor -%}
| `{{ f.name }}` | {{ f.type }}{% if f.item_type %} of {{ f.item_type }}{% endif %} | {{ "yes" if f.required else "no" }} | {{ "yes" if f.nullable else "no" }} | {% if f.dynamic %}`${{ f.dynamic }}`{% elif f.has_default %}`{{ f.default | tojson | replace("|", "\\|") }}`{% else %}none{% endif %} | {{ (rules | join("; ") | replace("|", "\\|")) or "none" }} | {{ (bodies | join(", ")) or "none" }} | {{ "no" if f.hidden else "yes" }} |
{%- endmacro -%}
## Data model

Every resource stores the built-in base fields, the fields `base_model` adds and its own `fields`. They
come from the Copier answers in `.copier-answers.yml`: change them there and run `copier update`, so all
layers (models, validation, storage and tests) are regenerated together.

The built-in fields are set by the server and never accepted in a request body: `id` (a UUID, returned),
`isDeleted` (soft delete), `createdTimestamp` and `updatedTimestamp` (ISO-8601 UTC with milliseconds, for
example `2026-09-29T22:49:26.625Z`; a create reads the clock once for both), and `createdBy`/`updatedBy`
(the `X-User-Id` header, stored only when it is sent). Only `id` is returned.
{%- set user_base = client_base_fields %}
{%- if user_base %}

`base_model` adds these fields to every resource:

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned |
|---|---|---|---|---|---|---|---|
{%- for f in user_base %}
{{ cell(f) }}
{%- endfor %}
{%- endif %}
{%- for resource in path_resources %}

`{{ resource.name }}` (`/{{ resource.endpoint }}`) fields:
{%- if resource.fields %}

| Field | Type | Required | Nullable | Default | Rules | Accepted by | Returned |
|---|---|---|---|---|---|---|---|
{%- for f in resource.fields %}
{{ cell(dict(f, in_create=f.in_create and "create" in resource.operations, in_replace=f.in_replace and "replace" in resource.operations, in_update=f.in_update and "update" in resource.operations)) }}
{%- endfor %}
{%- else %} none beyond the base fields.
{%- endif %}
{%- endfor %}

- A response holds `id` and every field that is not hidden, with `null` for a field without a value. A
  list is a JSON array (`[]` when empty).
- A request body must be a JSON object holding only the fields its operation accepts; any other field,
  including `id` and the built-in fields, is a 400. Values are never converted: `"1"` is not an integer.
- Create (POST) and replace (PUT) give a field left out its default (`$now`, `$today` and `$uuid` are
  evaluated on every write) or no value; a required field without a default must be sent. Replace keeps
  the fields it does not accept, such as immutable ones, at their stored values.
- Update (PATCH) changes only the fields sent; `null` clears a nullable field.
- A field without a value is not stored. Date-times are stored in UTC with milliseconds whatever offset
  was sent, dates as `YYYY-MM-DD`. Reading a record that lacks a field returns its static default
  (`null` for a nullable field).
- Integers must lie within ±9007199254740991, so every language and JSON parser reads them exactly.
  Patterns match anywhere in the value unless anchored with `^` and `$`; `email` and `uri` check the
  value's shape only.
