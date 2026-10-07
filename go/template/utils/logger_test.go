package utils

import (
	"bytes"
	"context"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"strings"
	"sync"
	"testing"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"go.opentelemetry.io/contrib/bridges/otelslog"
	"go.opentelemetry.io/otel/attribute"
	sdklog "go.opentelemetry.io/otel/sdk/log"
	"go.opentelemetry.io/otel/trace"
)

var testSpanContext = trace.NewSpanContext(trace.SpanContextConfig{
	TraceID:    trace.TraceID{0x4b, 0xf9, 0x2f, 0x35, 0x77, 0xb3, 0x4d, 0xa6, 0xa3, 0xce, 0x92, 0x9d, 0x0e, 0x0e, 0x47, 0x36},
	SpanID:     trace.SpanID{0x00, 0xf0, 0x67, 0xaa, 0x0b, 0xa9, 0x02, 0xb7},
	TraceFlags: trace.FlagsSampled,
	Remote:     true,
})

type memoryExporter struct {
	mu      sync.Mutex
	records []sdklog.Record
}

func (e *memoryExporter) Export(_ context.Context, records []sdklog.Record) error {
	e.mu.Lock()
	defer e.mu.Unlock()
	for _, record := range records {
		e.records = append(e.records, record.Clone())
	}
	return nil
}

func (e *memoryExporter) Shutdown(context.Context) error   { return nil }
func (e *memoryExporter) ForceFlush(context.Context) error { return nil }

func recordAttributes(record sdklog.Record) map[string]string {
	attributes := map[string]string{}
	record.WalkAttributes(func(kv attribute.KeyValue) bool {
		attributes[string(kv.Key)] = kv.Value.String()
		return true
	})
	return attributes
}

func captureRecords(t *testing.T) *memoryExporter {
	t.Helper()
	exporter := &memoryExporter{}
	provider := sdklog.NewLoggerProvider(sdklog.WithProcessor(sdklog.NewSimpleProcessor(exporter)))
	previous := slog.Default()
	slog.SetDefault(otelslog.NewLogger(ScopeName, otelslog.WithLoggerProvider(provider)))
	t.Cleanup(func() { slog.SetDefault(previous) })
	return exporter
}

func consoleLogger(t *testing.T) (*slog.Logger, *bytes.Buffer, *bytes.Buffer) {
	t.Helper()
	t.Setenv("OTEL_EXPORTER_OTLP_ENDPOINT", "")
	t.Setenv("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", "")
	var out, errOut bytes.Buffer
	provider, err := NewLoggerProvider(context.Background(), &out, &errOut)
	require.NoError(t, err)
	return otelslog.NewLogger(ScopeName, otelslog.WithLoggerProvider(provider)), &out, &errOut
}

func TestNewLoggerProvider_WithoutAnEndpoint_WritesOneJSONLinePerRecord(t *testing.T) {
	logger, out, errOut := consoleLogger(t)

	logger.Info("served", "status", 200)
	logger.Warn("slow")
	logger.Error("failed", "status", 500)

	assert.Equal(t, 2, strings.Count(out.String(), "\n"))
	assert.Contains(t, out.String(), `"Value":"served"`)
	assert.Contains(t, out.String(), `"Value":"slow"`)
	assert.NotContains(t, out.String(), `"Value":"failed"`)
	assert.Equal(t, 1, strings.Count(errOut.String(), "\n"))
	assert.Contains(t, errOut.String(), `"Value":"failed"`)
	assert.Contains(t, errOut.String(), `"SeverityText":"ERROR"`)
	assert.NotContains(t, errOut.String(), `"Value":"served"`)
}

func TestNewLoggerProvider_DescribesTheService(t *testing.T) {
	logger, out, _ := consoleLogger(t)

	logger.Info("served")

{%- for key, value in telemetry_resource | dictsort %}
	assert.Contains(t, out.String(), `{"Key":"{{ key }}","Value":{"Type":"STRING","Value":"{{ value }}"}}`)
{%- endfor %}
	assert.Contains(t, out.String(), `"Key":"process.runtime.name"`)
}

func TestNewLoggerProvider_EnvironmentOverridesTheServiceName(t *testing.T) {
	t.Setenv("OTEL_SERVICE_NAME", "renamed")
	t.Setenv("OTEL_RESOURCE_ATTRIBUTES", "deployment.environment.name=dev")
	logger, out, _ := consoleLogger(t)

	logger.Info("served")

	assert.Contains(t, out.String(), `{"Key":"service.name","Value":{"Type":"STRING","Value":"renamed"}}`)
	assert.Contains(t, out.String(), `{"Key":"deployment.environment.name","Value":{"Type":"STRING","Value":"dev"}}`)
}

func TestNewLoggerProvider_RecordsCarryTheTraceContext(t *testing.T) {
	logger, out, _ := consoleLogger(t)

	logger.InfoContext(trace.ContextWithRemoteSpanContext(context.Background(), testSpanContext), "served")

	assert.Contains(t, out.String(), testSpanContext.TraceID().String())
	assert.Contains(t, out.String(), testSpanContext.SpanID().String())
}

func TestNewLoggerProvider_WritesRecordsLoggedAfterTheRequestDeadline(t *testing.T) {
	logger, _, errOut := consoleLogger(t)
	ctx, cancel := context.WithCancel(trace.ContextWithRemoteSpanContext(context.Background(), testSpanContext))
	cancel()

	logger.ErrorContext(ctx, "failed")

	assert.Contains(t, errOut.String(), `"Value":"failed"`)
	assert.Contains(t, errOut.String(), testSpanContext.TraceID().String())
}

func TestNewLoggerProvider_WithAnEndpoint_ExportsOverOTLP(t *testing.T) {
	for _, variable := range []string{"OTEL_EXPORTER_OTLP_ENDPOINT", "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"} {
		t.Run(variable, func(t *testing.T) {
			paths := make(chan string, 1)
			server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
				paths <- r.URL.Path
			}))
			defer server.Close()
			endpoint := server.URL
			if variable == "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT" {
				endpoint += "/v1/logs"
			}
			t.Setenv("OTEL_EXPORTER_OTLP_ENDPOINT", "")
			t.Setenv("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", "")
			t.Setenv(variable, endpoint)
			var out, errOut bytes.Buffer
			provider, err := NewLoggerProvider(context.Background(), &out, &errOut)
			require.NoError(t, err)

			otelslog.NewLogger(ScopeName, otelslog.WithLoggerProvider(provider)).Error("failed")
			require.NoError(t, provider.ForceFlush(context.Background()))

			assert.Equal(t, "/v1/logs", <-paths)
			assert.Empty(t, out.String())
			assert.Empty(t, errOut.String())
			assert.NoError(t, provider.Shutdown(context.Background()))
		})
	}
}

func TestSetupLogging_SendsSlogThroughOpenTelemetry(t *testing.T) {
	previous := slog.Default()
	t.Cleanup(func() { slog.SetDefault(previous) })

	provider, err := SetupLogging(context.Background())

	require.NoError(t, err)
	assert.NotNil(t, provider)
	assert.NotSame(t, previous, slog.Default())
}
