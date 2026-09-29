{%- set ns = namespace(schemas=[]) -%}
{%- for resource in resources -%}
{%- for op in ["create", "update", "replace"] if op in resource.operations -%}
{%- set ns.schemas = ns.schemas + [(resource.name | to_snake) ~ "_" ~ op ~ "_request"] -%}
{%- endfor -%}
{%- endfor -%}
package controllers
{%- if ns.schemas %}

import (
	_ "embed"
	"encoding/json"
	"io"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/services"
)
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
{%- if ns.schemas %}

// decodeRequest validates the raw request body against the named schema, so
// unknown fields and wrongly typed values are rejected, then decodes it into
// target.
func decodeRequest(validator services.SchemaValidator, body io.Reader, schemaName string, target any) error {
	data, err := io.ReadAll(body)
	if err != nil {
		return &models.ValidationError{Message: "Request body could not be read."}
	}

	if err := validator.Validate(data, schemaName); err != nil {
		return err
	}

	if err := json.Unmarshal(data, target); err != nil {
		return &models.ValidationError{Message: "Request body must be valid JSON."}
	}

	return nil
}
{%- endif %}
