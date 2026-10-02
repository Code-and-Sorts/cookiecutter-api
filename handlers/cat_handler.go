package handlers

import (
	"context"
	"net/http"
	"strconv"
	"strings"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/controllers"
)

func RegisterCatRoutes(router *Router, controller controllers.CatController) {
	handler := handleCat(controller)
	router.Handle("/cats", handler)
	router.Handle("/cats/{id}", handler)
}

func handleCat(controller controllers.CatController) RouteHandler {
	return func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		id := request.PathParameters["id"]

		switch request.HTTPMethod + " " + request.Resource {
		case "GET /cats":
			limit, _ := strconv.Atoi(request.QueryStringParameters["limit"])
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.GetList(ctx, limit) })
		case "GET /cats/{id}":
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.Get(ctx, id) })
		case "POST /cats":
			return serve(ctx, http.StatusCreated, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Create(ctx, userID, strings.NewReader(request.Body))
			}))
		case "PATCH /cats/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Update(ctx, id, userID, strings.NewReader(request.Body))
			}))
		case "DELETE /cats/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Delete(ctx, id, userID)
			}))
		default:
			return methodNotAllowed()
		}
	}
}
