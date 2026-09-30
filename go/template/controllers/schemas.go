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

func RequestSchemas() map[string]string {
	schemas := make(map[string]string)
{%- for schema in ns.schemas %}
	schemas["{{ schema }}"] = {{ schema | to_lower_camel }}Schema
{%- endfor %}
	return schemas
}
{%- if ns.schemas %}

// Validating the raw body first rejects unknown fields and wrongly typed values.
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
