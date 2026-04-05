package utils

import (
	"encoding/json"
	"errors"
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	"net/http"
	"net/http/httptest"
{%- endif %}
	"testing"

	"{{cookiecutter.project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
)
{% if cookiecutter.cloud_service == 'Azure Function App' %}
func TestDetectError_WithNotFoundError_Returns404(t *testing.T) {
	// Arrange
	w := httptest.NewRecorder()
	err := &models.NotFoundError{Message: "Item not found"}

	// Act
	DetectError(w, err)

	// Assert
	assert.Equal(t, http.StatusNotFound, w.Code)
	var baseError models.BaseError
	json.NewDecoder(w.Body).Decode(&baseError)
	assert.Equal(t, "Item not found", baseError.ErrorMessage)
}

func TestDetectError_WithValidationError_Returns400(t *testing.T) {
	// Arrange
	w := httptest.NewRecorder()
	err := &models.ValidationError{Message: "Name is required."}

	// Act
	DetectError(w, err)

	// Assert
	assert.Equal(t, http.StatusBadRequest, w.Code)
	var baseError models.BaseError
	json.NewDecoder(w.Body).Decode(&baseError)
	assert.Equal(t, "Name is required.", baseError.ErrorMessage)
}

func TestDetectError_WithGenericError_Returns500(t *testing.T) {
	// Arrange
	w := httptest.NewRecorder()
	err := errors.New("Mock exception")

	// Act
	DetectError(w, err)

	// Assert
	assert.Equal(t, http.StatusInternalServerError, w.Code)
	var baseError models.BaseError
	json.NewDecoder(w.Body).Decode(&baseError)
	assert.Equal(t, "Mock exception", baseError.ErrorMessage)
}
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
func TestDetectError_WithNotFoundError_Returns404(t *testing.T) {
	// Arrange
	err := &models.NotFoundError{Message: "Item not found"}

	// Act
	resp := DetectError(err)

	// Assert
	assert.Equal(t, 404, resp.StatusCode)
	var baseError models.BaseError
	json.Unmarshal([]byte(resp.Body), &baseError)
	assert.Equal(t, "Item not found", baseError.ErrorMessage)
}

func TestDetectError_WithValidationError_Returns400(t *testing.T) {
	// Arrange
	err := &models.ValidationError{Message: "Name is required."}

	// Act
	resp := DetectError(err)

	// Assert
	assert.Equal(t, 400, resp.StatusCode)
	var baseError models.BaseError
	json.Unmarshal([]byte(resp.Body), &baseError)
	assert.Equal(t, "Name is required.", baseError.ErrorMessage)
}

func TestDetectError_WithGenericError_Returns500(t *testing.T) {
	// Arrange
	err := errors.New("Mock exception")

	// Act
	resp := DetectError(err)

	// Assert
	assert.Equal(t, 500, resp.StatusCode)
	var baseError models.BaseError
	json.Unmarshal([]byte(resp.Body), &baseError)
	assert.Equal(t, "Mock exception", baseError.ErrorMessage)
}

func TestGenerateErrorResponse_ReturnsCorrectResponse(t *testing.T) {
	// Act
	resp := GenerateErrorResponse("Not Found", 404)

	// Assert
	assert.Equal(t, 404, resp.StatusCode)
	assert.Equal(t, "application/json", resp.Headers["Content-Type"])
	var baseError models.BaseError
	json.Unmarshal([]byte(resp.Body), &baseError)
	assert.Equal(t, "Not Found", baseError.ErrorMessage)
}
{%- endif %}
