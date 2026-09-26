package utils

import (
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"testing"

	"kitties/models"

	"github.com/stretchr/testify/assert"
)

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
