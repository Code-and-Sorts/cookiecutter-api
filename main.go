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

var catController controllers.CatController

var dogController controllers.DogController

func init() {
	cfg, err := awsconfig.LoadDefaultConfig(context.Background())
	if err != nil {
		log.Fatalf("Failed to load AWS config: %v", err)
	}

	client := dynamodb.NewFromConfig(cfg)
	validator := newValidator()
	{
		tableName := os.Getenv("DYNAMODB_TABLE_NAME_ANIMALS")
		if tableName == "" {
			tableName = "animals"
		}
		catController = controllers.NewCatController(services.NewCatService(repositories.NewCatRepository(client, tableName)), validator)
	}
	{
		tableName := os.Getenv("DYNAMODB_TABLE_NAME_ANIMALS")
		if tableName == "" {
			tableName = "animals"
		}
		dogController = controllers.NewDogController(services.NewDogService(repositories.NewDogRepository(client, tableName)), validator)
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
	case "cats":
		return handleCat(ctx, request, needsID)
	case "dogs":
		return handleDog(ctx, request, needsID)
	default:
		return utils.GenerateErrorResponse("Not Found", 404), nil
	}
}

func handleCat(ctx context.Context, request events.APIGatewayProxyRequest, needsID bool) (events.APIGatewayProxyResponse, error) {
	switch request.HTTPMethod {
	case "GET":
		if needsID {
			result, err := catController.Get(ctx, request.PathParameters["item_id"])
			if err != nil {
				log.Printf("Exception in GetCat: %v", err)
				return utils.DetectError(err), nil
			}
			return jsonResponse(200, result)
		}
		limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
		result, err := catController.GetList(ctx, limit)
		if err != nil {
			log.Printf("Exception in GetCatList: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
	case "POST":
		result, err := catController.Create(ctx, strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in CreateCat: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(201, result)
	case "PATCH":
		result, err := catController.Update(ctx, request.PathParameters["item_id"], strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in UpdateCat: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
	case "PUT":
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	case "DELETE":
		if err := catController.Delete(ctx, request.PathParameters["item_id"]); err != nil {
			log.Printf("Exception in DeleteCat: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, map[string]string{
			"message": fmt.Sprintf("Cat with id %s was deleted successfully.", request.PathParameters["item_id"]),
		})
	default:
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	}
}

func handleDog(ctx context.Context, request events.APIGatewayProxyRequest, needsID bool) (events.APIGatewayProxyResponse, error) {
	switch request.HTTPMethod {
	case "GET":
		if needsID {
			result, err := dogController.Get(ctx, request.PathParameters["item_id"])
			if err != nil {
				log.Printf("Exception in GetDog: %v", err)
				return utils.DetectError(err), nil
			}
			return jsonResponse(200, result)
		}
		limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
		result, err := dogController.GetList(ctx, limit)
		if err != nil {
			log.Printf("Exception in GetDogList: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
	case "POST":
		result, err := dogController.Create(ctx, strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in CreateDog: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(201, result)
	case "PATCH":
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	case "PUT":
		result, err := dogController.Replace(ctx, request.PathParameters["item_id"], strings.NewReader(request.Body))
		if err != nil {
			log.Printf("Exception in ReplaceDog: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, result)
	case "DELETE":
		if err := dogController.Delete(ctx, request.PathParameters["item_id"]); err != nil {
			log.Printf("Exception in DeleteDog: %v", err)
			return utils.DetectError(err), nil
		}
		return jsonResponse(200, map[string]string{
			"message": fmt.Sprintf("Dog with id %s was deleted successfully.", request.PathParameters["item_id"]),
		})
	default:
		return utils.GenerateErrorResponse("Method Not Allowed", 405), nil
	}
}
