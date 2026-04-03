package utils

import (
	"encoding/json"
	"net/http"

	"{{cookiecutter.project_endpoint}}/models"
)

func DetectError(w http.ResponseWriter, err error) {
	w.Header().Set("Content-Type", "application/json")

	var statusCode int

	switch err.(type) {
	case *models.NotFoundError:
		statusCode = http.StatusNotFound
	case *models.ValidationError:
		statusCode = http.StatusBadRequest
	default:
		statusCode = http.StatusInternalServerError
	}

	w.WriteHeader(statusCode)
	json.NewEncoder(w).Encode(models.BaseError{ErrorMessage: err.Error()})
}
