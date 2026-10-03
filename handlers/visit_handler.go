package handlers

import (
	"context"
	"net/http"
	"strings"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/controllers"
)

func RegisterVisitRoutes(router *Router, controller controllers.VisitController) {
	handler := handleVisit(controller)
	router.Handle("/visits", handler)
	router.Handle("/visits/{id}", handler)
}

func handleVisit(controller controllers.VisitController) RouteHandler {
	return func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		id := request.PathParameters["id"]

		switch request.HTTPMethod + " " + request.Resource {
		case "GET /visits/{id}":
			return serve(ctx, http.StatusOK, func() (any, error) { return controller.Get(ctx, id) })
		case "POST /visits":
			return serve(ctx, http.StatusCreated, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Create(ctx, userID, strings.NewReader(request.Body))
			}))
		case "PATCH /visits/{id}":
			return serve(ctx, http.StatusOK, withUserID(request.Headers, func(userID string) (any, error) {
				return controller.Update(ctx, id, userID, strings.NewReader(request.Body))
			}))
		default:
			return methodNotAllowed()
		}
	}
}
