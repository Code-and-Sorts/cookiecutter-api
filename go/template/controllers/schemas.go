{%- set ns = namespace(schemas=[]) -%}
{%- for resource in resources -%}
{%- for op in ["create", "update", "replace"] if op in resource.operations -%}
{%- set ns.schemas = ns.schemas + [(resource.name | to_snake) ~ "_" ~ op ~ "_request"] -%}
{%- endfor -%}
{%- endfor -%}
package controllers
{%- if ns.schemas %}

import _ "embed"
{%- for schema in ns.schemas %}

//go:embed schemas/{{ schema }}.json
var {{ schema | to_lower_camel }}Schema string
{%- endfor %}
{%- endif %}

// RequestSchemas returns the JSON schemas that validate request bodies, keyed
// by the name each controller passes to SchemaValidator.Validate.
func RequestSchemas() map[string]string {
	schemas := make(map[string]string)
{%- for schema in ns.schemas %}
	schemas["{{ schema }}"] = {{ schema | to_lower_camel }}Schema
{%- endfor %}
	return schemas
}
