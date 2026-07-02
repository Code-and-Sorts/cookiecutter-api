{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
package main

import (
{%- if cloud_service == 'GCP Cloud Function' or cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"encoding/json"
{%- if 'delete' in used_ops %}
	"fmt"
{%- endif %}
	"log"
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}
	"net/http"
{%- endif %}
	"os"
{%- if 'list' in used_ops %}
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
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}

type appControllers struct {
{%- for resource in resources %}
	{{ resource.name }} controllers.{{ resource.name }}Controller
{%- endfor %}
}

func main() {
{%- if cloud_service == 'Azure Function App' %}
	listenAddr := ":80"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}

	c := initControllers()
{%- elif cloud_service == 'GCP Cloud Function' %}
	listenAddr := ":8080"
	if val, ok := os.LookupEnv("PORT"); ok {
		listenAddr = ":" + val
	}

	c, cleanup := initControllers()
	defer cleanup()
{%- endif %}

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/health", handleHealth())
{%- for resource in resources %}
{%- if "list" in resource.operations %}
	mux.HandleFunc("GET /api/{{ resource.endpoint }}", handleGet{{ resource.name }}List(c.{{ resource.name }}))
{%- endif %}
{%- if "get_by_id" in resource.operations %}
	mux.HandleFunc("GET /api/{{ resource.endpoint }}/{id}", handleGet{{ resource.name }}(c.{{ resource.name }}))
{%- endif %}
{%- if "create" in resource.operations %}
	mux.HandleFunc("POST /api/{{ resource.endpoint }}", handleCreate{{ resource.name }}(c.{{ resource.name }}))
{%- endif %}
{%- if "update" in resource.operations %}
	mux.HandleFunc("PATCH /api/{{ resource.endpoint }}/{id}", handleUpdate{{ resource.name }}(c.{{ resource.name }}))
{%- endif %}
{%- if "replace" in resource.operations %}
	mux.HandleFunc("PUT /api/{{ resource.endpoint }}/{id}", handleReplace{{ resource.name }}(c.{{ resource.name }}))
{%- endif %}
{%- if "delete" in resource.operations %}
	mux.HandleFunc("DELETE /api/{{ resource.endpoint }}/{id}", handleDelete{{ resource.name }}(c.{{ resource.name }}))
{%- endif %}
{%- endfor %}

	log.Printf("About to listen on %s", listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, mux))
}

{% if cloud_service == 'Azure Function App' -%}
func initControllers() appControllers {
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
	var c appControllers
{%- for resource in resources %}
	{
		containerName := os.Getenv("CosmosDbContainerName_{{ resource.container | to_camel }}")
		if containerName == "" {
			containerName = "{{ resource.container }}"
		}
		containerClient, err := client.NewContainer(databaseName, containerName)
		if err != nil {
			log.Fatalf("Failed to get Cosmos DB container %s: %v", containerName, err)
		}
		c.{{ resource.name }} = controllers.New{{ resource.name }}Controller(services.New{{ resource.name }}Service(repositories.New{{ resource.name }}Repository(containerClient)), validator)
	}
{%- endfor %}

	return c
}
{%- elif cloud_service == 'GCP Cloud Function' -%}
func initControllers() (appControllers, func()) {
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
	var c appControllers
{%- for resource in resources %}
	{
		collectionName := os.Getenv("FIRESTORE_COLLECTION_{{ resource.container | to_camel }}")
		if collectionName == "" {
			collectionName = "{{ resource.container }}"
		}
		collection := client.Collection(collectionName)
		c.{{ resource.name }} = controllers.New{{ resource.name }}Controller(services.New{{ resource.name }}Service(repositories.New{{ resource.name }}Repository(collection)), validator)
	}
{%- endfor %}

	return c, func() { client.Close() }
}
{%- endif %}

func handleHealth() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"status": "ok"})
	}
}
{%- for resource in resources %}
{%- if "get_by_id" in resource.operations %}

func handleGet{{ resource.name }}(controller controllers.{{ resource.name }}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Get{{ resource.name }} processed a request.")

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Get{{ resource.name }}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if "list" in resource.operations %}

