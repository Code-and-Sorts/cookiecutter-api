package handlers

import (
	"bytes"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
	"time"

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
			headers := http.Header{}
			if c.value != "" {
				headers.Set("x-user-id", c.value)
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
