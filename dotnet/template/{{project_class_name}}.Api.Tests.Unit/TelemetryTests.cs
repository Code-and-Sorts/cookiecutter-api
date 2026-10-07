{%- set azure = cloud_service == 'Azure Function App' -%}
{%- set aws = cloud_service == 'AWS Lambda' -%}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Xunit;

public class TelemetryTests
{
    private const string Otlp = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string OtlpLogs = "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT";
{%- if azure %}
    private const string AppInsights = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    private const string ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000";
{%- endif %}

    private static IConfiguration Configuration(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.Select(value => new KeyValuePair<string, string?>(value.Key, value.Value))).Build();

    public static TheoryData<string[], LogExporter[]> ExporterCases => new()
    {
        { [], [] },
        { [Otlp], [LogExporter.Otlp] },
        { [OtlpLogs], [LogExporter.Otlp] },
{%- if azure %}
        { [AppInsights], [LogExporter.AzureMonitor] },
        { [AppInsights, Otlp], [LogExporter.AzureMonitor, LogExporter.Otlp] },
{%- endif %}
    };

    [Theory]
    [MemberData(nameof(ExporterCases))]
    public void LogExporters_FollowTheSettings(string[] settings, LogExporter[] expected)
    {
        var configuration = Configuration(settings.Select(setting => (setting, (string?)Value(setting))).ToArray());

        Assert.Equal(expected, Telemetry.LogExporters(configuration));
    }

    [Fact]
    public void LogExporters_IgnoreEmptySettings()
    {
        Assert.Empty(Telemetry.LogExporters(Configuration((Otlp, ""), (OtlpLogs, null))));
    }

    [Fact]
    public void AddTelemetry_WithoutAnExporter_LeavesThePlatformConsoleAlone()
    {
        var services = new ServiceCollection().AddLogging().AddTelemetry(Configuration());
        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<LoggerProvider>());
    }

    [Theory]
    [InlineData(Otlp)]
{%- if azure %}
    [InlineData(AppInsights)]
{%- endif %}
    public void AddTelemetry_WithAnExporter_SendsILoggerThroughOpenTelemetry(string setting)
    {
        var configuration = Configuration((setting, Value(setting)));
        var services = new ServiceCollection().AddSingleton(configuration).AddLogging().AddTelemetry(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<LoggerProvider>());
        Assert.Contains(provider.GetServices<ILoggerProvider>(), logger => logger is OpenTelemetryLoggerProvider);
    }

    [Fact]
    public void AddTelemetry_ExportsAspNetCoreLogsFromWarningsOnly()
    {
        var configuration = Configuration((Otlp, Value(Otlp)));
        using var provider = new ServiceCollection().AddSingleton(configuration).AddTelemetry(configuration).BuildServiceProvider();
        var loggers = provider.GetRequiredService<ILoggerFactory>();

        Assert.False(loggers.CreateLogger("Microsoft.AspNetCore.Hosting.Diagnostics").IsEnabled(LogLevel.Information));
        Assert.True(loggers.CreateLogger("Microsoft.AspNetCore.Hosting.Diagnostics").IsEnabled(LogLevel.Warning));
        Assert.True(loggers.CreateLogger<TelemetryTests>().IsEnabled(LogLevel.Information));
    }

    [Fact]
    public async Task AddTelemetry_WithAnOtlpEndpoint_PostsTheRecordsOverHttp()
    {
        using var listener = new HttpListener();
        var endpoint = $"http://127.0.0.1:{FreePort()}";
        listener.Prefixes.Add($"{endpoint}/");
        listener.Start();
        var configuration = Configuration((Otlp, endpoint));
        await using var provider = new ServiceCollection().AddSingleton(configuration).AddLogging().AddTelemetry(configuration).BuildServiceProvider();
        var flush = Task.Run(() =>
        {
            provider.GetRequiredService<ILogger<TelemetryTests>>().LogError("failed");
            return provider.GetRequiredService<LoggerProvider>().ForceFlush();
        }, TestContext.Current.CancellationToken);

        var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        context.Response.Close();

        Assert.True(await flush);

        Assert.Equal("/v1/logs", context.Request.Url?.AbsolutePath);
        Assert.Equal("application/x-protobuf", context.Request.ContentType);
    }

    [Fact]
    public void UnexpectedError_IsAnErrorRecordWithTheExceptionInTheCurrentTrace()
    {
        var records = new List<LogRecord>();
        using var factory = LoggerFactory.Create(logging => logging.AddOpenTelemetry(options => options.AddInMemoryExporter(records)));
        var exception = new InvalidOperationException("Secret SDK diagnostics");
        using var activity = new Activity("request").Start();

        ErrorDetector.Classify(exception, factory.CreateLogger<TelemetryTests>());

        var record = Assert.Single(records);
        Assert.Equal(LogLevel.Error, record.LogLevel);
        Assert.Same(exception, record.Exception);
        Assert.Equal(activity.TraceId, record.TraceId);
        Assert.Equal(activity.SpanId, record.SpanId);
    }

    [Fact]
    public void Resource_NamesTheService()
    {
        Assert.Equal("{{ telemetry_resource['service.name'] }}", Telemetry.Resource["service.name"]);
        Assert.Equal("{{ telemetry_resource['cloud.provider'] }}", Telemetry.Resource["cloud.provider"]);
    }
{%- if aws %}

    [Theory]
    [InlineData("traceparent")]
    [InlineData("Traceparent")]
    public void StartRequestActivity_ContinuesTheCallersTrace(string header)
    {
        using var activity = Telemetry.StartRequestActivity(new Dictionary<string, string>
        {
            [header] = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
        });

        Assert.NotNull(activity);
        Assert.Same(activity, Activity.Current);
        Assert.Equal("4bf92f3577b34da6a3ce929d0e0e4736", activity.TraceId.ToHexString());
        Assert.Equal("00f067aa0ba902b7", activity.ParentSpanId.ToHexString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-traceparent")]
    public void StartRequestActivity_WithoutAValidTraceparent_StartsNothing(string? traceparent)
    {
        var headers = traceparent == null ? null : new Dictionary<string, string> { ["traceparent"] = traceparent };

        Assert.Null(Telemetry.StartRequestActivity(headers));
    }
{%- endif %}

    private static int FreePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        return ((IPEndPoint)socket.LocalEndpoint).Port;
    }

    private static string Value(string setting) =>
{%- if azure %}
        setting == AppInsights ? ConnectionString :
{%- endif %}
        "http://localhost:4318";
}
