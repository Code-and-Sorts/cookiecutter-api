package main

import (
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' or cookiecutter.cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"encoding/json"
	"fmt"
	"log"
{%- if cookiecutter.cloud_service == 'Azure Function App' or cookiecutter.cloud_service == 'GCP Cloud Function' %}
	"net/http"
{%- endif %}
	"os"
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
	"strings"
{%- endif %}
{% if cookiecutter.cloud_service == 'Azure Function App' %}
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
	"cloud.google.com/go/firestore"
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	awsconfig "github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
{%- endif %}

	"{{cookiecutter.project_endpoint}}/controllers"
	"{{cookiecutter.project_endpoint}}/repositories"
	"{{cookiecutter.project_endpoint}}/services"
	"{{cookiecutter.project_endpoint}}/utils"
)
{% if cookiecutter.cloud_service == 'Azure Function App' or cookiecutter.cloud_service == 'GCP Cloud Function' %}
func main() {
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	listenAddr := ":80"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}

	controller := initController()
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' %}
	listenAddr := ":8080"
	if val, ok := os.LookupEnv("PORT"); ok {
		listenAddr = ":" + val
	}

	controller, cleanup := initController()
	defer cleanup()
{%- endif %}

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/{{cookiecutter.project_endpoint}}/{id}", handleGet(controller))
	mux.HandleFunc("GET /api/{{cookiecutter.project_endpoint}}", handleGetList(controller))
	mux.HandleFunc("POST /api/{{cookiecutter.project_endpoint}}", handleCreate(controller))
	mux.HandleFunc("PATCH /api/{{cookiecutter.project_endpoint}}/{id}", handleUpdate(controller))
	mux.HandleFunc("DELETE /api/{{cookiecutter.project_endpoint}}/{id}", handleDelete(controller))

	log.Printf("About to listen on %s", listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, mux))
}

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
func initController() controllers.{{cookiecutter.project_class_name}}Controller {
	endpoint := os.Getenv("CosmosDbEndpoint")
	key := os.Getenv("CosmosDbKey")
	databaseName := os.Getenv("CosmosDbDatabaseName")
	containerName := os.Getenv("CosmosDbContainerName")

	cred, err := azcosmos.NewKeyCredential(key)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB credential: %v", err)
	}

	client, err := azcosmos.NewClientWithKey(endpoint, cred, nil)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB client: %v", err)
	}

	container, err := client.NewContainer(databaseName, containerName)
	if err != nil {
		log.Fatalf("Failed to get Cosmos DB container: %v", err)
	}

	repo := repositories.New{{cookiecutter.project_class_name}}Repository(container)
	svc := services.New{{cookiecutter.project_class_name}}Service(repo)
	validator, err := services.NewSchemaValidator(map[string]string{
		"create_request": controllers.CreateRequestSchema,
		"update_request": controllers.UpdateRequestSchema,
	})
	if err != nil {
		log.Fatalf("Failed to initialize schema validator: %v", err)
	}
	ctrl := controllers.New{{cookiecutter.project_class_name}}Controller(svc, validator)

	return ctrl
}
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
func initController() (controllers.{{cookiecutter.project_class_name}}Controller, func()) {
	projectID := os.Getenv("GCP_PROJECT_ID")
	if projectID == "" {
		log.Fatal("GCP_PROJECT_ID environment variable is required")
	}
	databaseName := os.Getenv("FIRESTORE_DATABASE")
	if databaseName == "" {
		databaseName = "(default)"
	}
	collectionName := os.Getenv("FIRESTORE_COLLECTION")
	if collectionName == "" {
		log.Fatal("FIRESTORE_COLLECTION environment variable is required")
	}

	ctx := context.Background()
	client, err := firestore.NewClientWithDatabase(ctx, projectID, databaseName)
	if err != nil {
		log.Fatalf("Failed to create Firestore client: %v", err)
	}

	collection := client.Collection(collectionName)

	repo := repositories.New{{cookiecutter.project_class_name}}Repository(collection)
	svc := services.New{{cookiecutter.project_class_name}}Service(repo)
	validator, err := services.NewSchemaValidator(map[string]string{
		"create_request": controllers.CreateRequestSchema,
		"update_request": controllers.UpdateRequestSchema,
	})
	if err != nil {
		log.Fatalf("Failed to initialize schema validator: %v", err)
	}
	ctrl := controllers.New{{cookiecutter.project_class_name}}Controller(svc, validator)

	return ctrl, func() { client.Close() }
}
{%- endif %}

