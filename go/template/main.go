{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
package main

import (
{%- if cloud_service == 'GCP Cloud Function' or cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"encoding/json"
{%- if cloud_service == 'AWS Lambda' or 'delete' in used_ops %}
	"fmt"
{%- endif %}
	"log"
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}
	"net/http"
{%- endif %}
	"os"
{%- if cloud_service == 'AWS Lambda' or 'list' in used_ops %}
	"strconv"
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	"strings"
{%- endif %}
{% if cloud_service == 'Azure Function App' %}
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	"cloud.google.com/go/firestore"
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	awsconfig "github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
{%- endif %}

	"{{project_endpoint}}/controllers"
	"{{project_endpoint}}/repositories"
	"{{project_endpoint}}/services"
	"{{project_endpoint}}/utils"
)

func newValidator() services.SchemaValidator {
	validator, err := services.NewSchemaValidator(map[string]string{
		"create_request":  controllers.CreateRequestSchema,
		"update_request":  controllers.UpdateRequestSchema,
		"replace_request": controllers.ReplaceRequestSchema,
	})
	if err != nil {
		log.Fatalf("Failed to initialize schema validator: %v", err)
	}
	return validator
}
{% if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}
func main() {
{%- if cloud_service == 'Azure Function App' %}
	listenAddr := ":80"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}

	resourceControllers := initControllers()
{%- elif cloud_service == 'GCP Cloud Function' %}
	listenAddr := ":8080"
	if val, ok := os.LookupEnv("PORT"); ok {
		listenAddr = ":" + val
	}

	resourceControllers, cleanup := initControllers()
	defer cleanup()
{%- endif %}

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/health", handleHealth())
{%- for resource in resources %}
{%- if "list" in resource.operations %}
	mux.HandleFunc("GET /api/{{ resource.endpoint }}", handleGetList(resourceControllers["{{ resource.container }}"]))
{%- endif %}
{%- if "get_by_id" in resource.operations %}
	mux.HandleFunc("GET /api/{{ resource.endpoint }}/{id}", handleGet(resourceControllers["{{ resource.container }}"]))
{%- endif %}
{%- if "create" in resource.operations %}
	mux.HandleFunc("POST /api/{{ resource.endpoint }}", handleCreate(resourceControllers["{{ resource.container }}"]))
{%- endif %}
{%- if "update" in resource.operations %}
	mux.HandleFunc("PATCH /api/{{ resource.endpoint }}/{id}", handleUpdate(resourceControllers["{{ resource.container }}"]))
{%- endif %}
{%- if "replace" in resource.operations %}
	mux.HandleFunc("PUT /api/{{ resource.endpoint }}/{id}", handleReplace(resourceControllers["{{ resource.container }}"]))
{%- endif %}
{%- if "delete" in resource.operations %}
	mux.HandleFunc("DELETE /api/{{ resource.endpoint }}/{id}", handleDelete(resourceControllers["{{ resource.container }}"]))
{%- endif %}
{%- endfor %}

	log.Printf("About to listen on %s", listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, mux))
}

