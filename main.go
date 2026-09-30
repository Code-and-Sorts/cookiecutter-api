package main

import (
	"errors"
	"log/slog"
	"net/http"
	"os"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/azcore/policy"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

	"kittenclaws/controllers"
	"kittenclaws/handlers"
	"kittenclaws/repositories"
	"kittenclaws/services"
	"kittenclaws/utils"
)

func main() {
	slog.SetDefault(utils.NewLogger())
	validator := newValidator()
	client, databaseName := newCosmosClient()

	router := handlers.NewRouter()
	handlers.RegisterHealthRoute(router)
	{
		container := cosmosContainer(client, databaseName, "CosmosDbContainerName_Animals", "animals")
		repository := repositories.NewCatRepository(container)
		controller := controllers.NewCatController(services.NewCatService(repository), validator)
		handlers.RegisterCatRoutes(router, controller)
	}
	{
		container := cosmosContainer(client, databaseName, "CosmosDbContainerName_Animals", "animals")
		repository := repositories.NewDogRepository(container)
		controller := controllers.NewDogController(services.NewDogService(repository), validator)
		handlers.RegisterDogRoutes(router, controller)
	}

	// The Functions host forwards requests to the port it passes in FUNCTIONS_CUSTOMHANDLER_PORT.
	listenAddr := ":" + utils.Getenv("FUNCTIONS_CUSTOMHANDLER_PORT", "80")

	slog.Info("Listening for requests", "address", listenAddr)
	exitOnError("Server stopped", http.ListenAndServe(listenAddr, handlers.Middleware(router)))
}

func newCosmosClient() (*azcosmos.Client, string) {
	endpoint := os.Getenv("CosmosDbEndpoint")
	key := os.Getenv("CosmosDbKey")
	databaseName := os.Getenv("CosmosDbDatabaseName")
	if endpoint == "" || key == "" || databaseName == "" {
		exitOnError("Missing settings", errors.New("CosmosDbEndpoint, CosmosDbKey and CosmosDbDatabaseName are required"))
	}

	cred, err := azcosmos.NewKeyCredential(key)
	exitOnError("Failed to create Cosmos DB credential", err)

	// The SDK's first account read ignores the request deadline and could wait a minute on an unreachable Cosmos DB.
	options := &azcosmos.ClientOptions{ClientOptions: azcore.ClientOptions{
		Retry: policy.RetryOptions{
			MaxRetries: 1,
			TryTimeout: 3 * time.Second,
			RetryDelay: 500 * time.Millisecond,
		},
	}}
	client, err := azcosmos.NewClientWithKey(endpoint, cred, options)
	exitOnError("Failed to create Cosmos DB client", err)

	return client, databaseName
}

func cosmosContainer(client *azcosmos.Client, databaseName, setting, defaultName string) *azcosmos.ContainerClient {
	name := utils.Getenv(setting, defaultName)
	container, err := client.NewContainer(databaseName, name)
	exitOnError("Failed to open Cosmos DB container "+name, err)
	return container
}

func newValidator() services.SchemaValidator {
	validator, err := services.NewSchemaValidator(controllers.RequestSchemas())
	exitOnError("Failed to initialize schema validator", err)
	return validator
}

func exitOnError(message string, err error) {
	if err != nil {
		slog.Error(message, "error", err)
		os.Exit(1)
	}
}