func handleGet(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Get{{cookiecutter.project_class_name}} processed a request.")

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Get{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleGetList(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Get{{cookiecutter.project_class_name}}List processed a request.")

		result, err := controller.GetList(r.Context())
		if err != nil {
			log.Printf("Exception in Get{{cookiecutter.project_class_name}}List: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleCreate(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Create{{cookiecutter.project_class_name}} processed a request.")

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in Create{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}

func handleUpdate(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Update{{cookiecutter.project_class_name}} processed a request.")

		result, err := controller.Update(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in Update{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleDelete(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Delete{{cookiecutter.project_class_name}} processed a request.")

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Delete{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("{{cookiecutter.project_class_name}} with id %s was deleted successfully.", id),
		})
	}
}
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
var controller controllers.{{cookiecutter.project_class_name}}Controller

func init() {
	tableName := os.Getenv("DYNAMODB_TABLE_NAME")

	cfg, err := awsconfig.LoadDefaultConfig(context.Background())
	if err != nil {
		log.Fatalf("Failed to load AWS config: %v", err)
	}

	client := dynamodb.NewFromConfig(cfg)

	repo := repositories.New{{cookiecutter.project_class_name}}Repository(client, tableName)
	svc := services.New{{cookiecutter.project_class_name}}Service(repo)
	validator, err := services.NewSchemaValidator(map[string]string{
		"create_request": controllers.CreateRequestSchema,
		"update_request": controllers.UpdateRequestSchema,
	})
	if err != nil {
		log.Fatalf("Failed to initialize schema validator: %v", err)
	}
	controller = controllers.New{{cookiecutter.project_class_name}}Controller(svc, validator)
}

func main() {
	lambda.Start(handler)
}

func handler(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	log.Printf("Received %s request for %s", request.HTTPMethod, request.Resource)

	switch request.HTTPMethod {
	case "GET":
		if id, ok := request.PathParameters["item_id"]; ok {
			return handleGet(ctx, id)
		}
		return handleGetList(ctx)
	case "POST":
		return handleCreate(ctx, request.Body)
	case "PATCH":
		id := request.PathParameters["item_id"]
		return handleUpdate(ctx, id, request.Body)
	case "DELETE":
		id := request.PathParameters["item_id"]
		return handleDelete(ctx, id)
	default:
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
}

func handleGet(ctx context.Context, id string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Get{{cookiecutter.project_class_name}} processed a request.")

	result, err := controller.Get(ctx, id)
	if err != nil {
		log.Printf("Exception in Get{{cookiecutter.project_class_name}}: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleGetList(ctx context.Context) (events.APIGatewayProxyResponse, error) {
	log.Printf("Get{{cookiecutter.project_class_name}}List processed a request.")

	result, err := controller.GetList(ctx)
	if err != nil {
		log.Printf("Exception in Get{{cookiecutter.project_class_name}}List: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleCreate(ctx context.Context, requestBody string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Create{{cookiecutter.project_class_name}} processed a request.")

	result, err := controller.Create(ctx, strings.NewReader(requestBody))
	if err != nil {
		log.Printf("Exception in Create{{cookiecutter.project_class_name}}: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 201,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleUpdate(ctx context.Context, id string, requestBody string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Update{{cookiecutter.project_class_name}} processed a request.")

	result, err := controller.Update(ctx, id, strings.NewReader(requestBody))
	if err != nil {
		log.Printf("Exception in Update{{cookiecutter.project_class_name}}: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(result)
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}

func handleDelete(ctx context.Context, id string) (events.APIGatewayProxyResponse, error) {
	log.Printf("Delete{{cookiecutter.project_class_name}} processed a request.")

	err := controller.Delete(ctx, id)
	if err != nil {
		log.Printf("Exception in Delete{{cookiecutter.project_class_name}}: %v", err)
		return utils.DetectError(err), nil
	}

	body, _ := json.Marshal(map[string]string{
		"message": fmt.Sprintf("{{cookiecutter.project_class_name}} with id %s was deleted successfully.", id),
	})
	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Headers:    map[string]string{"Content-Type": "application/json"},
		Body:       string(body),
	}, nil
}
{%- endif %}