{% if cloud_service == 'Azure Function App' -%}
func initControllers() map[string]controllers.ItemController {
	endpoint := os.Getenv("CosmosDbEndpoint")
	key := os.Getenv("CosmosDbKey")
	databaseName := os.Getenv("CosmosDbDatabaseName")

	if endpoint == "" || key == "" || databaseName == "" {
		log.Fatal("CosmosDbEndpoint, CosmosDbKey and CosmosDbDatabaseName environment variables are required")
	}

	cred, err := azcosmos.NewKeyCredential(key)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB credential: %v", err)
	}

	client, err := azcosmos.NewClientWithKey(endpoint, cred, nil)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB client: %v", err)
	}

	validator := newValidator()
	resourceControllers := make(map[string]controllers.ItemController)
{%- for container in resources | map(attribute='container') | unique %}
	{
		containerName := os.Getenv("CosmosDbContainerName_{{ container | to_camel }}")
		if containerName == "" {
			containerName = "{{ container }}"
		}
		containerClient, err := client.NewContainer(databaseName, containerName)
		if err != nil {
			log.Fatalf("Failed to get Cosmos DB container %s: %v", containerName, err)
		}
		repo := repositories.NewItemRepository(containerClient)
		svc := services.NewItemService(repo)
		resourceControllers["{{ container }}"] = controllers.NewItemController(svc, validator)
	}
{%- endfor %}

	return resourceControllers
}
{%- elif cloud_service == 'GCP Cloud Function' -%}
func initControllers() (map[string]controllers.ItemController, func()) {
	projectID := os.Getenv("GCP_PROJECT_ID")
	if projectID == "" {
		log.Fatal("GCP_PROJECT_ID environment variable is required")
	}
	databaseName := os.Getenv("FIRESTORE_DATABASE")
	if databaseName == "" {
		databaseName = "(default)"
	}

	ctx := context.Background()
	client, err := firestore.NewClientWithDatabase(ctx, projectID, databaseName)
	if err != nil {
		log.Fatalf("Failed to create Firestore client: %v", err)
	}

	validator := newValidator()
	resourceControllers := make(map[string]controllers.ItemController)
{%- for container in resources | map(attribute='container') | unique %}
	{
		collectionName := os.Getenv("FIRESTORE_COLLECTION_{{ container | to_camel }}")
		if collectionName == "" {
			collectionName = "{{ container }}"
		}
		collection := client.Collection(collectionName)
		repo := repositories.NewItemRepository(collection)
		svc := services.NewItemService(repo)
		resourceControllers["{{ container }}"] = controllers.NewItemController(svc, validator)
	}
{%- endfor %}

	return resourceControllers, func() { client.Close() }
}
{%- endif %}

func handleHealth() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"status": "ok"})
	}
}
{%- if 'get_by_id' in used_ops %}

func handleGet(controller controllers.ItemController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Get processed a request for %s.", r.URL.Path)

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Get: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if 'list' in used_ops %}

