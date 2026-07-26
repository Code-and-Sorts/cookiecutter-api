package utils

import (
	"encoding/json"
	"errors"
	"testing"

	"kitties/models"

	"github.com/stretchr/testify/assert"
)

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
