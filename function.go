// Package function is the Cloud Run function; deploy it with --entry-point api.
package function

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"

	"cloud.google.com/go/firestore"
	"github.com/GoogleCloudPlatform/functions-framework-go/functions"

	"kittenclaws/controllers"
	"kittenclaws/handlers"
	"kittenclaws/repositories"
	"kittenclaws/services"
	"kittenclaws/utils"
)

func init() {
	slog.SetDefault(utils.NewLogger())
	functions.HTTP("api", newAPI().ServeHTTP)
}

func newAPI() http.Handler {
	validator, err := services.NewSchemaValidator(controllers.RequestSchemas())
	exitOnError("Failed to initialize schema validator", err)
	client := newFirestoreClient()

	var resources handlers.Controllers
	{
		collection := client.Collection(utils.Getenv("FIRESTORE_COLLECTION_KITTENCLAWS", "kittenclaws"))
		repository := repositories.NewKittenClawsRepository(collection)
		resources.KittenClaws = controllers.NewKittenClawsController(services.NewKittenClawsService(repository), validator)
	}

	router := handlers.NewRouter()
	handlers.RegisterRoutes(router, resources)
	return handlers.Middleware(router)
}

func newFirestoreClient() *firestore.Client {
	projectID := os.Getenv("GCP_PROJECT_ID")
	if projectID == "" {
		exitOnError("Missing settings", errors.New("GCP_PROJECT_ID is required"))
	}
	databaseName := utils.Getenv("FIRESTORE_DATABASE", firestore.DefaultDatabaseID)

	client, err := firestore.NewClientWithDatabase(context.Background(), projectID, databaseName)
	exitOnError("Failed to create Firestore client", err)
	return client
}

func exitOnError(message string, err error) {
	if err != nil {
		slog.Error(message, "error", err)
		os.Exit(1)
	}
}
