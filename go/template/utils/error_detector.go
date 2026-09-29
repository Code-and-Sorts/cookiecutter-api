package utils

import (
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

// Response messages for errors that do not come from a resource.
const (
	NotFoundMessage         = "Not found."
	MethodNotAllowedMessage = "Method not allowed."
	UnexpectedErrorMessage  = "An unexpected error occurred."
)

// errorStatus maps err to its HTTP status and the message sent to the client.
// Unexpected errors are logged with a stack trace and never shown to clients.
func errorStatus(err error) (int, string) {
	var notFound *models.NotFoundError
	var validation *models.ValidationError
	switch {
	case errors.As(err, &notFound):
		return 404, notFound.Message
	case errors.As(err, &validation):
		return 400, validation.Message
	default:
		LogUnexpected(err)
		return 500, UnexpectedErrorMessage
	}
}

// LogUnexpected logs err at error level with the current stack trace.
func LogUnexpected(err any) {
	slog.Error("Unexpected error", "error", err, "stack", string(debug.Stack()))
}
{%- if cloud_service != 'AWS Lambda' %}

// WriteJSON writes body as a JSON response with the given status code.
func WriteJSON(w http.ResponseWriter, statusCode int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	if err := json.NewEncoder(w).Encode(body); err != nil {
		slog.Error("Failed to write response", "error", err)
	}
}

// WriteError writes the JSON error body {"errorMessage": message}.
func WriteError(w http.ResponseWriter, statusCode int, message string) {
	WriteJSON(w, statusCode, models.BaseError{ErrorMessage: message})
}

// DetectError writes the error response that matches err.
func DetectError(w http.ResponseWriter, err error) {
	statusCode, message := errorStatus(err)
	WriteError(w, statusCode, message)
}
{%- else %}

// JSONResponse builds an API Gateway response with a JSON body.
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

// GenerateErrorResponse builds the JSON error body {"errorMessage": message}.
func GenerateErrorResponse(message string, statusCode int) events.APIGatewayProxyResponse {
	body, _ := json.Marshal(models.BaseError{ErrorMessage: message})
	return events.APIGatewayProxyResponse{
		StatusCode: statusCode,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}
}

// DetectError builds the error response that matches err.
func DetectError(err error) events.APIGatewayProxyResponse {
	statusCode, message := errorStatus(err)
	return GenerateErrorResponse(message, statusCode)
}
{%- endif %}
