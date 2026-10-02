package handlers

import (
	"encoding/json"
	"net/http"
	"os"
	"sort"
	"strings"
	"testing"

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

type routeRecorder struct {
	patterns []string
}

func (r *routeRecorder) HandleFunc(pattern string, handler func(http.ResponseWriter, *http.Request)) {
	r.patterns = append(r.patterns, pattern)
}

func TestOpenAPI_SpecListsExactlyTheRegisteredRoutes(t *testing.T) {
	doc := loadOpenAPI(t)
	recorder := &routeRecorder{}

	RegisterRoutes(recorder, Controllers{})

	var routes []string
	for _, pattern := range recorder.patterns {
		if method, path, ok := strings.Cut(pattern, " "); ok {
			assert.True(t, strings.HasPrefix(path, "/api/"), pattern)
			path = strings.TrimPrefix(path, "/api")
			routes = append(routes, method+" "+path)
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
