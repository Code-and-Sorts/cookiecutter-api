{%- set route_prefix = '/api' if cloud_service == 'Azure Function App' else '' -%}
{%- set request_ops = [('create', 'CreateRequest'), ('update', 'UpdateRequest'), ('replace', 'ReplaceRequest')] -%}
package handlers

import (
{%- if cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"encoding/json"
	"net/http"
{%- if cloud_service != 'AWS Lambda' %}
	"net/http/httptest"
{%- endif %}
	"os"
	"sort"
	"strings"
	"testing"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}

	"{{project_endpoint}}/controllers"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

type openAPIDocument struct {
	Paths      map[string]map[string]json.RawMessage `json:"paths"`
	Components struct {
		Schemas map[string]map[string]any `json:"schemas"`
	} `json:"components"`
}

func loadOpenAPI(t *testing.T) ([]byte, openAPIDocument) {
	t.Helper()
	spec, err := os.ReadFile("../openapi.json")
	require.NoError(t, err)
	var doc openAPIDocument
	require.NoError(t, json.Unmarshal(spec, &doc))
	return spec, doc
}

func (doc openAPIDocument) routes() []string {
	var routes []string
	for path, item := range doc.Paths {
		for method := range item {
			if method != "parameters" {
				routes = append(routes, strings.ToUpper(method)+" "+path)
			}
		}
	}
	sort.Strings(routes)
	return routes
}
{%- if cloud_service != 'AWS Lambda' %}

type routeRecorder struct {
	patterns []string
}

func (r *routeRecorder) HandleFunc(pattern string, handler func(http.ResponseWriter, *http.Request)) {
	r.patterns = append(r.patterns, pattern)
}

func TestOpenAPI_Get_ServesSpec(t *testing.T) {
	mux := NewRouter()
	RegisterOpenAPIRoute(mux, []byte(`{"openapi": "3.1.0"}`))
	w := httptest.NewRecorder()

	mux.ServeHTTP(w, httptest.NewRequest(http.MethodGet, "{{ route_prefix }}/openapi.json", nil))

	assert.Equal(t, http.StatusOK, w.Code)
	assert.Equal(t, "application/json", w.Header().Get("Content-Type"))
	assert.JSONEq(t, `{"openapi": "3.1.0"}`, w.Body.String())
}

func TestOpenAPI_OtherMethod_Returns405(t *testing.T) {
	mux := NewRouter()
	RegisterOpenAPIRoute(mux, nil)
	w := httptest.NewRecorder()

	mux.ServeHTTP(w, httptest.NewRequest(http.MethodPost, "{{ route_prefix }}/openapi.json", nil))

	assert.Equal(t, http.StatusMethodNotAllowed, w.Code)
	assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, w.Body.String())
}

func TestOpenAPI_SpecListsExactlyTheRegisteredRoutes(t *testing.T) {
	spec, doc := loadOpenAPI(t)
	recorder := &routeRecorder{}

	RegisterRoutes(recorder, Controllers{}, spec)

	var routes []string
	for _, pattern := range recorder.patterns {
		if method, path, ok := strings.Cut(pattern, " "); ok {
{%- if route_prefix %}
			assert.True(t, strings.HasPrefix(path, "{{ route_prefix }}/"), pattern)
			path = strings.TrimPrefix(path, "{{ route_prefix }}")
{%- endif %}
			routes = append(routes, method+" "+path)
		}
	}
	sort.Strings(routes)
	assert.Equal(t, doc.routes(), routes)
}
{%- else %}

func serveSpec(method string, spec []byte) events.APIGatewayProxyResponse {
	router := NewRouter()
	RegisterOpenAPIRoute(router, spec)
	response, _ := router.ServeRequest(context.Background(), events.APIGatewayProxyRequest{
		HTTPMethod: method,
		Resource:   "/openapi.json",
		Path:       "/openapi.json",
	})
	return response
}

func TestOpenAPI_Get_ServesSpec(t *testing.T) {
	response := serveSpec(http.MethodGet, []byte(`{"openapi": "3.1.0"}`))

	assert.Equal(t, http.StatusOK, response.StatusCode)
	assert.Equal(t, "application/json", response.Headers["Content-Type"])
	assert.JSONEq(t, `{"openapi": "3.1.0"}`, response.Body)
}

func TestOpenAPI_OtherMethod_Returns405(t *testing.T) {
	response := serveSpec(http.MethodPost, nil)

	assert.Equal(t, http.StatusMethodNotAllowed, response.StatusCode)
	assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, response.Body)
}

func TestOpenAPI_SpecListsExactlyTheRegisteredRoutes(t *testing.T) {
	spec, doc := loadOpenAPI(t)
	var fakes Controllers
{%- for resource in resources %}
	fakes.{{ resource.name }} = &fake{{ resource.name }}Controller{}
{%- endfor %}
	router := NewRouter()
	RegisterRoutes(router, fakes, spec)
	const id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

	var routes []string
	for resource := range router.routes {
		for _, method := range []string{http.MethodGet, http.MethodPost, http.MethodPut, http.MethodPatch, http.MethodDelete} {
			response, err := router.ServeRequest(context.Background(), events.APIGatewayProxyRequest{
				HTTPMethod:     method,
				Resource:       resource,
				Path:           strings.ReplaceAll(resource, "{id}", id),
				PathParameters: map[string]string{"id": id},
				Body:           `{"name": "Fake"}`,
			})
			require.NoError(t, err)
			if response.StatusCode != http.StatusMethodNotAllowed {
				routes = append(routes, method+" "+resource)
			}
		}
	}
	sort.Strings(routes)
	assert.Equal(t, doc.routes(), routes)
}
{%- endif %}

func TestOpenAPI_RequestSchemasMatchValidator(t *testing.T) {
	_, doc := loadOpenAPI(t)
	validatorSchemas := controllers.RequestSchemas()
	specToValidator := map[string]string{}
{%- for resource in path_resources %}
{%- for op, suffix in request_ops if op in resource.operations %}
	specToValidator["{{ resource.name }}{{ suffix }}"] = "{{ resource.snake_name }}_{{ op }}_request"
{%- endfor %}
{%- endfor %}

	var specRequests []string
	for name := range doc.Components.Schemas {
		if strings.HasSuffix(name, "Request") {
			specRequests = append(specRequests, name)
		}
	}
	var mapped []string
	for name := range specToValidator {
		mapped = append(mapped, name)
	}
	assert.ElementsMatch(t, mapped, specRequests)
	assert.Len(t, validatorSchemas, len(specToValidator))

	for specName, validatorName := range specToValidator {
		var schema map[string]any
		require.NoError(t, json.Unmarshal([]byte(validatorSchemas[validatorName]), &schema), validatorName)
		delete(schema, "$schema")
		assert.Equal(t, doc.Components.Schemas[specName], schema, specName)
	}
}
