package handlers

import (
	"context"
	"net/http"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/utils"
)

func RegisterHealthRoute(router *Router) {
	router.Handle("/health", handleHealth)
}

func handleHealth(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
	if request.HTTPMethod != http.MethodGet {
		return methodNotAllowed()
	}
	return utils.JSONResponse(http.StatusOK, map[string]string{"status": "ok"})
}
