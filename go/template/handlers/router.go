{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set need_write = all_ops | select('in', ['create', 'update', 'replace', 'delete']) | list | length > 0 -%}
package handlers

import (
	"context"
	"log/slog"
	"net/http"
{%- if need_write %}
	"strings"
{%- endif %}
	"time"
{%- if need_write %}
	"unicode/utf8"
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}
{% if need_write %}
	"{{project_endpoint}}/models"
{%- endif %}
	"{{project_endpoint}}/utils"
)

// Covers every database call and SDK retry, so a failing database answers 500 inside the platform timeout.
const RequestTimeout = 8 * time.Second
{%- if need_write %}

const (
	UserIDHeader         = "X-User-Id"
	MaxUserIDLength      = 256
	UserIDTooLongMessage = "X-User-Id must be at most 256 characters."
)

// Checked before the body is read, so an oversized header never reaches the controller or the database.
func withUserID({% if cloud_service == 'AWS Lambda' %}headers map[string]string{% else %}header http.Header{% endif %}, call func(userID string) (any, error)) func() (any, error) {
	return func() (any, error) {
{%- if cloud_service == 'AWS Lambda' %}
		var userID string
		// API Gateway keeps the client's header casing.
		for name, value := range headers {
			if strings.EqualFold(name, UserIDHeader) {
				userID = value
			}
		}
		userID = strings.TrimSpace(userID)
{%- else %}
		userID := strings.TrimSpace(header.Get(UserIDHeader))
{%- endif %}
		if utf8.RuneCountInString(userID) > MaxUserIDLength {
			return nil, &models.ValidationError{Message: UserIDTooLongMessage}
		}
		return call(userID)
	}
}
{%- endif %}
{%- if cloud_service != 'AWS Lambda' %}

// Satisfied by *http.ServeMux; the OpenAPI contract test passes a recorder to list the routes.
type Mux interface {
	HandleFunc(pattern string, handler func(http.ResponseWriter, *http.Request))
}

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
