package handlers
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}

import (
	"encoding/json"
	"net/http"
)

// HandleHealth reports that the API is up.
func HandleHealth() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"status": "ok"})
	}
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

import "github.com/aws/aws-lambda-go/events"

// HandleHealth reports that the API is up.
func HandleHealth() (events.APIGatewayProxyResponse, error) {
	return jsonResponse(200, map[string]string{"status": "ok"})
}
{%- endif %}
