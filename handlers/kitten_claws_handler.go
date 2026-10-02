package handlers

import (
	"context"
	"net/http"
	"strconv"
	"strings"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/controllers"
)

func RegisterKittenClawsRoutes(router *Router, controller controllers.KittenClawsController) {
	handler := handleKittenClaws(controller)
	router.Handle("/kittenclaws", handler)
	router.Handle("/kittenclaws/{id}", handler)
}

func handleKittenClaws(controller controllers.KittenClawsController) RouteHandler {
	return func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		id := request.PathParameters["id"]

		switch request.HTTPMethod + " " + request.Resource {
		case "GET /kittenclaws":
			limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.GetList(ctx, limit) })
		case "GET /kittenclaws/{id}":
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.Get(ctx, id) })
		case "POST /kittenclaws":
			return serve(ctx, http.StatusCreated, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Create(ctx, userID, strings.NewReader(request.Body))
			}))
		case "PATCH /kittenclaws/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Update(ctx, id, userID, strings.NewReader(request.Body))
			}))
		case "DELETE /kittenclaws/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Delete(ctx, id, userID)
			}))
		default:
			return methodNotAllowed()
		}
	}
}
