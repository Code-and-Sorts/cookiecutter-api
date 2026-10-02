package services

import (
	"bytes"
	"errors"
	"fmt"
	"strings"

	"github.com/santhosh-tekuri/jsonschema/v6"
	"golang.org/x/text/language"
	"golang.org/x/text/message"

	"kittenclaws/models"
)

type SchemaValidator interface {
	// Validate returns a *models.ValidationError for invalid JSON or a schema mismatch.
	Validate(body []byte, schemaName string) error
}

type schemaValidator struct {
	schemas map[string]*jsonschema.Schema
	printer *message.Printer
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

	return &schemaValidator{schemas: schemas, printer: message.NewPrinter(language.English)}, nil
}

func (v *schemaValidator) Validate(body []byte, schemaName string) error {
	sch, ok := v.schemas[schemaName]
	if !ok {
		return fmt.Errorf("schema %s not found", schemaName)
	}

	inst, err := jsonschema.UnmarshalJSON(bytes.NewReader(body))
	if err != nil {
		return &models.ValidationError{Message: "Request body must be valid JSON."}
	}

	if err := sch.Validate(inst); err != nil {
		var validationErr *jsonschema.ValidationError
		if errors.As(err, &validationErr) {
			return &models.ValidationError{Message: v.describe(validationErr)}
		}
		return err
	}

	return nil
}

func (v *schemaValidator) describe(err *jsonschema.ValidationError) string {
	var problems []string
	var collect func(e *jsonschema.ValidationError)
	collect = func(e *jsonschema.ValidationError) {
		if len(e.Causes) == 0 {
			location := "request body"
			if len(e.InstanceLocation) > 0 {
				location = strings.Join(e.InstanceLocation, ".")
			}
			problems = append(problems, location+": "+e.ErrorKind.LocalizedString(v.printer))
		}
		for _, cause := range e.Causes {
			collect(cause)
		}
	}
	collect(err)
	return strings.Join(problems, "; ") + "."
}