func handleGetList(controller controllers.ItemController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("GetList processed a request for %s.", r.URL.Path)

		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		result, err := controller.GetList(r.Context(), limit)
		if err != nil {
			log.Printf("Exception in GetList: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if 'create' in used_ops %}

func handleCreate(controller controllers.ItemController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Create processed a request for %s.", r.URL.Path)

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in Create: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if 'update' in used_ops %}

func handleUpdate(controller controllers.ItemController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Update processed a request for %s.", r.URL.Path)

		result, err := controller.Update(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in Update: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if 'replace' in used_ops %}

func handleReplace(controller controllers.ItemController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Replace processed a request for %s.", r.URL.Path)

		result, err := controller.Replace(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in Replace: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if 'delete' in used_ops %}

func handleDelete(controller controllers.ItemController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Delete processed a request for %s.", r.URL.Path)

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Delete: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("item with id %s was deleted successfully.", id),
		})
	}
}
{%- endif %}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
type resourceRoute struct {
	container  string
	operations map[string]bool
}

var (
	resourceControllers = map[string]controllers.ItemController{}

	// routes maps each endpoint to its storage container and enabled operations.
	routes = map[string]resourceRoute{
{%- for resource in resources %}
		"{{ resource.endpoint }}": {container: "{{ resource.container }}", operations: map[string]bool{ {%- for op in resource.operations %}"{{ op }}": true{% if not loop.last %}, {% endif %}{% endfor %}}},
{%- endfor %}
	}
)

func init() {
	cfg, err := awsconfig.LoadDefaultConfig(context.Background())
	if err != nil {
		log.Fatalf("Failed to load AWS config: %v", err)
	}

	client := dynamodb.NewFromConfig(cfg)
	validator := newValidator()
{%- for container in resources | map(attribute='container') | unique %}
	{
		tableName := os.Getenv("DYNAMODB_TABLE_NAME_{{ container | upper | replace('-', '_') }}")
		if tableName == "" {
			tableName = "{{ container }}"
		}
		repo := repositories.NewItemRepository(client, tableName)
		svc := services.NewItemService(repo)
		resourceControllers["{{ container }}"] = controllers.NewItemController(svc, validator)
	}
{%- endfor %}
}

func main() {
	lambda.Start(handler)
}

func methodNotAllowed() (events.APIGatewayProxyResponse, error) {
	return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
}

func handler(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	log.Printf("Received %s request for %s", request.HTTPMethod, request.Resource)

	if request.HTTPMethod == "GET" && (strings.HasSuffix(strings.TrimRight(request.Path, "/"), "/health") || strings.HasSuffix(strings.TrimRight(request.Resource, "/"), "/health")) {
		return handleHealth()
	}

	// ``request.Resource`` is the API Gateway route template (e.g. "/cats/{item_id}"),
	// so the first path segment is the literal endpoint and "{item_id}" marks an
	// id-bearing route. Use it (not the concrete path) to resolve the resource.
	trimmed := strings.Trim(request.Resource, "/")
	endpoint := trimmed
	if idx := strings.Index(trimmed, "/"); idx >= 0 {
		endpoint = trimmed[:idx]
	}
	needsID := strings.Contains(request.Resource, "{item_id}")

	route, ok := routes[endpoint]
	if !ok {
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
	controller := resourceControllers[route.container]

	switch request.HTTPMethod {
	case "GET":
		if needsID {
			if !route.operations["get_by_id"] {
				return methodNotAllowed()
			}
			return handleGet(ctx, controller, request.PathParameters["item_id"])
		}
		if !route.operations["list"] {
			return methodNotAllowed()
		}
		limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
		return handleGetList(ctx, controller, limit)
	case "POST":
		if !route.operations["create"] {
			return methodNotAllowed()
		}
		return handleCreate(ctx, controller, request.Body)
	case "PATCH":
		if !route.operations["update"] {
			return methodNotAllowed()
		}
		return handleUpdate(ctx, controller, request.PathParameters["item_id"], request.Body)
	case "PUT":
		if !route.operations["replace"] {
			return methodNotAllowed()
		}
		return handleReplace(ctx, controller, request.PathParameters["item_id"], request.Body)
	case "DELETE":
		if !route.operations["delete"] {
			return methodNotAllowed()
		}
		return handleDelete(ctx, controller, request.PathParameters["item_id"])
	default:
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
}

func handleGet(ctx context.Context, controller controllers.ItemController, id string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Get processed a request.")

	result, err := controller.Get(ctx, id)
	if err != nil {
		log.Printf("Exception in Get: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleHealth() (events.APIGatewayProxyResponse, error) {
	body, _ := json.Marshal(map[string]string{"status": "ok"})
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleGetList(ctx context.Context, controller controllers.ItemController, limit int) (events.APIGatewayProxyResponse, error) {
	log.Printf("GetList processed a request.")

	result, err := controller.GetList(ctx, limit)
	if err != nil {
		log.Printf("Exception in GetList: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleCreate(ctx context.Context, controller controllers.ItemController, requestBody string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Create processed a request.")

	result, err := controller.Create(ctx, strings.NewReader(requestBody))
	if err != nil {
		log.Printf("Exception in Create: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 201,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleUpdate(ctx context.Context, controller controllers.ItemController, id string, requestBody string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Update processed a request.")

	result, err := controller.Update(ctx, id, strings.NewReader(requestBody))
	if err != nil {
		log.Printf("Exception in Update: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleReplace(ctx context.Context, controller controllers.ItemController, id string, requestBody string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Replace processed a request.")

	result, err := controller.Replace(ctx, id, strings.NewReader(requestBody))
	if err != nil {
		log.Printf("Exception in Replace: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleDelete(ctx context.Context, controller controllers.ItemController, id string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Delete processed a request.")

	err := controller.Delete(ctx, id)
	if err != nil {
		log.Printf("Exception in Delete: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(map[string]string{
		"message": fmt.Sprintf("item with id %s was deleted successfully.", id),
	})
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}
{%- endif %}
