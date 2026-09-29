package main

import (
{%- if cloud_service == 'GCP Cloud Function' or cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"log"
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}
	"net/http"
{%- endif %}
	"os"
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
	"{{project_endpoint}}/handlers"
	"{{project_endpoint}}/repositories"
	"{{project_endpoint}}/services"
{%- if cloud_service == 'AWS Lambda' %}
	"{{project_endpoint}}/utils"
{%- endif %}
)

func newValidator() services.SchemaValidator {
	validator, err := services.NewSchemaValidator(controllers.RequestSchemas())
	if err != nil {
		log.Fatalf("Failed to initialize schema validator: %v", err)
	}
	return validator
}
{%- if cloud_service == 'Azure Function App' or cloud_service == 'GCP Cloud Function' %}

type appControllers struct {
{%- set name_width = resources | map(attribute='name') | map('length') | max %}
{%- for resource in resources %}
	{{ resource.name ~ ' ' * (name_width - resource.name | length) }} controllers.{{ resource.name }}Controller
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

	mux.HandleFunc("GET /api/health", handlers.HandleHealth())
{%- for resource in resources %}
	handlers.Register{{ resource.name }}Routes(mux, c.{{ resource.name }})
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
		collectionName := os.Getenv("FIRESTORE_COLLECTION_{{ resource.container | upper | replace('-', '_') }}")
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

func handler(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	log.Printf("Received %s request for %s", request.HTTPMethod, request.Resource)

	if request.HTTPMethod == "GET" && (strings.HasSuffix(strings.TrimRight(request.Path, "/"), "/health") || strings.HasSuffix(strings.TrimRight(request.Resource, "/"), "/health")) {
		return handlers.HandleHealth()
	}

	trimmed := strings.Trim(request.Resource, "/")
	endpoint := trimmed
	if idx := strings.Index(trimmed, "/"); idx >= 0 {
		endpoint = trimmed[:idx]
	}

	switch endpoint {
{%- for resource in resources %}
	case "{{ resource.endpoint }}":
		return handlers.Handle{{ resource.name }}(ctx, request, {{ resource.name | to_lower_camel }}Controller)
{%- endfor %}
	default:
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
}
{%- endif %}