func handleGet{{ resource.name }}List(controller controllers.{{ resource.name }}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Get{{ resource.name }}List processed a request.")

		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		result, err := controller.GetList(r.Context(), limit)
		if err != nil {
			log.Printf("Exception in Get{{ resource.name }}List: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if "create" in resource.operations %}

func handleCreate{{ resource.name }}(controller controllers.{{ resource.name }}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Create{{ resource.name }} processed a request.")

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in Create{{ resource.name }}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if "update" in resource.operations %}

func handleUpdate{{ resource.name }}(controller controllers.{{ resource.name }}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Update{{ resource.name }} processed a request.")

		result, err := controller.Update(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in Update{{ resource.name }}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if "replace" in resource.operations %}

func handleReplace{{ resource.name }}(controller controllers.{{ resource.name }}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Replace{{ resource.name }} processed a request.")

		result, err := controller.Replace(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in Replace{{ resource.name }}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}
{%- endif %}
{%- if "delete" in resource.operations %}

func handleDelete{{ resource.name }}(controller controllers.{{ resource.name }}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Delete{{ resource.name }} processed a request.")

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Delete{{ resource.name }}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("{{ resource.name }} with id %s was deleted successfully.", id),
		})
	}
}
{%- endif %}
{%- endfor %}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
{%- for resource in resources %}

var {{ resource.name | to_lower_camel }}Controller controllers.{{ resource.name }}Controller
{%- endfor %}

func init() {
	cfg, err := awsconfig.LoadDefaultConfig(context.Background())
	if err != nil {
		log.Fatalf("Failed to load AWS config: %v", err)
	}

	client := dynamodb.NewFromConfig(cfg)
	validator := newValidator()
{%- for resource in resources %}
	{
		tableName := os.Getenv("DYNAMODB_TABLE_NAME_{{ resource.container | upper | replace('-', '_') }}")
		if tableName == "" {
			tableName = "{{ resource.container }}"
		}
		{{ resource.name | to_lower_camel }}Controller = controllers.New{{ resource.name }}Controller(services.New{{ resource.name }}Service(repositories.New{{ resource.name }}Repository(client, tableName)), validator)
	}
{%- endfor %}
}

func main() {
	lambda.Start(handler)
}

func jsonResponse(statusCode int, body any) (events.APIGatewayProxyResponse, error) {
	data, _ := json.Marshal(body)
	return events.APIGatewayProxyResponse{
		StatusCode: statusCode,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(data),
	}, nil
}

func handleHealth() (events.APIGatewayProxyResponse, error) {
	return jsonResponse(200, map[string]string{"status": "ok"})
}

func handler(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	log.Printf("Received %s request for %s", request.HTTPMethod, request.Resource)

	if request.HTTPMethod == "GET" && (strings.HasSuffix(strings.TrimRight(request.Path, "/"), "/health") || strings.HasSuffix(strings.TrimRight(request.Resource, "/"), "/health")) {
		return handleHealth()
	}

	// ``request.Resource`` is the API Gateway route template (e.g. "/cats/{item_id}"),
	// so the first path segment is the literal endpoint and "{item_id}" marks an
	// id-bearing route.
	trimmed := strings.Trim(request.Resource, "/")
	endpoint := trimmed
	if idx := strings.Index(trimmed, "/"); idx >= 0 {
		endpoint = trimmed[:idx]
	}
	needsID := strings.Contains(request.Resource, "{item_id}")

	switch endpoint {
{%- for resource in resources %}
	case "{{ resource.endpoint }}":
		return handle{{ resource.name }}(ctx, request, needsID)
{%- endfor %}
	default:
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
}
{%- for resource in resources %}
{%- set r = resource.name %}
{%- set lc = resource.name | to_lower_camel %}

func handle{{ r }}(ctx context.Context, request events.APIGatewayProxyRequest, needsID bool) (events.APIGatewayProxyResponse, error) {
	switch request.HTTPMethod {
	case "GET":
		if needsID {
{%- if "get_by_id" in resource.operations %}
			result, err := {{ lc }}Controller.Get(ctx, request.PathParameters["item_id"])
			if err != nil {
				log.Printf("Exception in Get{{ r }}: %v", err)
				return utils.DetectError(err), nil
			}
			return jsonResponse(200, result)
{%- else %}
			return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
{%- endif %}
		}
{%- if "list" in resource.operations %}
		limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
		result, err := {{ lc }}Controller.GetList(ctx, limit)
		if err != nil {
			log.Printf("Exception in Get{{ r }}List: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
{%- else %}
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
{%- endif %}
	case "POST":
{%- if "create" in resource.operations %}
		result, err := {{ lc }}Controller.Create(ctx, strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in Create{{ r }}: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(201, result)
{%- else %}
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
{%- endif %}
	case "PATCH":
{%- if "update" in resource.operations %}
		result, err := {{ lc }}Controller.Update(ctx, request.PathParameters["item_id"], strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in Update{{ r }}: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
{%- else %}
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
{%- endif %}
	case "PUT":
{%- if "replace" in resource.operations %}
		result, err := {{ lc }}Controller.Replace(ctx, request.PathParameters["item_id"], strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in Replace{{ r }}: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
{%- else %}
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
{%- endif %}
	case "DELETE":
{%- if "delete" in resource.operations %}
		if err := {{ lc }}Controller.Delete(ctx, request.PathParameters["item_id"]); err != nil {
			log.Printf("Exception in Delete{{ r }}: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, map[string]string{
			"message": fmt.Sprintf("{{ r }} with id %s was deleted successfully.", request.PathParameters["item_id"]),
		})
{%- else %}
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
{%- endif %}
	default:
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	}
}
{%- endfor %}
{%- endif %}
