package utils

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
{%- if cloud_service != 'AWS Lambda' %}
	"net/http"
	"net/http/httptest"
{%- endif %}
	"testing"

	"{{project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"go.opentelemetry.io/otel/log"
	"go.opentelemetry.io/otel/trace"
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
	assert.Contains(t, logs.String(), "exception.stacktrace=")
}

func TestErrorStatus_UnexpectedError_EmitsAnExceptionRecordInTheTrace(t *testing.T) {
	records := captureRecords(t)
	ctx := trace.ContextWithRemoteSpanContext(context.Background(), testSpanContext)

	status, message := errorStatus(ctx, errors.New("secret detail"))

	assert.Equal(t, 500, status)
	assert.Equal(t, UnexpectedErrorMessage, message)
	require.Len(t, records.records, 1)
	record := records.records[0]
	assert.Equal(t, log.SeverityError, record.Severity())
	assert.Equal(t, testSpanContext.TraceID(), record.TraceID())
	assert.Equal(t, testSpanContext.SpanID(), record.SpanID())
	attributes := recordAttributes(record)
	assert.Equal(t, "*errors.errorString", attributes["exception.type"])
	assert.Equal(t, "secret detail", attributes["exception.message"])
	assert.Contains(t, attributes["exception.stacktrace"], "utils.LogUnexpected")
}
{%- if cloud_service != 'AWS Lambda' %}

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
	assert.Contains(t, logs.String(), "exception.stacktrace=")
}

func TestWriteJSON_LogsEncodingFailure(t *testing.T) {
	logs := captureLogs(t)
	w := httptest.NewRecorder()

	WriteJSON(w, http.StatusOK, make(chan int))

	assert.Contains(t, logs.String(), "Failed to write response")
}
{%- else %}

func decodeError(t *testing.T, body string) string {
	t.Helper()
	var baseError models.BaseError
	assert.NoError(t, json.Unmarshal([]byte(body), &baseError))
	return baseError.ErrorMessage
}

func TestDetectError_WithNotFoundError_Returns404(t *testing.T) {
	resp := DetectError(context.Background(), fmt.Errorf("wrapped: %w", models.NewNotFoundError("Item", "abc")))

	assert.Equal(t, 404, resp.StatusCode)
	assert.Equal(t, "Item with id abc was not found.", decodeError(t, resp.Body))
}

func TestDetectError_WithValidationError_Returns400(t *testing.T) {
	logs := captureLogs(t)

	resp := DetectError(context.Background(), &models.ValidationError{Message: "Name is required."})

	assert.Equal(t, 400, resp.StatusCode)
	assert.Equal(t, "Name is required.", decodeError(t, resp.Body))
	assert.Empty(t, logs.String())
}

func TestDetectError_WithGenericError_Returns500WithoutDetails(t *testing.T) {
	logs := captureLogs(t)

	resp := DetectError(context.Background(), errors.New("Mock exception"))

	assert.Equal(t, 500, resp.StatusCode)
	assert.Equal(t, UnexpectedErrorMessage, decodeError(t, resp.Body))
	assert.Contains(t, logs.String(), "level=ERROR")
	assert.Contains(t, logs.String(), "Mock exception")
	assert.Contains(t, logs.String(), "exception.stacktrace=")
}

func TestGenerateErrorResponse_ReturnsJSONErrorBody(t *testing.T) {
	resp := GenerateErrorResponse(NotFoundMessage, 404)

	assert.Equal(t, 404, resp.StatusCode)
	assert.Equal(t, "application/json", resp.Headers["Content-Type"])
	assert.JSONEq(t, `{"errorMessage": "Not found."}`, resp.Body)
}

func TestJSONResponse_EncodesBody(t *testing.T) {
	resp := JSONResponse(200, []string{})

	assert.Equal(t, 200, resp.StatusCode)
	assert.Equal(t, "application/json", resp.Headers["Content-Type"])
	assert.Equal(t, `[]`, resp.Body)
}

func TestJSONResponse_Returns500_WhenBodyCannotBeEncoded(t *testing.T) {
	captureLogs(t)

	resp := JSONResponse(200, make(chan int))

	assert.Equal(t, 500, resp.StatusCode)
	assert.Equal(t, UnexpectedErrorMessage, decodeError(t, resp.Body))
}
{%- endif %}
