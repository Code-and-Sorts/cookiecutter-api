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

func TestValidate_WithValidData_ReturnsNoError(t *testing.T) {
	// Arrange
	validator := NewSchemaValidator()
	data := map[string]string{"name": "TestItem"}

	// Act
	err := validator.Validate(data, testSchema)

	// Assert
	assert.NoError(t, err)
}

func TestValidate_WithMissingRequiredField_ReturnsValidationError(t *testing.T) {
	// Arrange
	validator := NewSchemaValidator()
	data := map[string]string{}

	// Act
	err := validator.Validate(data, testSchema)

	// Assert
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestValidate_WithEmptyName_ReturnsValidationError(t *testing.T) {
	// Arrange
	validator := NewSchemaValidator()
	data := map[string]string{"name": ""}

	// Act
	err := validator.Validate(data, testSchema)

	// Assert
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}
