package services

import (
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

const testSchema = `{
	"$schema": "https://json-schema.org/draft/2020-12/schema",
	"type": "object",
	"properties": {
		"name": {
			"type": "string",
			"minLength": 1
		}
	},
	"required": ["name"],
	"additionalProperties": false
}`

func newTestValidator(t *testing.T) SchemaValidator {
	t.Helper()
	validator, err := NewSchemaValidator(map[string]string{
		"test_schema": testSchema,
	})
	assert.NoError(t, err)
	return validator
}

func TestValidate_WithValidData_ReturnsNoError(t *testing.T) {
	validator := newTestValidator(t)

	err := validator.Validate([]byte(`{"name": "TestItem"}`), "test_schema")

	assert.NoError(t, err)
}

func TestValidate_WithInvalidBodies_ReturnsValidationError(t *testing.T) {
	validator := newTestValidator(t)
	cases := map[string]struct {
		body    string
		message string
	}{
		"malformed JSON":   {`{"name": `, "Request body must be valid JSON."},
		"trailing content": {`{"name": "a"} {}`, "Request body must be valid JSON."},
		"empty body":       {``, "Request body must be valid JSON."},
		"not an object":    {`["name"]`, "request body: got array, want object."},
		"null body":        {`null`, "request body: got null, want object."},
		"missing name":     {`{}`, "request body: missing property 'name'."},
		"empty name":       {`{"name": ""}`, "name: minLength: got 0, want 1."},
		"number name":      {`{"name": 1}`, "name: got number, want string."},
		"null name":        {`{"name": null}`, "name: got null, want string."},
		"unknown field":    {`{"name": "a", "color": "red"}`, "request body: additional properties 'color' not allowed."},
		"id field":         {`{"name": "a", "id": "x"}`, "request body: additional properties 'id' not allowed."},
		"system field":     {`{"name": "a", "isDeleted": true}`, "request body: additional properties 'isDeleted' not allowed."},
		"several problems": {`{"name": 1, "createdBy": "x"}`, ""},
	}

	for name, tc := range cases {
		t.Run(name, func(t *testing.T) {
			err := validator.Validate([]byte(tc.body), "test_schema")

			var validationErr *models.ValidationError
			assert.ErrorAs(t, err, &validationErr)
			if tc.message != "" {
				assert.Equal(t, tc.message, validationErr.Message)
			}
		})
	}
}

func TestValidate_WithUnknownSchema_ReturnsError(t *testing.T) {
	validator := newTestValidator(t)

	err := validator.Validate([]byte(`{}`), "missing_schema")

	assert.Error(t, err)
	assert.NotErrorAs(t, err, new(*models.ValidationError))
}

func TestNewSchemaValidator_WithInvalidSchema_ReturnsError(t *testing.T) {
	_, err := NewSchemaValidator(map[string]string{"broken": `{`})
	assert.Error(t, err)

	_, err = NewSchemaValidator(map[string]string{"bad_type": `{"type": 5}`})
	assert.Error(t, err)
}
