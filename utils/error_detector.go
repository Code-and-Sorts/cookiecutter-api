package utils

import (
	"context"
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"
	"runtime/debug"

	"kittenclaws/models"
)

const (
	NotFoundMessage         = "Not found."
	MethodNotAllowedMessage = "Method not allowed."
	UnexpectedErrorMessage  = "An unexpected error occurred."
)

// Unexpected errors are logged with a stack trace and never shown to clients.
func errorStatus(ctx context.Context, err error) (int, string) {
	var notFound *models.NotFoundError
	var validation *models.ValidationError
	switch {
	case errors.As(err, &notFound):
		return 404, notFound.Message
	case errors.As(err, &validation):
		return 400, validation.Message
	case errors.Is(ctx.Err(), context.Canceled):
		slog.InfoContext(ctx, "Request cancelled by the client", "error", err)
		return 500, UnexpectedErrorMessage
	default:
		LogUnexpected(err)
		return 500, UnexpectedErrorMessage
	}
}

func LogUnexpected(err any) {
	slog.Error("Unexpected error", "error", err, "stack", string(debug.Stack()))
}

func WriteJSON(w http.ResponseWriter, statusCode int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	if err := json.NewEncoder(w).Encode(body); err != nil {
		slog.Error("Failed to write response", "error", err)
	}
}

func WriteError(w http.ResponseWriter, statusCode int, message string) {
	WriteJSON(w, statusCode, models.BaseError{ErrorMessage: message})
}

func DetectError(ctx context.Context, w http.ResponseWriter, err error) {
	statusCode, message := errorStatus(ctx, err)
	WriteError(w, statusCode, message)
}
