package handlers

import (
	"bytes"
	"context"
	"log/slog"
	"net/http"
	"strings"
	"testing"
	"time"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

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
			headers := map[string]string{}
			if c.value != "" {
				headers["x-user-id"] = c.value
			}
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

func captureLogs(t *testing.T) *bytes.Buffer {
	t.Helper()
	var logs bytes.Buffer
	previous := slog.Default()
	slog.SetDefault(slog.New(slog.NewTextHandler(&logs, nil)))
	t.Cleanup(func() { slog.SetDefault(previous) })
	return &logs
}

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
