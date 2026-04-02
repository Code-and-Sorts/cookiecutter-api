package services

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/santhosh-tekuri/jsonschema/v6"

	"{{cookiecutter.project_endpoint}}/models"
)

type SchemaValidator interface {
	Validate(data any, schemaJSON string) error
}

type schemaValidator struct{}

func NewSchemaValidator() SchemaValidator {
	return &schemaValidator{}
}

func (v *schemaValidator) Validate(data any, schemaJSON string) error {
	schema, err := jsonschema.UnmarshalJSON(strings.NewReader(schemaJSON))
	if err != nil {
		return fmt.Errorf("failed to unmarshal schema: %w", err)
	}

	c := jsonschema.NewCompiler()
	if err := c.AddResource("schema.json", schema); err != nil {
		return fmt.Errorf("failed to add schema resource: %w", err)
	}

	sch, err := c.Compile("schema.json")
	if err != nil {
		return fmt.Errorf("failed to compile schema: %w", err)
	}

	jsonBytes, err := json.Marshal(data)
	if err != nil {
		return fmt.Errorf("failed to marshal data: %w", err)
	}

	inst, err := jsonschema.UnmarshalJSON(strings.NewReader(string(jsonBytes)))
	if err != nil {
		return fmt.Errorf("failed to unmarshal instance: %w", err)
	}

	if err := sch.Validate(inst); err != nil {
		return &models.ValidationError{Message: "Failed schema validation."}
	}

	return nil
}
