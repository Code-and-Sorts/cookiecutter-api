package utils

import (
	"encoding/json"
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}
	"net/http"
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	"github.com/aws/aws-lambda-go/events"
{%- endif %}

	"{{project_endpoint}}/models"
)
{% if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}
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
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
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
{%- endif %}
