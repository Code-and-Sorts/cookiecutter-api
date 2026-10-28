package utils

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"os"
	"os/signal"
	"syscall"

	"go.opentelemetry.io/contrib/bridges/otelslog"
	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/attribute"
	"go.opentelemetry.io/otel/exporters/otlp/otlplog/otlploghttp"
	"go.opentelemetry.io/otel/exporters/stdout/stdoutlog"
	"go.opentelemetry.io/otel/log"
	sdklog "go.opentelemetry.io/otel/sdk/log"
	"go.opentelemetry.io/otel/sdk/resource"
)

const ScopeName = "{{ project_endpoint }}"

func SetupLogging(ctx context.Context) (*sdklog.LoggerProvider, error) {
	provider, err := NewLoggerProvider(ctx, os.Stdout, os.Stderr)
	if err != nil {
		return nil, err
	}
	slog.SetDefault(otelslog.NewLogger(ScopeName, otelslog.WithLoggerProvider(provider)))
	// Through slog, an export failure would be queued on the exporter that just failed.
	otel.SetErrorHandler(otel.ErrorHandlerFunc(func(err error) {
		fmt.Fprintln(os.Stderr, "OpenTelemetry:", err)
	}))
	return provider, nil
}

func NewLoggerProvider(ctx context.Context, out, errOut io.Writer) (*sdklog.LoggerProvider, error) {
	res, err := resource.New(ctx,
		resource.WithAttributes(
{%- for key, value in telemetry_resource | dictsort %}
			attribute.String("{{ key }}", "{{ value }}"),
{%- endfor %}
		),
		resource.WithFromEnv(),
		resource.WithTelemetrySDK(),
		resource.WithProcessRuntimeName(),
		resource.WithProcessRuntimeVersion(),
	)
	if err != nil {
		return nil, err
	}

	options := []sdklog.LoggerProviderOption{sdklog.WithResource(res)}
	if otlpConfigured() {
		exporter, err := otlploghttp.New(ctx)
		if err != nil {
			return nil, err
		}
		options = append(options, sdklog.WithProcessor(sdklog.NewBatchProcessor(exporter)))
	} else {
		stdout, err := stdoutlog.New(stdoutlog.WithWriter(out))
		if err != nil {
			return nil, err
		}
		stderr, err := stdoutlog.New(stdoutlog.WithWriter(errOut))
		if err != nil {
			return nil, err
		}
		options = append(options,
			sdklog.WithProcessor(severitySplit{Processor: sdklog.NewSimpleProcessor(stdout)}),
			sdklog.WithProcessor(severitySplit{Processor: sdklog.NewSimpleProcessor(stderr), errors: true}),
		)
	}
	return sdklog.NewLoggerProvider(options...), nil
}

func otlpConfigured() bool {
	return os.Getenv("OTEL_EXPORTER_OTLP_ENDPOINT") != "" || os.Getenv("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT") != ""
}

// Hosts such as Azure Functions treat stderr as failures, so only error records go there.
type severitySplit struct {
	sdklog.Processor
	errors bool
}

func (p severitySplit) OnEmit(ctx context.Context, record *sdklog.Record) error {
	if (record.Severity() >= log.SeverityError) != p.errors {
		return nil
	}
	// The console exporter refuses a cancelled context, which would drop the errors logged after a request deadline.
	return p.Processor.OnEmit(context.WithoutCancel(ctx), record)
}

func ShutdownOnSignal(provider *sdklog.LoggerProvider) {
	signals := make(chan os.Signal, 1)
	signal.Notify(signals, os.Interrupt, syscall.SIGTERM)
	go func() {
		received := <-signals
		_ = provider.Shutdown(context.Background())
		// Sent again with the default handling restored, so the process ends as the platform expects.
		signal.Stop(signals)
		if process, err := os.FindProcess(os.Getpid()); err != nil || process.Signal(received) != nil {
			os.Exit(1)
		}
	}()
}
