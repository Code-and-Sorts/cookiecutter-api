package handlers

import (
	"context"
	"net/http"
	"testing"

	"github.com/aws/aws-lambda-go/events"

	"github.com/stretchr/testify/assert"
)

func callHealth(method, resource string) events.APIGatewayProxyResponse {
	router := NewRouter()
	RegisterHealthRoute(router)
	response, _ := router.ServeRequest(context.Background(), events.APIGatewayProxyRequest{
		HTTPMethod: method,
		Resource:   resource,
		Path:       resource,
	})
	return response
}

func TestHealth_Get_ReturnsOK(t *testing.T) {
	response := callHealth(http.MethodGet, "/health")

	assert.Equal(t, http.StatusOK, response.StatusCode)
	assert.Equal(t, "application/json", response.Headers["Content-Type"])
	assert.JSONEq(t, `{"status": "ok"}`, response.Body)
}

func TestHealth_OtherMethod_Returns405(t *testing.T) {
	response := callHealth(http.MethodPost, "/health")

	assert.Equal(t, http.StatusMethodNotAllowed, response.StatusCode)
	assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, response.Body)
}

func TestHealth_OnlyMatchesExactPath(t *testing.T) {
	response := callHealth(http.MethodGet, "/items/{id}")

	assert.Equal(t, http.StatusNotFound, response.StatusCode)
}
