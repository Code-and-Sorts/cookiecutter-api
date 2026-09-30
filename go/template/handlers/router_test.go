{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set need_write = all_ops | select('in', ['create', 'update', 'replace', 'delete']) | list | length > 0 -%}
package handlers

import (
	"bytes"
{%- if cloud_service == 'AWS Lambda' %}
	"context"
{%- endif %}
	"log/slog"
	"net/http"
{%- if cloud_service != 'AWS Lambda' %}
	"net/http/httptest"
{%- endif %}
{%- if need_write %}
	"strings"
{%- endif %}
	"testing"
	"time"
{%- if cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-lambda-go/events"
{%- endif %}

{%- if need_write %}

	"{{project_endpoint}}/models"
{%- endif %}

	"github.com/stretchr/testify/assert"
)

{%- if need_write %}

func TestWithUserID(t *testing.T) {
	cases := map[string]struct {
		value   string
		userID  string
		message string
	}{
		"present":        {value: "  alice  ", userID: "alice"},
		"absent":         {},
		"blank":          {value: "   "},
		"at the limit":   {value: strings.Repeat("é", MaxUserIDLength), userID: strings.Repeat("é", MaxUserIDLength)},
		"over the limit": {value: strings.Repeat("a", MaxUserIDLength+1), message: UserIDTooLongMessage},
	}

	for name, c := range cases {
		t.Run(name, func(t *testing.T) {
{%- if cloud_service == 'AWS Lambda' %}
			headers := map[string]string{}
			if c.value != "" {
				headers["x-user-id"] = c.value
			}
{%- else %}
			headers := http.Header{}
			if c.value != "" {
				headers.Set("x-user-id", c.value)
			}
{%- endif %}
			var got *string

			_, err := withUserID(headers, func(userID string) (any, error) {
				got = &userID
				return nil, nil
			})()

			if c.message != "" {
				var validation *models.ValidationError
				if assert.ErrorAs(t, err, &validation) {
					assert.Equal(t, c.message, validation.Message)
				}
				assert.Nil(t, got)
				return
			}
			assert.NoError(t, err)
			if assert.NotNil(t, got) {
				assert.Equal(t, c.userID, *got)
			}
		})
	}
}
{%- endif %}

func captureLogs(t *testing.T) *bytes.Buffer {
	t.Helper()
	var logs bytes.Buffer
	previous := slog.Default()
	slog.SetDefault(slog.New(slog.NewTextHandler(&logs, nil)))
	t.Cleanup(func() { slog.SetDefault(previous) })
	return &logs
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

func TestMiddleware_LogsRequestAndSetsDeadline(t *testing.T) {
	logs := captureLogs(t)
	var hasDeadline bool
	handler := Middleware(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		_, hasDeadline = r.Context().Deadline()
	}))

	handler.ServeHTTP(httptest.NewRecorder(), httptest.NewRequest(http.MethodGet, "/items", nil))

	assert.True(t, hasDeadline)
	assert.Contains(t, logs.String(), "method=GET path=/items")
}

func TestRecover_Panic_ReturnsJSON500(t *testing.T) {
	captureLogs(t)
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

func TestRouter_LogsRequest(t *testing.T) {
	logs := captureLogs(t)

	_, err := NewRouter().ServeRequest(context.Background(), events.APIGatewayProxyRequest{
		HTTPMethod: http.MethodGet,
		Resource:   "/unknown",
		Path:       "/unknown",
	})

	assert.NoError(t, err)
	assert.Contains(t, logs.String(), "method=GET path=/unknown")
}

func TestRouter_Panic_ReturnsJSON500(t *testing.T) {
	captureLogs(t)
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
