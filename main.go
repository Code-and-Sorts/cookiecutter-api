package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"strconv"
	"strings"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	awsconfig "github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"

	"kitties/controllers"
	"kitties/repositories"
	"kitties/services"
	"kitties/utils"
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

var kittenClawsController controllers.KittenClawsController

func init() {
	cfg, err := awsconfig.LoadDefaultConfig(context.Background())
	if err != nil {
		log.Fatalf("Failed to load AWS config: %v", err)
	}

	client := dynamodb.NewFromConfig(cfg)
	validator := newValidator()
	{
		tableName := os.Getenv("DYNAMODB_TABLE_NAME_KITTIES")
		if tableName == "" {
			tableName = "kitties"
		}
		kittenClawsController = controllers.NewKittenClawsController(services.NewKittenClawsService(repositories.NewKittenClawsRepository(client, tableName)), validator)
	}
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

	trimmed := strings.Trim(request.Resource, "/")
	endpoint := trimmed
	if idx := strings.Index(trimmed, "/"); idx >= 0 {
		endpoint = trimmed[:idx]
	}
	needsID := strings.Contains(request.Resource, "{item_id}")

	switch endpoint {
	case "kitties":
		return handleKittenClaws(ctx, request, needsID)
	default:
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
}

func handleKittenClaws(ctx context.Context, request events.APIGatewayProxyRequest, needsID bool) (events.APIGatewayProxyResponse, error) {
	switch request.HTTPMethod {
	case "GET":
		if needsID {
			result, err := kittenClawsController.Get(ctx, request.PathParameters["item_id"])
			if err != nil {
				log.Printf("Exception in GetKittenClaws: %v", err)
				return utils.DetectError(err), nil
			}
			return jsonResponse(200, result)
		}
		limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
		result, err := kittenClawsController.GetList(ctx, limit)
		if err != nil {
			log.Printf("Exception in GetKittenClawsList: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
	case "POST":
		result, err := kittenClawsController.Create(ctx, strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in CreateKittenClaws: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(201, result)
	case "PATCH":
		result, err := kittenClawsController.Update(ctx, request.PathParameters["item_id"], strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in UpdateKittenClaws: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
	case "PUT":
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	case "DELETE":
		if err := kittenClawsController.Delete(ctx, request.PathParameters["item_id"]); err != nil {
			log.Printf("Exception in DeleteKittenClaws: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, map[string]string{
			"message": fmt.Sprintf("KittenClaws with id %s was deleted successfully.", request.PathParameters["item_id"]),
		})
	default:
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	}
}
