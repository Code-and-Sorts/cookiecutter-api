package handlers

import (
	"context"
	"log/slog"
	"net/http"
	"strings"
	"time"
	"unicode/utf8"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/models"
	"kittenclaws/utils"
)

// Covers every database call and SDK retry, so a failing database answers 500 inside the platform timeout.
const RequestTimeout = 8 * time.Second

const (
	UserIDHeader         = "X-User-Id"
	MaxUserIDLength      = 256
	UserIDTooLongMessage = "X-User-Id must be at most 256 characters."
)

// Checked before the body is read, so an oversized header never reaches the controller or the database.
func withUserID(headers map[string]string, call func(userID string) (any, error)) func() (any, error) {
	return func() (any, error) {
		var userID string
		// API Gateway keeps the client's header casing.
		for name, value := range headers {
			if strings.EqualFold(name, UserIDHeader) {
				userID = value
			}
		}
		userID = strings.TrimSpace(userID)
		if utf8.RuneCountInString(userID) > MaxUserIDLength {
			return nil, &models.ValidationError{Message: UserIDTooLongMessage}
		}
		return call(userID)
	}
}

type RouteHandler func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse

type Router struct {
	routes map[string]RouteHandler
}

func NewRouter() *Router {
	return &Router{routes: make(map[string]RouteHandler)}
}

func (router *Router) Handle(resource string, handler RouteHandler) {
	router.routes[resource] = handler
}

func (router *Router) ServeRequest(ctx context.Context, request events.APIGatewayProxyRequest) (response events.APIGatewayProxyResponse, err error) {
	slog.Info("Processing request", "method", request.HTTPMethod, "path", request.Path)
	ctx, cancel := context.WithTimeout(ctx, RequestTimeout)
	defer cancel()
	defer func() {
		if recovered := recover(); recovered != nil {
			utils.LogUnexpected(recovered)
			response = utils.GenerateErrorResponse(utils.UnexpectedErrorMessage, http.StatusInternalServerError)
		}
	}()

	handler, ok := router.routes[request.Resource]
	if !ok {
		return utils.GenerateErrorResponse(utils.NotFoundMessage, http.StatusNotFound), nil
	}
	return handler(ctx, request), nil
}

func serve(ctx context.Context, status int, call func() (any, error)) events.APIGatewayProxyResponse {
	result, err := call()
	if err != nil {
		return utils.DetectError(ctx, err)
	}
	return utils.JSONResponse(status, result)
}

func methodNotAllowed() events.APIGatewayProxyResponse {
	return utils.GenerateErrorResponse(utils.MethodNotAllowedMessage, http.StatusMethodNotAllowed)
}
