package utils

import (
	"context"
	"encoding/json"
	"errors"
	"log/slog"
{%- if cloud_service != 'AWS Lambda' %}
	"net/http"
{%- endif %}
	"runtime/debug"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}

	"{{project_endpoint}}/models"
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
{%- if cloud_service != 'AWS Lambda' %}

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
{%- else %}

func JSONResponse(statusCode int, body any) events.APIGatewayProxyResponse {
	data, err := json.Marshal(body)
	if err != nil {
		LogUnexpected(err)
		return GenerateErrorResponse(UnexpectedErrorMessage, 500)
	}
	return events.APIGatewayProxyResponse{
		StatusCode: statusCode,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(data),
	}
}

func GenerateErrorResponse(message string, statusCode int) events.APIGatewayProxyResponse {
	return JSONResponse(statusCode, models.BaseError{ErrorMessage: message})
}

func DetectError(ctx context.Context, err error) events.APIGatewayProxyResponse {
	statusCode, message := errorStatus(ctx, err)
	return GenerateErrorResponse(message, statusCode)
}
{%- endif %}
