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
	c := jsonschema.NewCompiler()
	if err := c.AddResource("schema.json", strings.NewReader(schemaJSON)); err != nil {
		return fmt.Errorf("failed to add schema resource: %w", err)
	}

	schema, err := c.Compile("schema.json")
	if err != nil {
		return fmt.Errorf("failed to compile schema: %w", err)
	}

	jsonBytes, err := json.Marshal(data)
	if err != nil {
		return fmt.Errorf("failed to marshal data: %w", err)
	}

	var inst any
	if err := json.Unmarshal(jsonBytes, &inst); err != nil {
		return fmt.Errorf("failed to unmarshal data: %w", err)
	}

	if err := schema.Validate(inst); err != nil {
		return &models.ValidationError{Message: "Failed schema validation."}
	}

	return nil
}
