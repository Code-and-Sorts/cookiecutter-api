package handlers

import (
	"context"
	"net/http"
	"strconv"
	"strings"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/controllers"
)

func RegisterDogRoutes(router *Router, controller controllers.DogController) {
	handler := handleDog(controller)
	router.Handle("/dogs", handler)
	router.Handle("/dogs/{id}", handler)
}

func handleDog(controller controllers.DogController) RouteHandler {
	return func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		id := request.PathParameters["id"]

		switch request.HTTPMethod + " " + request.Resource {
		case "GET /dogs":
			limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.GetList(ctx, limit) })
		case "GET /dogs/{id}":
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.Get(ctx, id) })
		case "POST /dogs":
			return serve(ctx, http.StatusCreated, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Create(ctx, userID, strings.NewReader(request.Body))
			}))
		case "PUT /dogs/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Replace(ctx, id, userID, strings.NewReader(request.Body))
			}))
		case "DELETE /dogs/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Delete(ctx, id, userID)
			}))
		default:
			return methodNotAllowed()
		}
	}
}
