package handlers

import (
{%- if cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"net/http"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}
)
{%- set route_prefix = '/api' if cloud_service == 'Azure Function App' else '' %}
{%- if cloud_service != 'AWS Lambda' %}

func RegisterOpenAPIRoute(mux Mux, spec []byte) {
	mux.HandleFunc("GET {{ route_prefix }}/openapi.json", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		w.Write(spec)
	})
	mux.HandleFunc("{{ route_prefix }}/openapi.json", methodNotAllowed)
}
{%- else %}

func RegisterOpenAPIRoute(router *Router, spec []byte) {
	router.Handle("/openapi.json", func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		if request.HTTPMethod != http.MethodGet {
			return methodNotAllowed()
		}
		return events.APIGatewayProxyResponse{
			StatusCode: http.StatusOK,
			Headers:    map[string]string{"Content-Type": "application/json"},
			Body:       string(spec),
		}
	})
}
{%- endif %}
