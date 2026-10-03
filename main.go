package main

import (
	"context"
	"log/slog"
	"os"

	"github.com/aws/aws-lambda-go/lambda"
	awsconfig "github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"

	"kittenclaws/controllers"
	"kittenclaws/handlers"
	"kittenclaws/repositories"
	"kittenclaws/services"
	"kittenclaws/utils"
)

func main() {
	slog.SetDefault(utils.NewLogger())
	validator := newValidator()
	cfg, err := awsconfig.LoadDefaultConfig(context.Background())
	exitOnError("Failed to load AWS config", err)
	client := dynamodb.NewFromConfig(cfg)

	router := handlers.NewRouter()
	handlers.RegisterHealthRoute(router)
	{
		table := utils.Getenv("DYNAMODB_TABLE_NAME_KITTENCLAWS", "kittenclaws")
		repository := repositories.NewKittenClawsRepository(repositories.Container{Client: client, TableName: table})
		controller := controllers.NewKittenClawsController(services.NewKittenClawsService(repository), validator)
		handlers.RegisterKittenClawsRoutes(router, controller)
	}

	lambda.Start(router.ServeRequest)
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
