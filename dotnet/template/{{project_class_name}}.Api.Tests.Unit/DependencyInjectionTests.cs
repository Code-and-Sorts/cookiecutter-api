namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- elif cloud_service == 'GCP Cloud Function' %}
using Google.Api.Gax;
{%- else %}
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
{%- endif %}
using Microsoft.Extensions.Configuration;
using Xunit;

public class DependencyInjectionTests
{
    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void StoreNames_ListsEachStoreOnce()
    {
        var names = DependencyInjection.StoreNames(Configuration([]));

        Assert.Equal(
            new[]
            {
{%- for c in path_resources | unique(attribute='container') | map(attribute='container') | sort %}
                "{{ c }}",
{%- endfor %}
            },
            names);
    }
{%- if cloud_service == 'Azure Function App' %}

    private const string AzureConnection = "AccountEndpoint=https://account.documents.azure.com:443/;AccountKey=a2V5;";
    private const string HttpEmulatorConnection = "AccountEndpoint=http://localhost:8081/;AccountKey=a2V5;";
    private const string HttpsEmulatorConnection = "AccountEndpoint=https://localhost:8081/;AccountKey=a2V5;";

    [Fact]
    public void CreateCosmosClientOptions_WithoutTheEmulatorFlag_KeepsProductionSettings()
    {
        var options = DependencyInjection.CreateCosmosClientOptions(Configuration([]), AzureConnection);

        Assert.Equal(ConnectionMode.Direct, options.ConnectionMode);
        Assert.False(options.LimitToEndpoint);
        Assert.Null(options.ServerCertificateCustomValidationCallback);
        Assert.Equal(TimeSpan.FromSeconds(5), options.RequestTimeout);
    }

    [Fact]
    public void CreateCosmosClientOptions_EmulatorFlagFalse_KeepsProductionSettings()
    {
        var configuration = Configuration(new() { ["CosmosDbEmulator"] = "false", ["CosmosDbConnectionMode"] = "Gateway" });

        var options = DependencyInjection.CreateCosmosClientOptions(configuration, HttpsEmulatorConnection);

        Assert.Equal(ConnectionMode.Gateway, options.ConnectionMode);
        Assert.False(options.LimitToEndpoint);
        Assert.Null(options.ServerCertificateCustomValidationCallback);
    }

    [Fact]
    public void CreateCosmosClientOptions_Emulator_UsesGatewayModeAndTheConfiguredEndpointOnly()
    {
        var configuration = Configuration(new() { ["CosmosDbEmulator"] = "true", ["CosmosDbConnectionMode"] = "Direct" });

        var options = DependencyInjection.CreateCosmosClientOptions(configuration, HttpEmulatorConnection);

        Assert.Equal(ConnectionMode.Gateway, options.ConnectionMode);
        Assert.True(options.LimitToEndpoint);
        Assert.Null(options.ServerCertificateCustomValidationCallback);
    }

    [Fact]
    public void CreateCosmosClientOptions_HttpsEmulator_SkipsCertificateValidation()
    {
        var configuration = Configuration(new() { ["CosmosDbEmulator"] = "true" });

        var options = DependencyInjection.CreateCosmosClientOptions(configuration, HttpsEmulatorConnection);

        Assert.NotNull(options.ServerCertificateCustomValidationCallback);
        Assert.True(options.ServerCertificateCustomValidationCallback(null!, null!, default));
    }

    [Fact]
    public void CreateCosmosClientOptions_InvalidConnectionMode_Throws()
    {
        var configuration = Configuration(new() { ["CosmosDbConnectionMode"] = "Tcp" });

        Assert.Throws<InvalidOperationException>(() => DependencyInjection.CreateCosmosClientOptions(configuration, AzureConnection));
    }
{%- elif cloud_service == 'GCP Cloud Function' %}

    [Fact]
    public void CreateFirestoreDbBuilder_UsesTheEmulatorOnlyWhenItIsConfigured()
    {
        var builder = DependencyInjection.CreateFirestoreDbBuilder("demo-project", "(default)");

        Assert.Equal(EmulatorDetection.EmulatorOrProduction, builder.EmulatorDetection);
        Assert.Equal("demo-project", builder.ProjectId);
        Assert.Equal("(default)", builder.DatabaseId);
    }
{%- else %}

    [Theory]
    [InlineData(null, false)]
    [InlineData("http://localhost:8000", false)]
    [InlineData("", true)]
    public void CreateDynamoDbConfig_IgnoresOnlyAnEmptyEndpointOverride(string? endpoint, bool ignored)
    {
        var config = DependencyInjection.CreateDynamoDbConfig(Configuration(new() { ["AWS_ENDPOINT_URL_DYNAMODB"] = endpoint }));

        Assert.Equal(ignored, config.IgnoreConfiguredEndpointUrls);
        Assert.Equal(2, config.MaxErrorRetry);
    }

    [Fact]
    public void DynamoDbClient_ReadsTheEndpointOverrideFromTheEnvironment()
    {
        var previous = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL_DYNAMODB");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_DYNAMODB", "http://localhost:8000");
        try
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var config = DependencyInjection.CreateDynamoDbConfig(configuration);
            config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
            using var client = new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"), config);

            Assert.Equal("http://localhost:8000/", client.DetermineServiceOperationEndpoint(new ListTablesRequest()).URL);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_DYNAMODB", previous);
        }
    }
{%- endif %}
}
