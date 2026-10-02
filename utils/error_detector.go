package utils

import (
	"context"
	"encoding/json"
	"errors"
	"log/slog"
	"runtime/debug"

	"github.com/aws/aws-lambda-go/events"

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
