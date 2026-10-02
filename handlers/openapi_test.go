package handlers

import (
	"context"
	"encoding/json"
	"net/http"
	"os"
	"sort"
	"strings"
	"testing"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/controllers"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

type openAPIDocument struct {
	Paths      map[string]map[string]json.RawMessage `json:"paths"`
	Components struct {
		Schemas map[string]map[string]any `json:"schemas"`
	} `json:"components"`
}

func loadOpenAPI(t *testing.T) openAPIDocument {
	t.Helper()
	spec, err := os.ReadFile("../openapi.json")
	require.NoError(t, err)
	var doc openAPIDocument
	require.NoError(t, json.Unmarshal(spec, &doc))
	return doc
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

func TestOpenAPI_SpecListsExactlyTheRegisteredRoutes(t *testing.T) {
	doc := loadOpenAPI(t)
	var fakes Controllers
	fakes.KittenClaws = &fakeKittenClawsController{}
	router := NewRouter()
	RegisterRoutes(router, fakes)
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

func TestOpenAPI_RequestSchemasMatchValidator(t *testing.T) {
	doc := loadOpenAPI(t)
	validatorSchemas := controllers.RequestSchemas()
	specToValidator := map[string]string{}
	specToValidator["KittenClawsCreateRequest"] = "kitten_claws_create_request"
	specToValidator["KittenClawsUpdateRequest"] = "kitten_claws_update_request"

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
