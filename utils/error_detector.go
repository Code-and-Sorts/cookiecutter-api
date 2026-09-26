package utils

import (
	"encoding/json"
	"github.com/aws/aws-lambda-go/events"

	"kitties/models"
)

func DetectError(err error) events.APIGatewayProxyResponse {
	var statusCode int

	switch err.(type) {
	case *models.NotFoundError:
		statusCode = 404
	case *models.ValidationError:
		statusCode = 400
	default:
		statusCode = 500
	}

	return GenerateErrorResponse(err.Error(), statusCode)
}

func GenerateErrorResponse(message string, statusCode int) events.APIGatewayProxyResponse {
	body, _ := json.Marshal(models.BaseError{ErrorMessage: message})
	return events.APIGatewayProxyResponse{
		StatusCode: statusCode,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}
}
