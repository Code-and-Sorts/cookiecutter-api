package services

import (
	"testing"

	"{{cookiecutter.project_endpoint}}/models"

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
	"required": ["name"]
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
	// Arrange
	validator := newTestValidator(t)
	data := map[string]string{"name": "TestItem"}

	// Act
	err := validator.Validate(data, "test_schema")

	// Assert
	assert.NoError(t, err)
}

func TestValidate_WithMissingRequiredField_ReturnsValidationError(t *testing.T) {
	// Arrange
	validator := newTestValidator(t)
	data := map[string]string{}

	// Act
	err := validator.Validate(data, "test_schema")

	// Assert
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestValidate_WithEmptyName_ReturnsValidationError(t *testing.T) {
	// Arrange
	validator := newTestValidator(t)
	data := map[string]string{"name": ""}

	// Act
	err := validator.Validate(data, "test_schema")

	// Assert
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}
