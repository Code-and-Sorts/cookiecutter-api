{%- set azure = cloud_service == 'Azure Function App' -%}
{%- set aws = cloud_service == 'AWS Lambda' -%}
namespace {{project_class_name}}.Api;

using System;
using System.Collections.Generic;
{%- if aws %}
using System.Diagnostics;
using System.Linq;
{%- endif %}
{%- if azure %}
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
{%- endif %}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

public enum LogExporter
{
{%- if azure %}
    AzureMonitor,
{%- endif %}
    Otlp,
}

public static class Telemetry
{
    public static readonly IReadOnlyDictionary<string, object> Resource = new Dictionary<string, object>
    {
{%- for key, value in telemetry_resource | dictsort %}
        ["{{ key }}"] = "{{ value }}",
{%- endfor %}
    };

    public static IReadOnlyList<LogExporter> LogExporters(IConfiguration configuration)
    {
        var exporters = new List<LogExporter>();
{%- if azure %}
        if (!string.IsNullOrEmpty(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            exporters.Add(LogExporter.AzureMonitor);
        }
{%- endif %}
        if (!string.IsNullOrEmpty(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]) || !string.IsNullOrEmpty(configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"]))
        {
            exporters.Add(LogExporter.Otlp);
        }
        return exporters;
    }

    // Without an exporter the platform's own console logging is all there is to do.
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var exporters = LogExporters(configuration);
        if (exporters.Count == 0)
        {
            return services;
        }

        {% if azure %}var telemetry = {% endif %}services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.Clear().AddAttributes(Resource).AddTelemetrySdk().AddEnvironmentVariableDetector())
            .WithLogging(logging =>
            {
{%- if azure %}
                if (exporters.Contains(LogExporter.AzureMonitor))
                {
                    logging.AddAzureMonitorLogExporter(options => options.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
                }
{%- endif %}
                if (exporters.Contains(LogExporter.Otlp))
                {
                    logging.AddOtlpExporter((exporter, processor) =>
                    {
                        // The other languages' exporters default to OTLP over HTTP; .NET's defaults to gRPC.
                        if (string.IsNullOrEmpty(configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]) && string.IsNullOrEmpty(configuration["OTEL_EXPORTER_OTLP_LOGS_PROTOCOL"]))
                        {
                            exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                        }
{%- if aws %}
                        // Lambda freezes the process once a handler returns, so records are sent as they are written.
                        processor.ExportProcessorType = ExportProcessorType.Simple;
{%- endif %}
                    });
                }
            });
        // ASP.NET Core logs every request at Information, and the platform records requests already.
        services.AddLogging(logging => logging.AddFilter<OpenTelemetryLoggerProvider>("Microsoft.AspNetCore", LogLevel.Warning));
{%- if azure %}
        // Runs each invocation in the trace the host started and stops the host exporting the worker's logs again.
        telemetry.UseFunctionsWorkerDefaults();
{%- endif %}
        return services;
    }
{%- if aws %}

    // Logs carry the caller's trace and span ids when the request has a traceparent header, in any casing.
    public static Activity? StartRequestActivity(IDictionary<string, string>? headers)
    {
        string? Header(string name) => headers?.FirstOrDefault(header => string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase)).Value;

        if (!ActivityContext.TryParse(Header("traceparent"), Header("tracestate"), out var parent))
        {
            return null;
        }
        return new Activity("{{ project_endpoint }}").SetParentId(parent.TraceId, parent.SpanId, parent.TraceFlags).Start();
    }
{%- endif %}
}
