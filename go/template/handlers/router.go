package handlers

import (
	"context"
	"log/slog"
	"net/http"
	"time"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}

	"{{project_endpoint}}/utils"
)

// Covers every database call and SDK retry, so a failing database answers 500 inside the platform timeout.
const RequestTimeout = 8 * time.Second
{%- if cloud_service != 'AWS Lambda' %}

func NewRouter() *http.ServeMux {
	mux := http.NewServeMux()
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		utils.WriteError(w, http.StatusNotFound, utils.NotFoundMessage)
	})
	return mux
}

func serve(w http.ResponseWriter, r *http.Request, status int, call func() (any, error)) {
	result, err := call()
	if err != nil {
		utils.DetectError(r.Context(), w, err)
		return
	}
	utils.WriteJSON(w, status, result)
}

// Registered without a method, so ServeMux only runs it when no method-specific pattern matches.
func methodNotAllowed(w http.ResponseWriter, r *http.Request) {
	utils.WriteError(w, http.StatusMethodNotAllowed, utils.MethodNotAllowedMessage)
}

func Middleware(next http.Handler) http.Handler {
	return Recover(LogRequests(WithRequestTimeout(next)))
}

func LogRequests(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		slog.Info("Processing request", "method", r.Method, "path", r.URL.Path)
		next.ServeHTTP(w, r)
	})
}

func WithRequestTimeout(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		ctx, cancel := context.WithTimeout(r.Context(), RequestTimeout)
		defer cancel()
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

// Without this, net/http drops the connection on a panic instead of answering 500.
func Recover(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		defer func() {
			if recovered := recover(); recovered != nil {
				if recovered == http.ErrAbortHandler {
					panic(recovered)
				}
				utils.LogUnexpected(recovered)
				utils.WriteError(w, http.StatusInternalServerError, utils.UnexpectedErrorMessage)
			}
		}()
		next.ServeHTTP(w, r)
	})
}
{%- else %}

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
{%- endif %}
