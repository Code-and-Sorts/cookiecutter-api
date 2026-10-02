package utils

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

func captureLogs(t *testing.T) *bytes.Buffer {
	t.Helper()
	var logs bytes.Buffer
	previous := slog.Default()
	slog.SetDefault(slog.New(slog.NewTextHandler(&logs, nil)))
	t.Cleanup(func() { slog.SetDefault(previous) })
	return &logs
}

func TestErrorStatus_CancelledRequest_IsNotLoggedAsError(t *testing.T) {
	logs := captureLogs(t)
	ctx, cancel := context.WithCancel(context.Background())
	cancel()

	status, message := errorStatus(ctx, ctx.Err())

	assert.Equal(t, 500, status)
	assert.Equal(t, UnexpectedErrorMessage, message)
	assert.Contains(t, logs.String(), "level=INFO")
	assert.NotContains(t, logs.String(), "level=ERROR")
}

func TestErrorStatus_TimedOutDatabaseCall_IsLoggedAsError(t *testing.T) {
	logs := captureLogs(t)
	ctx, cancel := context.WithTimeout(context.Background(), 0)
	defer cancel()
	<-ctx.Done()

	status, message := errorStatus(ctx, fmt.Errorf("database call: %w", ctx.Err()))

	assert.Equal(t, 500, status)
	assert.Equal(t, UnexpectedErrorMessage, message)
	assert.Contains(t, logs.String(), "level=ERROR")
	assert.Contains(t, logs.String(), "deadline exceeded")
	assert.Contains(t, logs.String(), "stack=")
}

func decodeError(t *testing.T, w *httptest.ResponseRecorder) string {
	t.Helper()
	assert.Equal(t, "application/json", w.Header().Get("Content-Type"))
	var baseError models.BaseError
	assert.NoError(t, json.NewDecoder(w.Body).Decode(&baseError))
	return baseError.ErrorMessage
}

func TestDetectError_WithNotFoundError_Returns404(t *testing.T) {
	w := httptest.NewRecorder()

	DetectError(context.Background(), w, fmt.Errorf("wrapped: %w", models.NewNotFoundError("Item", "abc")))

	assert.Equal(t, http.StatusNotFound, w.Code)
	assert.Equal(t, "Item with id abc was not found.", decodeError(t, w))
}

func TestDetectError_WithValidationError_Returns400(t *testing.T) {
	logs := captureLogs(t)
	w := httptest.NewRecorder()

	DetectError(context.Background(), w, &models.ValidationError{Message: "Name is required."})

	assert.Equal(t, http.StatusBadRequest, w.Code)
	assert.Equal(t, "Name is required.", decodeError(t, w))
	assert.Empty(t, logs.String())
}

func TestDetectError_WithGenericError_Returns500WithoutDetails(t *testing.T) {
	logs := captureLogs(t)
	w := httptest.NewRecorder()

	DetectError(context.Background(), w, errors.New("Mock exception"))

	assert.Equal(t, http.StatusInternalServerError, w.Code)
	assert.Equal(t, UnexpectedErrorMessage, decodeError(t, w))
	assert.Contains(t, logs.String(), "level=ERROR")
	assert.Contains(t, logs.String(), "Mock exception")
	assert.Contains(t, logs.String(), "stack=")
}

func TestWriteJSON_LogsEncodingFailure(t *testing.T) {
	logs := captureLogs(t)
	w := httptest.NewRecorder()

	WriteJSON(w, http.StatusOK, make(chan int))

	assert.Contains(t, logs.String(), "Failed to write response")
}
