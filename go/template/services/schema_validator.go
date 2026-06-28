package services

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/santhosh-tekuri/jsonschema/v6"

	"{{project_endpoint}}/models"
)

type SchemaValidator interface {
	Validate(data any, schemaName string) error
}

type schemaValidator struct {
	schemas map[string]*jsonschema.Schema
}

func NewSchemaValidator(schemaDefs map[string]string) (SchemaValidator, error) {
	schemas := make(map[string]*jsonschema.Schema, len(schemaDefs))
	for name, schemaJSON := range schemaDefs {
		resource, err := jsonschema.UnmarshalJSON(strings.NewReader(schemaJSON))
		if err != nil {
			return nil, fmt.Errorf("failed to unmarshal schema %s: %w", name, err)
		}

		c := jsonschema.NewCompiler()
		if err := c.AddResource(name, resource); err != nil {
			return nil, fmt.Errorf("failed to add schema resource %s: %w", name, err)
		}

		sch, err := c.Compile(name)
		if err != nil {
			return nil, fmt.Errorf("failed to compile schema %s: %w", name, err)
		}

		schemas[name] = sch
	}

	return &schemaValidator{schemas: schemas}, nil
}

func (v *schemaValidator) Validate(data any, schemaName string) error {
	sch, ok := v.schemas[schemaName]
	if !ok {
		return fmt.Errorf("schema %s not found", schemaName)
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
