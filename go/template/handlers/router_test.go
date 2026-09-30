package handlers

import (
{%- if cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"io"
	"log/slog"
	"net/http"
{%- if cloud_service != 'AWS Lambda' %}
	"net/http/httptest"
{%- endif %}
	"testing"
	"time"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}

	"github.com/stretchr/testify/assert"
)

// discardLogs silences the default logger for the rest of the test.
func discardLogs(t *testing.T) {
	t.Helper()
	previous := slog.Default()
	slog.SetDefault(slog.New(slog.NewTextHandler(io.Discard, nil)))
	t.Cleanup(func() { slog.SetDefault(previous) })
}
{%- if cloud_service != 'AWS Lambda' %}

func TestNewRouter_UnknownPath_ReturnsJSON404(t *testing.T) {
	w := httptest.NewRecorder()

	NewRouter().ServeHTTP(w, httptest.NewRequest(http.MethodGet, "/unknown", nil))

	assert.Equal(t, http.StatusNotFound, w.Code)
	assert.Equal(t, "application/json", w.Header().Get("Content-Type"))
	assert.JSONEq(t, `{"errorMessage": "Not found."}`, w.Body.String())
}

func TestWithRequestTimeout_SetsDeadline(t *testing.T) {
	var deadline time.Time
	var hasDeadline bool
	handler := WithRequestTimeout(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		deadline, hasDeadline = r.Context().Deadline()
	}))

	handler.ServeHTTP(httptest.NewRecorder(), httptest.NewRequest(http.MethodGet, "/", nil))

	assert.True(t, hasDeadline)
	assert.WithinDuration(t, time.Now().Add(RequestTimeout), deadline, time.Second)
}

func TestRecover_Panic_ReturnsJSON500(t *testing.T) {
	discardLogs(t)
	handler := Recover(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		panic("boom")
	}))
	w := httptest.NewRecorder()

	handler.ServeHTTP(w, httptest.NewRequest(http.MethodGet, "/", nil))

	assert.Equal(t, http.StatusInternalServerError, w.Code)
	assert.JSONEq(t, `{"errorMessage": "An unexpected error occurred."}`, w.Body.String())
}

func TestRecover_AbortHandler_IsNotSwallowed(t *testing.T) {
	handler := Recover(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		panic(http.ErrAbortHandler)
	}))

	assert.PanicsWithValue(t, http.ErrAbortHandler, func() {
		handler.ServeHTTP(httptest.NewRecorder(), httptest.NewRequest(http.MethodGet, "/", nil))
	})
}
{%- else %}

func TestRouter_UnknownResource_ReturnsJSON404(t *testing.T) {
	response, err := NewRouter().ServeRequest(context.Background(), events.APIGatewayProxyRequest{
		HTTPMethod: http.MethodGet,
		Resource:   "/unknown",
	})

	assert.NoError(t, err)
	assert.Equal(t, http.StatusNotFound, response.StatusCode)
	assert.Equal(t, "application/json", response.Headers["Content-Type"])
	assert.JSONEq(t, `{"errorMessage": "Not found."}`, response.Body)
}

func TestRouter_SetsRequestDeadline(t *testing.T) {
	var deadline time.Time
	var hasDeadline bool
	router := NewRouter()
	router.Handle("/items", func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		deadline, hasDeadline = ctx.Deadline()
		return methodNotAllowed()
	})

	_, err := router.ServeRequest(context.Background(), events.APIGatewayProxyRequest{HTTPMethod: http.MethodGet, Resource: "/items"})

	assert.NoError(t, err)
	assert.True(t, hasDeadline)
	assert.WithinDuration(t, time.Now().Add(RequestTimeout), deadline, time.Second)
}

func TestRouter_Panic_ReturnsJSON500(t *testing.T) {
	discardLogs(t)
	router := NewRouter()
	router.Handle("/boom", func(ctx context.Context, request events.APIGatewayProxyRequest) events.APIGatewayProxyResponse {
		panic("boom")
	})

	response, err := router.ServeRequest(context.Background(), events.APIGatewayProxyRequest{
		HTTPMethod: http.MethodGet,
		Resource:   "/boom",
	})

	assert.NoError(t, err)
	assert.Equal(t, http.StatusInternalServerError, response.StatusCode)
	assert.JSONEq(t, `{"errorMessage": "An unexpected error occurred."}`, response.Body)
}
{%- endif %}
