package handlers

import (
	"context"
	"net/http"
	"time"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}

	"{{project_endpoint}}/utils"
)

// RequestTimeout bounds each request, including every database call and the
// SDK's retries, so a failing database answers with the JSON 500 well inside
// the platform's own timeout instead of hanging.
const RequestTimeout = 8 * time.Second
{%- if cloud_service != 'AWS Lambda' %}

// NewRouter returns a ServeMux that answers any request no route matches with
// the JSON 404 body. Each Register*Route(s) function adds its routes to it.
func NewRouter() *http.ServeMux {
	mux := http.NewServeMux()
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		utils.WriteError(w, http.StatusNotFound, utils.NotFoundMessage)
	})
	return mux
}

// methodNotAllowed answers a known path requested with a method it does not
// serve. Routes register it on their paths without a method, so it only runs
// when no method-specific pattern matches.
func methodNotAllowed(w http.ResponseWriter, r *http.Request) {
	utils.WriteError(w, http.StatusMethodNotAllowed, utils.MethodNotAllowedMessage)
}

// WithRequestTimeout gives each request's context the RequestTimeout deadline.
func WithRequestTimeout(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		ctx, cancel := context.WithTimeout(r.Context(), RequestTimeout)
		defer cancel()
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

// Recover answers a panic in next with the JSON 500 body instead of dropping
// the connection.
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

// RouteHandler serves the API Gateway requests for one resource path.
type RouteHandler func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse

// Router dispatches API Gateway requests by their exact resource path, such as
// "/cats" or "/cats/{id}".
type Router struct {
	routes map[string]RouteHandler
}

// NewRouter returns an empty Router. Each Register*Route(s) function adds its
// routes to it.
func NewRouter() *Router {
	return &Router{routes: make(map[string]RouteHandler)}
}

// Handle registers handler for the API Gateway resource path.
func (router *Router) Handle(resource string, handler RouteHandler) {
	router.routes[resource] = handler
}

// ServeRequest is the Lambda handler. Each request gets the RequestTimeout
// deadline, unknown resource paths get the JSON 404 body, and a panic gets the
// JSON 500 body.
func (router *Router) ServeRequest(ctx context.Context, request events.APIGatewayProxyRequest) (response events.APIGatewayProxyResponse, err error) {
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

// methodNotAllowed answers a known path requested with a method it does not
// serve.
func methodNotAllowed() events.APIGatewayProxyResponse {
	return utils.GenerateErrorResponse(utils.MethodNotAllowedMessage, http.StatusMethodNotAllowed)
}
{%- endif %}
