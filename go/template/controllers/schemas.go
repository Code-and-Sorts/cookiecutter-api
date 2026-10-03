{%- set ns = namespace(schemas=[]) -%}
{%- for resource in resources -%}
{%- for op in ["create", "update", "replace"] if op in resource.operations -%}
{%- set ns.schemas = ns.schemas + [(resource.name | to_snake) ~ "_" ~ op ~ "_request"] -%}
{%- endfor -%}
{%- endfor -%}
package controllers
{%- if ns.schemas %}

import (
	"bytes"
	_ "embed"
	"encoding/json"
	"io"
	"math"
	"strconv"

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

// Returns the properties the body sent, so an update can tell an absent field from an explicit null.
func decodeRequest(validator services.SchemaValidator, body io.Reader, schemaName string, target any) (map[string]bool, error) {
	data, err := io.ReadAll(body)
	if err != nil {
		return nil, &models.ValidationError{Message: "Request body could not be read."}
	}

	if err := validator.Validate(data, schemaName); err != nil {
		return nil, err
	}

	// The schema reads 1.0 and 1e3 as integers, so whole numbers are rewritten before they reach int64 fields.
	decoder := json.NewDecoder(bytes.NewReader(data))
	decoder.UseNumber()
	var properties map[string]any
	if err := decoder.Decode(&properties); err != nil {
		return nil, err
	}
	normalized, err := json.Marshal(wholeNumbers(properties))
	if err != nil {
		return nil, err
	}
	if err := json.Unmarshal(normalized, target); err != nil {
		return nil, &models.ValidationError{Message: err.Error()}
	}

	sent := make(map[string]bool, len(properties))
	for name := range properties {
		sent[name] = true
	}
	return sent, nil
}

func wholeNumbers(value any) any {
	switch v := value.(type) {
	case map[string]any:
		for key, item := range v {
			v[key] = wholeNumbers(item)
		}
	case []any:
		for index, item := range v {
			v[index] = wholeNumbers(item)
		}
	case json.Number:
		if number, err := v.Float64(); err == nil && number == math.Trunc(number) && math.Abs(number) < math.MaxInt64 {
			return json.Number(strconv.FormatInt(int64(number), 10))
		}
	}
	return value
}
{%- endif %}
